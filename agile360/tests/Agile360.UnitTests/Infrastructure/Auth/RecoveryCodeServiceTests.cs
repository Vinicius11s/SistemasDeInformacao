using Agile360.Domain.Entities;
using Agile360.Infrastructure.Auth;
using Agile360.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Agile360.UnitTests.Infrastructure.Auth;

/// <summary>
/// Testes unitários do <see cref="RecoveryCodeService"/> cobrindo os 5 cenários críticos do QA.
///
/// Estratégia: SQLite In-Memory via EF Core (arquivo ":memory:" com conexão persistida).
/// SQLite suporta ExecuteDeleteAsync / ExecuteUpdateAsync — necessário para testar o B7a atômico.
/// Cada teste recebe um DbContext isolado (conexão separada) para evitar interferências.
/// </summary>
public class RecoveryCodeServiceTests : IDisposable
{
    // ── Infraestrutura de teste ───────────────────────────────────────────────

    private readonly SqliteConnection _connection;
    private readonly Agile360DbContext _db;
    private readonly RecoveryCodeService _sut;
    private readonly Guid _advogadoId = Guid.NewGuid();

    public RecoveryCodeServiceTests()
    {
        // SQLite in-memory: mantém a conexão aberta para que o banco persista durante o teste
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // UseSnakeCaseNamingConvention() alinha o mapeamento do EF Core com o schema real do banco.
        // Sem ele, propriedades como "FotoUrl" → coluna "FotoUrl" (PascalCase) em vez de "foto_url",
        // causando "no such column" no SQLite.
        var options = new DbContextOptionsBuilder<Agile360DbContext>()
            .UseSqlite(_connection)
            .UseSnakeCaseNamingConvention()
            .Options;

        _db  = new Agile360DbContext(options);

        // EnsureCreated falha com índices filtrados PostgreSQL-specific (ex: WHERE revoked_at IS NULL).
        // Criamos MANUALMENTE apenas as tabelas necessárias para estes testes.
        // Schema snake_case: espelha o que UseSnakeCaseNamingConvention + AdvogadoConfiguration geram.
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS advogado (
                id                  TEXT NOT NULL PRIMARY KEY,
                email               TEXT NOT NULL,
                nome                TEXT NOT NULL,
                role                TEXT,
                oab                 TEXT,
                telefone            TEXT,
                whatsapp_id         TEXT,
                foto_url            TEXT,
                nome_escritorio     TEXT,
                cpf_cnpj            TEXT,
                cidade              TEXT,
                estado              TEXT,
                plano               TEXT,
                status_assinatura   TEXT,
                data_expiracao      TEXT,
                stripe_customer_id  TEXT,
                password_hash       TEXT,
                mfa_enabled         INTEGER NOT NULL DEFAULT 0,
                mfa_secret          TEXT,
                mfa_pending_secret  TEXT,
                is_active           INTEGER NOT NULL DEFAULT 1,
                created_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at          TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            CREATE TABLE IF NOT EXISTS advogado_recovery_codes (
                id          TEXT NOT NULL PRIMARY KEY,
                advogado_id TEXT NOT NULL,
                code_hash   TEXT NOT NULL,
                is_used     INTEGER NOT NULL DEFAULT 0,
                used_at     TEXT,
                created_at  TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (advogado_id) REFERENCES advogado(id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS ix_recovery_codes_advogado_active
                ON advogado_recovery_codes (advogado_id, is_used);
            """;
        cmd.ExecuteNonQuery();

        _sut = new RecoveryCodeService(_db);

        // Seed: advogado mínimo para satisfazer FK
        _db.Advogados.Add(new Advogado
        {
            Id    = _advogadoId,
            Email = "advogado@test.com",
            Nome  = "Advogado Teste",
        });
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // ════════════════════════════════════════════════════════════════════════
    // Q1 — Testes unitários: geração, consumo, invalidação
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GenerateCodesAsync_DeveRetornar10Codigos()
    {
        // Act
        var codes = await _sut.GenerateCodesAsync(_advogadoId);

        // Assert
        codes.Should().HaveCount(10);
    }

    [Fact]
    public async Task GenerateCodesAsync_TodosCodigosDevemSerUnicos()
    {
        // Act
        var codes = await _sut.GenerateCodesAsync(_advogadoId);

        // Assert — Q2: unicidade
        codes.Distinct().Should().HaveCount(10,
            "cada código deve ser único — duplicatas indicam falha de entropia ou HashSet");
    }

    [Fact]
    public async Task GenerateCodesAsync_CodigosDevemTerFormatoXXXX_XXXX()
    {
        // Act
        var codes = await _sut.GenerateCodesAsync(_advogadoId);

        // Assert — 9 chars visíveis: 4 alfanum + hífen + 4 alfanum
        codes.Should().AllSatisfy(code =>
        {
            code.Should().MatchRegex(@"^[A-Z2-9]{4}-[A-Z2-9]{4}$",
                "formato esperado: XXXX-XXXX com alfabeto sem 0/O/1/I/L");
        });
    }

    [Fact]
    public async Task GenerateCodesAsync_DevePersistir10HashesNoBanco()
    {
        // Act
        await _sut.GenerateCodesAsync(_advogadoId);

        // Assert
        var count = await _db.RecoveryCodes
            .CountAsync(c => c.AdvogadoId == _advogadoId && !c.IsUsed);

        count.Should().Be(10);
    }

    [Fact]
    public async Task GenerateCodesAsync_PlaintextNuncaDeveSerHashNoRetorno()
    {
        // Act
        var codes = await _sut.GenerateCodesAsync(_advogadoId);

        // Assert — texto limpo não deve começar com "$2" (prefixo BCrypt)
        codes.Should().AllSatisfy(code =>
            code.Should().NotStartWith("$2",
                "o plaintext nunca deve ser o hash BCrypt — o serviço deve retornar o código original"));
    }

    [Fact]
    public async Task GenerateCodesAsync_RegeneracaoDeveInvalidarCodigosAnteriores()
    {
        // Arrange — geração inicial
        await _sut.GenerateCodesAsync(_advogadoId);
        var countBefore = await _db.RecoveryCodes.CountAsync(c => c.AdvogadoId == _advogadoId);
        countBefore.Should().Be(10);

        // Act — regeneração
        await _sut.GenerateCodesAsync(_advogadoId);

        // Assert — apenas 10 registros no total (os anteriores foram deletados — hard delete)
        var countAfter = await _db.RecoveryCodes.CountAsync(c => c.AdvogadoId == _advogadoId);
        countAfter.Should().Be(10, "regeneração deve apagar os anteriores, não acumular");
    }

    // ════════════════════════════════════════════════════════════════════════
    // Q3 — Burn-after-use: código consumido retorna false na segunda chamada
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ValidateAndConsumeAsync_PrimeiraUtilizacaoDeveRetornarTrue()
    {
        // Arrange
        var codes = await _sut.GenerateCodesAsync(_advogadoId);
        var code  = codes[0];

        // Act
        var result = await _sut.ValidateAndConsumeAsync(_advogadoId, code);

        // Assert
        result.Should().BeTrue("código válido deve ser aceito na primeira tentativa");
    }

    [Fact]
    public async Task ValidateAndConsumeAsync_BurnAfterUse_SegundaChamadaDeveRetornarFalse()
    {
        // Arrange — Q3: burn-after-use
        var codes = await _sut.GenerateCodesAsync(_advogadoId);
        var code  = codes[0];

        // Act — primeiro uso
        var first = await _sut.ValidateAndConsumeAsync(_advogadoId, code);

        // Act — segundo uso do mesmo código
        var second = await _sut.ValidateAndConsumeAsync(_advogadoId, code);

        // Assert
        first.Should().BeTrue("primeiro uso deve ser aceito");
        second.Should().BeFalse("código já consumido deve ser rejeitado (burn-after-use)");
    }

    [Fact]
    public async Task ValidateAndConsumeAsync_BurnAfterUse_CodigoUsadoDeveMarcarIsUsedTrue()
    {
        // Arrange
        var codes = await _sut.GenerateCodesAsync(_advogadoId);
        var code  = codes[0];

        // Act
        await _sut.ValidateAndConsumeAsync(_advogadoId, code);

        // Assert — todos os registros com is_used = true devem ter used_at preenchido
        var usedRecord = await _db.RecoveryCodes
            .FirstOrDefaultAsync(c => c.AdvogadoId == _advogadoId && c.IsUsed);

        usedRecord.Should().NotBeNull();
        usedRecord!.UsedAt.Should().NotBeNull("used_at deve ser preenchido ao consumir o código");
        usedRecord.UsedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ValidateAndConsumeAsync_CodigoInvalidoDeveRetornarFalse()
    {
        // Arrange
        await _sut.GenerateCodesAsync(_advogadoId);

        // Act — código totalmente inválido
        var result = await _sut.ValidateAndConsumeAsync(_advogadoId, "ZZZZ-9999");

        // Assert
        result.Should().BeFalse("código inexistente deve ser rejeitado");
    }

    [Fact]
    public async Task ValidateAndConsumeAsync_DeveAceitarCodigoSemHifen()
    {
        // Arrange — testa tolerância de formato (usuário pode digitar sem hífen)
        var codes = await _sut.GenerateCodesAsync(_advogadoId);
        var codeWithoutHyphen = codes[0].Replace("-", "");

        // Act
        var result = await _sut.ValidateAndConsumeAsync(_advogadoId, codeWithoutHyphen);

        // Assert
        result.Should().BeTrue("o serviço deve normalizar o input removendo hífens");
    }

    [Fact]
    public async Task ValidateAndConsumeAsync_DeveAceitarCodigoEmMinusculas()
    {
        // Arrange — testa tolerância de case
        var codes = await _sut.GenerateCodesAsync(_advogadoId);
        var codeLower = codes[0].ToLowerInvariant();

        // Act
        var result = await _sut.ValidateAndConsumeAsync(_advogadoId, codeLower);

        // Assert
        result.Should().BeTrue("o serviço deve normalizar o input para uppercase");
    }

    // ════════════════════════════════════════════════════════════════════════
    // Q5 — Timing: diferença entre código inválido e inexistente deve ser mínima
    // (BCrypt é constant-time — este teste valida o comportamento, não o tempo exato)
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ValidateAndConsumeAsync_CodigoInexistenteNaoDeveDiferenciarMensagemDeInvalido()
    {
        // Arrange
        await _sut.GenerateCodesAsync(_advogadoId);

        // Act — código de advogado diferente (inexistente neste contexto)
        var result = await _sut.ValidateAndConsumeAsync(_advogadoId, "AAAA-0000");

        // Assert — mesmo resultado false (sem diferença semântica no retorno)
        result.Should().BeFalse("não deve haver diferença de retorno entre código inválido e inexistente");
    }

    // ════════════════════════════════════════════════════════════════════════
    // GetRemainingCountAsync
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetRemainingCountAsync_ApesDeGerarDeveRetornar10()
    {
        // Arrange
        await _sut.GenerateCodesAsync(_advogadoId);

        // Act
        var count = await _sut.GetRemainingCountAsync(_advogadoId);

        // Assert
        count.Should().Be(10);
    }

    [Fact]
    public async Task GetRemainingCountAsync_ApesUsarUmDeveRetornar9()
    {
        // Arrange
        var codes = await _sut.GenerateCodesAsync(_advogadoId);
        await _sut.ValidateAndConsumeAsync(_advogadoId, codes[0]);

        // Act
        var count = await _sut.GetRemainingCountAsync(_advogadoId);

        // Assert
        count.Should().Be(9);
    }

    [Fact]
    public async Task GetRemainingCountAsync_SemCodigosDeveRetornar0()
    {
        // Act — sem gerar nenhum código
        var count = await _sut.GetRemainingCountAsync(_advogadoId);

        // Assert
        count.Should().Be(0);
    }

    // ════════════════════════════════════════════════════════════════════════
    // DeleteAllAsync — limpeza ao desativar MFA
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteAllAsync_DeveDeletarTodosOsCodigosDoAdvogado()
    {
        // Arrange
        await _sut.GenerateCodesAsync(_advogadoId);

        // Act
        await _sut.DeleteAllAsync(_advogadoId);

        // Assert
        var count = await _db.RecoveryCodes.CountAsync(c => c.AdvogadoId == _advogadoId);
        count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAllAsync_NaoDeveAfetar_CodigosDeOutroAdvogado()
    {
        // Arrange
        var outroAdvogadoId = Guid.NewGuid();
        _db.Advogados.Add(new Advogado
        {
            Id    = outroAdvogadoId,
            Email = "outro@test.com",
            Nome  = "Outro Advogado",
        });
        await _db.SaveChangesAsync();

        await _sut.GenerateCodesAsync(_advogadoId);
        await _sut.GenerateCodesAsync(outroAdvogadoId);

        // Act — deleta apenas do primeiro advogado
        await _sut.DeleteAllAsync(_advogadoId);

        // Assert — o segundo advogado não deve ser afetado
        var countOutro = await _db.RecoveryCodes.CountAsync(c => c.AdvogadoId == outroAdvogadoId);
        countOutro.Should().Be(10, "os códigos de outro advogado não devem ser deletados");
    }

    // ════════════════════════════════════════════════════════════════════════
    // Q2 — Teste de unicidade: 10 códigos gerados sempre distintos (100 iterações)
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GenerateCodesAsync_UniqueAcross100Iteracoes()
    {
        // Arrange — coleta todos os códigos gerados em 100 chamadas
        var allCodes = new HashSet<string>();

        for (int i = 0; i < 100; i++)
        {
            var codes = await _sut.GenerateCodesAsync(_advogadoId);
            foreach (var code in codes)
            {
                allCodes.Add(code);
            }
        }

        // Assert — 100 × 10 = 1000 chamadas; com 40 bits de entropia, colisão é improvável
        // Aceita-se até 5% de colisão natural (50/1000) para evitar flakiness
        allCodes.Count.Should().BeGreaterThan(950,
            "em 1000 códigos gerados, a entropia de 40 bits garante alta diversidade");
    }
}

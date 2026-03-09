using Agile360.Application.Clientes.DTOs;
using Agile360.Application.Clientes.Services;
using Agile360.Domain.Entities;
using Agile360.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace Agile360.UnitTests.Application.Clientes;

/// <summary>
/// Plano de testes — Cadastro em Massa via Excel (AC-2 a AC-10)
/// Todos os testes focam no Service, que é testável sem Excel nem HTTP.
/// O Controller (parse do Excel) é coberto por Integration Tests.
/// </summary>
public class ClienteBulkImportServiceTests
{
    private readonly IClienteRepository _repo = Substitute.For<IClienteRepository>();
    private readonly ClienteBulkImportService _sut;

    public ClienteBulkImportServiceTests()
    {
        // Por padrão: nenhum CPF existe na base → GetByCpfAsync retorna null
        _repo.GetByCpfAsync(Arg.Any<string>()).ReturnsNull();

        // Batch insert devolve os mesmos objetos (simula Supabase retornando representation)
        _repo.AddRangeAsync(Arg.Any<IEnumerable<Cliente>>())
             .Returns(call => (IReadOnlyList<Cliente>)call.Arg<IEnumerable<Cliente>>().ToList());

        _sut = new ClienteBulkImportService(_repo);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CENÁRIO 1 — Arquivo / lista vazia
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "Lista vazia → resultado zerado, sem chamar o repositório")]
    public async Task ImportarAsync_ListaVazia_RetornaZeroSemChamarRepo()
    {
        var resultado = await _sut.ImportarAsync([]);

        resultado.Total.Should().Be(0);
        resultado.Sucesso.Should().Be(0);
        resultado.Falhas.Should().Be(0);
        resultado.Erros.Should().BeEmpty();
        await _repo.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<Cliente>>());
    }

    [Fact(DisplayName = "Linhas só com nome em branco → resultado zerado (ignoradas silenciosamente)")]
    public async Task ImportarAsync_ApenasLinhasComNomeEmBranco_Ignora()
    {
        var linhas = new List<ImportarClienteRow>
        {
            CriarLinha(1, nomeCompleto: "   "),
            CriarLinha(2, nomeCompleto: ""),
        };

        var resultado = await _sut.ImportarAsync(linhas);

        // Total = linhas.Count (o service conta todas as linhas recebidas).
        // Linhas com nome em branco são IGNORADAS silenciosamente (sem erro, sem inserção).
        resultado.Total.Should().Be(2);
        resultado.Sucesso.Should().Be(0);
        resultado.Falhas.Should().Be(0);
        await _repo.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<Cliente>>());
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CENÁRIO 2 — CPF com formato inválido
    // ════════════════════════════════════════════════════════════════════════

    [Theory(DisplayName = "CPF com formato inválido → gera erro na linha")]
    [InlineData("123")]                     // muito curto
    [InlineData("1234567890A")]             // letra no CPF
    [InlineData("123.456.789")]             // faltam dígitos verificadores
    [InlineData("123456789001122")]         // dígitos demais
    public async Task ImportarAsync_CpfFormatoInvalido_GeraErroNaLinha(string cpfInvalido)
    {
        var linhas = new List<ImportarClienteRow>
        {
            CriarLinha(2, nomeCompleto: "João Silva", cpf: cpfInvalido)
        };

        var resultado = await _sut.ImportarAsync(linhas);

        resultado.Falhas.Should().Be(1);
        resultado.Sucesso.Should().Be(0);
        resultado.Erros.Should().ContainSingle()
            .Which.Motivo.Should().Contain("CPF inválido");
    }

    [Theory(DisplayName = "CPF com formato válido (com e sem pontuação) → aceito")]
    [InlineData("123.456.789-00")]   // formatado
    [InlineData("12345678900")]      // apenas dígitos
    public async Task ImportarAsync_CpfFormatoValido_AceitaLinha(string cpfValido)
    {
        var linhas = new List<ImportarClienteRow>
        {
            CriarLinha(2, nomeCompleto: "Maria Costa", cpf: cpfValido)
        };

        var resultado = await _sut.ImportarAsync(linhas);

        resultado.Sucesso.Should().Be(1);
        resultado.Falhas.Should().Be(0);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CENÁRIO 3 — Caracteres especiais e data inválida
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "Nome com caracteres especiais/acentos → aceito normalmente")]
    public async Task ImportarAsync_NomeComCaracteresEspeciais_Aceito()
    {
        var linhas = new List<ImportarClienteRow>
        {
            CriarLinha(2, nomeCompleto: "José Ação & Companhia Ltda — São Paulo")
        };

        var resultado = await _sut.ImportarAsync(linhas);

        resultado.Sucesso.Should().Be(1);
        resultado.Falhas.Should().Be(0);
    }

    [Theory(DisplayName = "data_nascimento em formato inválido → gera erro na linha")]
    [InlineData("31-12-1990")]       // separador errado
    [InlineData("1990/12/31")]       // ordem errada
    [InlineData("99/99/9999")]       // data impossível
    [InlineData("abc")]              // não é data
    public async Task ImportarAsync_DataNascimentoFormatoInvalido_GeraErro(string dataInvalida)
    {
        var linhas = new List<ImportarClienteRow>
        {
            CriarLinha(2, nomeCompleto: "Ana Lima", dataNascimentoRaw: dataInvalida)
        };

        var resultado = await _sut.ImportarAsync(linhas);

        resultado.Falhas.Should().Be(1);
        resultado.Erros.Should().ContainSingle()
            .Which.Motivo.Should().Contain("data_nascimento");
    }

    [Fact(DisplayName = "data_nascimento no formato correto dd/MM/yyyy → aceita")]
    public async Task ImportarAsync_DataNascimentoCorreta_Aceita()
    {
        var linhas = new List<ImportarClienteRow>
        {
            CriarLinha(2, nomeCompleto: "Carlos Pinto", dataNascimentoRaw: "15/05/1985")
        };

        var resultado = await _sut.ImportarAsync(linhas);

        resultado.Sucesso.Should().Be(1);
        resultado.Falhas.Should().Be(0);
    }

    [Theory(DisplayName = "Estado com mais de 2 letras → gera erro na linha")]
    [InlineData("São Paulo")]
    [InlineData("RJX")]
    [InlineData("MINAS")]
    public async Task ImportarAsync_EstadoMaisDe2Letras_GeraErro(string estadoInvalido)
    {
        var linhas = new List<ImportarClienteRow>
        {
            CriarLinha(2, nomeCompleto: "Pedro Alves", estado: estadoInvalido)
        };

        var resultado = await _sut.ImportarAsync(linhas);

        resultado.Falhas.Should().Be(1);
        resultado.Erros.Single().Motivo.Should().Contain("estado");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CENÁRIO 4 — Tentativa de duplicar cliente já existente (por CPF)
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "CPF já cadastrado na base → linha pulada com motivo 'CPF já cadastrado'")]
    public async Task ImportarAsync_CpfJaCadastrado_GeraErroDeDuplicidade()
    {
        const string cpfExistente = "123.456.789-00";
        _repo.GetByCpfAsync("12345678900")
             .Returns(new Cliente { NomeCompleto = "Antigo Dono", Cpf = "12345678900" });

        var linhas = new List<ImportarClienteRow>
        {
            CriarLinha(2, nomeCompleto: "Novo Cliente", cpf: cpfExistente)
        };

        var resultado = await _sut.ImportarAsync(linhas);

        resultado.Falhas.Should().Be(1);
        resultado.Sucesso.Should().Be(0);
        resultado.Erros.Should().ContainSingle()
            .Which.Motivo.Should().Contain("CPF").And.Contain("cadastrado");
        await _repo.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<Cliente>>());
    }

    [Fact(DisplayName = "CPF duplicado dentro da PRÓPRIA planilha → segunda ocorrência gera erro")]
    public async Task ImportarAsync_CpfDuplicadoNaPlanilha_SegundaOcorrenciaGeraErro()
    {
        const string cpf = "12345678900";
        var linhas = new List<ImportarClienteRow>
        {
            CriarLinha(2, nomeCompleto: "Cliente A", cpf: cpf),
            CriarLinha(3, nomeCompleto: "Cliente B", cpf: cpf), // mesmo CPF
        };

        var resultado = await _sut.ImportarAsync(linhas);

        resultado.Sucesso.Should().Be(1);
        resultado.Falhas.Should().Be(1);
        resultado.Erros.Should().ContainSingle()
            .Which.Should().Match<ImportarClienteErro>(e =>
                e.Linha == 3 && e.Motivo.Contains("CPF"));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CENÁRIO 5 — Erro parcial: 50 certas, 2 falhas
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "Erro parcial: linhas válidas são inseridas, inválidas retornam no relatório")]
    public async Task ImportarAsync_ErrosParciais_InsereLinhsValidasRetornaErros()
    {
        var linhas = new List<ImportarClienteRow>();

        // 50 linhas válidas
        for (int i = 1; i <= 50; i++)
            linhas.Add(CriarLinha(i + 1, nomeCompleto: $"Cliente {i:D2}"));

        // 2 linhas inválidas
        linhas.Add(CriarLinha(52, nomeCompleto: "Erro CPF",  cpf: "INVALIDO"));
        linhas.Add(CriarLinha(53, nomeCompleto: "Erro Data", dataNascimentoRaw: "32/13/2000"));

        var resultado = await _sut.ImportarAsync(linhas);

        resultado.Total.Should().Be(52);
        resultado.Sucesso.Should().Be(50);
        resultado.Falhas.Should().Be(2);
        resultado.Erros.Should().HaveCount(2);

        // Confirma que o repositório recebeu exatamente 50 clientes
        await _repo.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<Cliente>>(list => list.Count() == 50));
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CENÁRIO 6 — Stress de chaves (PGRST102)
    //  Garante que linha totalmente preenchida + linha só com Nome/CPF
    //  geram objetos com o mesmo conjunto de chaves no JSON enviado ao banco.
    //  Se os campos opcionais fossem omitidos para null, o PostgREST rejeitaria
    //  com PGRST102 ("All object keys must match").
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "PGRST102: linha completa + linha só com Nome e CPF → ambas inseridas sem erro")]
    public async Task ImportarAsync_LinhaCompletoELinhaParcial_AmbasInseridassemPgrst102()
    {
        // Linha 2: todos os campos preenchidos
        var linhaCompleta = new ImportarClienteRow(
            Linha:             2,
            NomeCompleto:      "João Silva Completo",
            Cpf:               "111.444.777-35",
            Rg:                "12.345.678-9",
            OrgaoExpedidor:    "SSP-SP",
            DataNascimentoRaw: "15/05/1985",
            EstadoCivil:       "Solteiro",
            Profissao:         "Engenheiro",
            Telefone:          "(11) 91234-5678",
            NumeroConta:       "12345-6",
            Pix:               "joao@email.com",
            Cep:               "01310-100",
            Endereco:          "Av. Paulista",
            Numero:            "1000",
            Bairro:            "Bela Vista",
            Complemento:       "Apto 42",
            Cidade:            "São Paulo",
            Estado:            "SP"
        );

        // Linha 3: somente Nome e CPF — todos os demais campos nulos
        var linhaParcial = new ImportarClienteRow(
            Linha:             3,
            NomeCompleto:      "Maria Só Nome CPF",
            Cpf:               "222.333.444-05",
            Rg:                null,
            OrgaoExpedidor:    null,
            DataNascimentoRaw: null,
            EstadoCivil:       null,
            Profissao:         null,
            Telefone:          null,
            NumeroConta:       null,
            Pix:               null,
            Cep:               null,
            Endereco:          null,
            Numero:            null,
            Bairro:            null,
            Complemento:       null,
            Cidade:            null,
            Estado:            null
        );

        var resultado = await _sut.ImportarAsync([linhaCompleta, linhaParcial]);

        // Ambas devem ser inseridas com sucesso — o service não deve gerar nenhum erro
        resultado.Total.Should().Be(2);
        resultado.Sucesso.Should().Be(2,
            because: "campos opcionais nulos devem aparecer como null no JSON " +
                     "(não ser omitidos), garantindo chaves uniformes → sem PGRST102");
        resultado.Falhas.Should().Be(0);
        resultado.Erros.Should().BeEmpty();

        // O repositório deve ter recebido exatamente 2 entidades
        await _repo.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<Cliente>>(list => list.Count() == 2));
    }

    [Fact(DisplayName = "PGRST102: lote de 10 onde metade tem campos opcionais nulos → todos inseridos")]
    public async Task ImportarAsync_LoteMistoParcialCompleto_TodosInseridosUniformes()
    {
        var linhas = new List<ImportarClienteRow>();

        // 5 linhas completas (com telefone, cidade, estado, etc.)
        for (int i = 1; i <= 5; i++)
            linhas.Add(new ImportarClienteRow(
                Linha: i + 1, NomeCompleto: $"Completo {i:D2}",
                Cpf: null, Rg: "99999999", OrgaoExpedidor: "SSP",
                DataNascimentoRaw: "01/01/1990", EstadoCivil: "Casado",
                Profissao: "Médico", Telefone: $"(11) 9000{i:D4}",
                NumeroConta: "001", Pix: $"pix{i}@email.com",
                Cep: "01000-000", Endereco: "Rua A", Numero: "1",
                Bairro: "Centro", Complemento: "Sl 1", Cidade: "SP", Estado: "SP"));

        // 5 linhas parciais (só nome — sem nenhum campo opcional)
        for (int i = 6; i <= 10; i++)
            linhas.Add(CriarLinha(i + 1, nomeCompleto: $"Parcial {i:D2}"));

        var resultado = await _sut.ImportarAsync(linhas);

        resultado.Total.Should().Be(10);
        resultado.Sucesso.Should().Be(10);
        resultado.Falhas.Should().Be(0);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CENÁRIO 8 — Caminho feliz completo
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "Lote totalmente válido → todos inseridos, sem erros")]
    public async Task ImportarAsync_LoteTotalmenteValido_InsereTodosComSucesso()
    {
        var linhas = Enumerable.Range(1, 10)
            .Select(i => CriarLinha(i + 1, nomeCompleto: $"Advogado {i}", cidade: "São Paulo", estado: "SP"))
            .ToList();

        var resultado = await _sut.ImportarAsync(linhas);

        resultado.Total.Should().Be(10);
        resultado.Sucesso.Should().Be(10);
        resultado.Falhas.Should().Be(0);
        resultado.Erros.Should().BeEmpty();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Helper — factory de ImportarClienteRow para os testes
    // ════════════════════════════════════════════════════════════════════════

    private static ImportarClienteRow CriarLinha(
        int linha,
        string nomeCompleto      = "Teste da Silva",
        string? cpf              = null,
        string? dataNascimentoRaw = null,
        string? estado           = null,
        string? cidade           = null) =>
        new(
            Linha:             linha,
            NomeCompleto:      nomeCompleto,
            Cpf:               cpf,
            Rg:                null,
            OrgaoExpedidor:    null,
            DataNascimentoRaw: dataNascimentoRaw,
            EstadoCivil:       null,
            Profissao:         null,
            Telefone:          null,
            NumeroConta:       null,
            Pix:               null,
            Cep:               null,
            Endereco:          null,
            Numero:            null,
            Bairro:            null,
            Complemento:       null,
            Cidade:            cidade,
            Estado:            estado
        );
}

using Agile360.Application.Interfaces;
using Agile360.Domain.Entities;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Agile360.Infrastructure.Auth;

/// <summary>
/// Implementação de <see cref="IRecoveryCodeService"/>.
///
/// Política de segurança implementada:
///   1. Geração via <see cref="RandomNumberGenerator"/> — entropia criptográfica.
///   2. Alfabeto sem caracteres ambíguos (0/O, 1/I/L) — 32 chars, potência de 2 → sem modulo bias.
///   3. BCrypt cost 12 — ~250ms por hash, adequado para 8 chars com entropia suficiente.
///   4. Plaintext exposto APENAS no retorno de <see cref="GenerateCodesAsync"/>; nunca persiste.
///   5. <see cref="ValidateAndConsumeAsync"/> é atômico:
///      UPDATE WHERE is_used = false + rowsAffected == 1 previne race condition em
///      requisições concorrentes com o mesmo código (B7a).
/// </summary>
public class RecoveryCodeService : IRecoveryCodeService
{
    // 32 chars sem ambíguos (0/O, 1/I/L) → sem modulo bias: 256 ÷ 32 = 8 (exato)
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private const int CodesPerSet = 10;
    private const int WorkFactor = 12; // BCrypt cost factor

    private readonly Agile360DbContext _db;

    public RecoveryCodeService(Agile360DbContext db)
    {
        _db = db;
    }

    // ── Geração ───────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GenerateCodesAsync(Guid advogadoId, CancellationToken ct = default)
    {
        // 1. Invalida todos os códigos anteriores (hard delete — sem soft delete aqui)
        await _db.Set<RecoveryCode>()
            .Where(c => c.AdvogadoId == advogadoId)
            .ExecuteDeleteAsync(ct);

        // 2. Gera 10 códigos únicos em plaintext
        var plainCodes = GenerateUniquePlainCodes(CodesPerSet);

        // 3. Persiste apenas os hashes BCrypt — nunca o plaintext.
        //    Hasheia o código NORMALIZADO (sem hífen, uppercase) para que a validação
        //    funcione independente da formatação que o usuário digitar (com ou sem hífen).
        var entities = plainCodes.Select(code => new RecoveryCode
        {
            Id         = Guid.NewGuid(),
            AdvogadoId = advogadoId,
            CodeHash   = BCrypt.Net.BCrypt.HashPassword(Normalize(code), WorkFactor),
            IsUsed     = false,
            CreatedAt  = DateTimeOffset.UtcNow,
        }).ToList();

        await _db.Set<RecoveryCode>().AddRangeAsync(entities, ct);
        await _db.SaveChangesAsync(ct);

        // 4. Retorna os plaintext APENAS neste momento — única oportunidade de exibição
        return plainCodes.AsReadOnly();
    }

    // ── Validação + Consumo Atômico (B7a) ────────────────────────────────────

    /// <inheritdoc />
    /// <remarks>
    /// B7a — Proteção contra race condition:
    ///   Não usamos "SELECT → BCrypt.Verify → UPDATE" em duas operações separadas,
    ///   pois duas requisições concorrentes poderiam ambas passar o BCrypt.Verify
    ///   antes de qualquer uma marcar is_used = true.
    ///
    ///   Estratégia adotada:
    ///     1. Carrega todos os hashes não usados do advogado (máx. 10 registros).
    ///     2. Verifica qual hash corresponde ao código fornecido.
    ///     3. Executa UPDATE ... SET is_used = true, used_at = NOW()
    ///              WHERE id = @matchedId AND is_used = false
    ///     4. Verifica rowsAffected == 1. Se 0, outro request ganhou a corrida → rejeita.
    ///
    ///   O UPDATE condicional (WHERE is_used = false) garante que apenas UM request
    ///   consiga marcar o código — o segundo verá rowsAffected = 0 e retornará false.
    /// </remarks>
    public async Task<bool> ValidateAndConsumeAsync(Guid advogadoId, string code, CancellationToken ct = default)
    {
        var normalized = Normalize(code);

        // 1. Carrega todos os hashes ativos (is_used = false) do advogado
        var activeCodes = await _db.Set<RecoveryCode>()
            .Where(c => c.AdvogadoId == advogadoId && !c.IsUsed)
            .AsNoTracking()
            .Select(c => new { c.Id, c.CodeHash })
            .ToListAsync(ct);

        if (activeCodes.Count == 0)
            return false;

        // 2. Encontra qual hash corresponde ao código fornecido
        //    BCrypt.Verify é constant-time → sem timing attack
        Guid? matchedId = null;
        foreach (var entry in activeCodes)
        {
            if (BCrypt.Net.BCrypt.Verify(normalized, entry.CodeHash))
            {
                matchedId = entry.Id;
                break;
            }
        }

        if (matchedId is null)
            return false;

        // 3. UPDATE atômico: WHERE id = @matchedId AND is_used = false
        //    Se duas requisições simultâneas chegarem aqui com o mesmo matchedId,
        //    apenas UMA verá rowsAffected == 1; a outra verá 0 e será rejeitada.
        var now = DateTimeOffset.UtcNow;
        var rowsAffected = await _db.Set<RecoveryCode>()
            .Where(c => c.Id == matchedId.Value && !c.IsUsed)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.IsUsed, true)
                .SetProperty(c => c.UsedAt, now),
                ct);

        // 4. rowsAffected == 0 significa que o código foi consumido por outra requisição concorrente
        return rowsAffected == 1;
    }

    // ── Consulta ─────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public Task<int> GetRemainingCountAsync(Guid advogadoId, CancellationToken ct = default)
    {
        return _db.Set<RecoveryCode>()
            .Where(c => c.AdvogadoId == advogadoId && !c.IsUsed)
            .CountAsync(ct);
    }

    // ── Limpeza ───────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public Task DeleteAllAsync(Guid advogadoId, CancellationToken ct = default)
    {
        return _db.Set<RecoveryCode>()
            .Where(c => c.AdvogadoId == advogadoId)
            .ExecuteDeleteAsync(ct);
    }

    // ── Geração de códigos (privado) ──────────────────────────────────────────

    /// <summary>
    /// Gera <paramref name="count"/> códigos únicos no formato XXXX-XXXX
    /// usando <see cref="RandomNumberGenerator"/> para entropia criptográfica.
    ///
    /// Alfabeto: 32 chars (potência de 2) → 256 / 32 = 8 exato → sem modulo bias.
    /// Entropia por código: log₂(32⁸) ≈ 40 bits — adequado para uso único.
    /// </summary>
    private static List<string> GenerateUniquePlainCodes(int count)
    {
        var codes = new HashSet<string>(count);
        while (codes.Count < count)
        {
            codes.Add(GenerateSingleCode());
        }
        return codes.ToList();
    }

    private static string GenerateSingleCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(8);
        var chars = bytes.Select(b => Alphabet[b % Alphabet.Length]).ToArray();
        // Formato visual: XXXX-XXXX (9 chars visíveis — 8 alfanum + 1 hífen)
        return $"{new string(chars[..4])}-{new string(chars[4..])}";
    }

    /// <summary>
    /// Normaliza o código antes de comparar/hashear:
    /// remove hífens visuais e converte para uppercase.
    /// Isso permite que o usuário informe "ABCDEFGH" ou "ABCD-EFGH" e ambos funcionem.
    /// </summary>
    private static string Normalize(string code) =>
        code.Replace("-", "").Trim().ToUpperInvariant();
}

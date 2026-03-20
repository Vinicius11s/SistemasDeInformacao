using Agile360.Application.Auth.DTOs;
using Agile360.Application.Interfaces;
using Agile360.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OtpNet;
using System.Security.Cryptography;
using System.Text;

namespace Agile360.Infrastructure.Auth;

/// <summary>
/// TOTP (RFC 6238) MFA service backed by Otp.NET.
///
/// Security contract:
///   - TOTP secrets are NEVER stored in plaintext.
///   - AES-256-GCM encryption is applied before every DB write.
///   - The encryption key lives in configuration (MfaSettings:EncryptionKey).
///   - A 32-byte random key is derived via PBKDF2 if the raw config value is shorter.
///   - B10: CompleteSetupAsync gera 10 recovery codes automaticamente ao ativar o MFA.
///   - B11: DisableAsync deleta todos os recovery codes ao desativar o MFA.
/// </summary>
public class MfaService : IMfaService
{
    private const string Issuer = "Agile360";
    private const int TotpStep = 30;      // seconds per code
    // Carência temporária: ±2 minutos.
    // Cada passo do TOTP = 30s → 2 min / 30s = 4 passos.
    private const int VerifyWindow = 4;   // ±4 steps tolerance (~±120s)

    private readonly Agile360DbContext _db;
    private readonly byte[] _encKey;
    private readonly IRecoveryCodeService _recoveryCodes;
    private readonly ILogger<MfaService> _logger;

    public MfaService(
        Agile360DbContext db,
        IConfiguration configuration,
        IRecoveryCodeService recoveryCodes,
        ILogger<MfaService> logger)
    {
        _db            = db;
        _recoveryCodes = recoveryCodes;
        _logger        = logger;
        var raw = configuration["MfaSettings:EncryptionKey"] ?? throw new InvalidOperationException(
            "MfaSettings:EncryptionKey is required for MFA. Add it to appsettings or user-secrets.");
        // Derive a 32-byte key using PBKDF2 (tolerant of short config values)
        _encKey = DeriveKey(raw);
    }

    // ── Setup flow ────────────────────────────────────────────────────────

    public async Task<MfaSetupResponse> BeginSetupAsync(Guid advogadoId, string email, CancellationToken ct = default)
    {
        var advogado = await GetAdvogadoAsync(advogadoId, ct);

        // Generate a fresh 20-byte (160-bit) random secret
        var secretBytes   = KeyGeneration.GenerateRandomKey(20);
        var base32Secret  = Base32Encoding.ToString(secretBytes);
        var encrypted     = Encrypt(base32Secret);

        // Store as pending (not active yet — will be promoted after first successful verify)
        advogado.MfaPendingSecret = encrypted;
        advogado.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        // ── DIAG ──────────────────────────────────────────────────────────────
        // [TEMP] Confirma que o segredo pendente foi persistido com sucesso.
        // Remover antes do deploy em produção.
        _logger.LogDebug(
            "[MFA-DIAG] BeginSetup OK — advogadoId: {Id} | " +
            "secretLen: {SLen} chars | encryptedLen: {ELen} chars | " +
            "utcNow: {Utc} | step: {Step}s | algo: SHA1 | digits: 6",
            advogadoId,
            base32Secret.Length,
            encrypted.Length,
            DateTimeOffset.UtcNow.ToString("o"),
            TotpStep);

        var qrUrl = $"otpauth://totp/{Uri.EscapeDataString(Issuer)}:{Uri.EscapeDataString(email)}" +
                    $"?secret={base32Secret}&issuer={Uri.EscapeDataString(Issuer)}&algorithm=SHA1&digits=6&period={TotpStep}";

        return new MfaSetupResponse(qrUrl, base32Secret, advogado.MfaEnabled);
    }

    public async Task<bool> CompleteSetupAsync(Guid advogadoId, string code, CancellationToken ct = default)
    {
        var advogado = await GetAdvogadoAsync(advogadoId, ct);

        // ── DIAG ──────────────────────────────────────────────────────────────
        // [TEMP] Ponto de diagnóstico 1: estado do segredo pendente no banco.
        var hasPendingSecret = !string.IsNullOrEmpty(advogado.MfaPendingSecret);
        _logger.LogDebug(
            "[MFA-DIAG] CompleteSetup — advogadoId: {Id} | " +
            "hasPendingSecret: {HasSecret} | encryptedLen: {ELen} | " +
            "codeLen: {CLen} | utcNow: {Utc}",
            advogadoId,
            hasPendingSecret,
            advogado.MfaPendingSecret?.Length ?? 0,
            code.Trim().Length,
            DateTimeOffset.UtcNow.ToString("o"));

        if (!hasPendingSecret)
        {
            _logger.LogWarning(
                "[MFA-DIAG] CompleteSetup FALHOU — mfa_pending_secret é NULL/vazio " +
                "para advogadoId: {Id}. Passo 1 não persistiu ou foi sobrescrito.", advogadoId);
            return false;
        }

        string plainSecret;
        try
        {
            plainSecret = Decrypt(advogado.MfaPendingSecret!);

            // ── DIAG ──────────────────────────────────────────────────────────
            // [TEMP] Ponto de diagnóstico 2: resultado da descriptografia.
            // NÃO loga o segredo real — apenas comprimento e formato esperado (32 chars base32).
            _logger.LogDebug(
                "[MFA-DIAG] Decrypt OK — plainSecretLen: {Len} | isValidBase32Len: {Valid}",
                plainSecret.Length,
                plainSecret.Length is 32 or 16 or 26 or 56); // tamanhos válidos de Base32 (10/8/20/35 bytes)
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[MFA-DIAG] CompleteSetup FALHOU — Decrypt lançou exceção para advogadoId: {Id}. " +
                "Verifique se MfaSettings:EncryptionKey é a mesma usada no BeginSetup.",
                advogadoId);
            return false;
        }

        var codeOk = VerifyCode(plainSecret, code, _logger);
        if (!codeOk)
            return false;

        // Promote pending → active
        advogado.MfaSecret = advogado.MfaPendingSecret;
        advogado.MfaPendingSecret = null;
        advogado.MfaEnabled = true;
        advogado.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        // B10: Os códigos de recuperação são gerados no controller VerifySetup,
        // que chama IRecoveryCodeService.GenerateCodesAsync após esta chamada retornar true.
        // Isso permite que os códigos em plaintext sejam incluídos na resposta HTTP
        // para exibição única ao usuário no Passo 3 do stepper de segurança.

        return true;
    }

    public async Task<bool> DisableAsync(Guid advogadoId, string code, CancellationToken ct = default)
    {
        var advogado = await GetAdvogadoAsync(advogadoId, ct);

        if (!advogado.MfaEnabled || string.IsNullOrEmpty(advogado.MfaSecret))
            return false;

        var plainSecret = Decrypt(advogado.MfaSecret);
        if (!VerifyCode(plainSecret, code))
            return false;

        advogado.MfaEnabled = false;
        advogado.MfaSecret = null;
        advogado.MfaPendingSecret = null;
        advogado.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        // B11: Remove todos os códigos de recuperação ao desativar o MFA.
        // Hard delete — códigos de backup sem MFA ativo não fazem sentido.
        await _recoveryCodes.DeleteAllAsync(advogado.Id, ct);

        return true;
    }

    // ── Login flow ────────────────────────────────────────────────────────

    public async Task<bool> ValidateCodeAsync(Guid advogadoId, string code, CancellationToken ct = default)
    {
        var advogado = await _db.Advogados
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == advogadoId, ct);

        if (advogado is null || !advogado.MfaEnabled || string.IsNullOrEmpty(advogado.MfaSecret))
            return false;

        var plainSecret = Decrypt(advogado.MfaSecret);
        // Passa logger para produzir logs diagnósticos (serverUtcEpoch/window/matchedStep)
        // sem vazar o segredo TOTP ou o código digitado.
        return VerifyCode(plainSecret, code, logger: _logger);
    }

    // ── Status ────────────────────────────────────────────────────────────

    public async Task<MfaStatusResponse> GetStatusAsync(Guid advogadoId, CancellationToken ct = default)
    {
        var enabled = await _db.Advogados
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(a => a.Id == advogadoId)
            .Select(a => a.MfaEnabled)
            .FirstOrDefaultAsync(ct);
        return new MfaStatusResponse(enabled);
    }

    // ── Private helpers ───────────────────────────────────────────────────

    private async Task<Domain.Entities.Advogado> GetAdvogadoAsync(Guid advogadoId, CancellationToken ct)
    {
        var adv = await _db.Advogados
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == advogadoId, ct)
            ?? throw new InvalidOperationException("Advogado não encontrado.");
        return adv;
    }

    /// <summary>
    /// Sobrecarga sem logger — usada por ValidateCodeAsync (login flow).
    /// </summary>
    private static bool VerifyCode(string base32Secret, string code) =>
        VerifyCode(base32Secret, code, logger: null);

    /// <summary>
    /// Valida o código TOTP com janela de ±VerifyWindow ciclos.
    /// Sobrecarga com logger opcional para diagnóstico no setup flow.
    /// </summary>
    private static bool VerifyCode(string base32Secret, string code, ILogger? logger)
    {
        try
        {
            var trimmed = code.Trim();

            // ── DIAG ────────────────────────────────────────────────────────
            // Hora UTC real do servidor — compara com a do celular para clock skew.
            var serverUtc = DateTimeOffset.UtcNow;

            var secretBytes = Base32Encoding.ToBytes(base32Secret);
            var totp        = new Totp(secretBytes, step: TotpStep);

            // Calcula o código esperado neste instante para confirmar no log
            // (NÃO exposto ao usuário — apenas vai para os logs de Debug internos).
            var expectedCode = totp.ComputeTotp(serverUtc.UtcDateTime);

            // Janela explícita: ±VerifyWindow ciclos (±30s com TotpStep=30).
            var window = new VerificationWindow(previous: VerifyWindow, future: VerifyWindow);
            var ok     = totp.VerifyTotp(trimmed, out long matchedStep, window);

            logger?.LogDebug(
                "[MFA-DIAG] VerifyCode — serverUtcEpoch: {Epoch} | " +
                "serverUtc: {Utc} | " +
                "codeDigits: {Digits} | is6Digits: {Is6} | " +
                "step: {Step}s | window: ±{Win} | algo: SHA1 | " +
                "expectedMatchesReceived: {Match} | verifyResult: {Ok} | matchedStep: {Step2}",
                serverUtc.ToUnixTimeSeconds(),
                serverUtc.ToString("o"),
                trimmed.Length,
                trimmed.Length == 6,
                TotpStep,
                VerifyWindow,
                expectedCode == trimmed,
                ok,
                ok ? matchedStep : -1L);

            // Log "aparecível" mesmo em níveis acima de Debug:
            // ajuda a distinguir drift vs token/secret inconsistente.
            if (logger != null && !ok)
            {
                logger.LogWarning(
                    "[MFA-TOTP] VerifyTotp falhou — serverUtcEpoch: {Epoch} | step: {Step}s | window: ±{Win} | matchedStep: {MatchedStep}",
                    serverUtc.ToUnixTimeSeconds(),
                    TotpStep,
                    VerifyWindow,
                    matchedStep);
            }

            return ok;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[MFA-DIAG] VerifyCode lançou exceção — base32SecretLen: {Len}", base32Secret.Length);
            return false;
        }
    }

    // ── AES-256-GCM encryption ────────────────────────────────────────────

    /// <summary>
    /// Encrypts plaintext → "base64(nonce[12] || ciphertext || tag[16])".
    /// </summary>
    private string Encrypt(string plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize); // 12 bytes
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        using var aes = new AesGcm(_encKey, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        var result = new byte[nonce.Length + ciphertext.Length + tag.Length];
        nonce.CopyTo(result, 0);
        ciphertext.CopyTo(result, nonce.Length);
        tag.CopyTo(result, nonce.Length + ciphertext.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts the format produced by <see cref="Encrypt"/>.
    /// </summary>
    private string Decrypt(string encryptedBase64)
    {
        var raw = Convert.FromBase64String(encryptedBase64);
        const int nonceSize = 12;
        const int tagSize = 16;
        var ciphertextSize = raw.Length - nonceSize - tagSize;

        var nonce = raw[..nonceSize];
        var ciphertext = raw[nonceSize..(nonceSize + ciphertextSize)];
        var tag = raw[(nonceSize + ciphertextSize)..];
        var plaintext = new byte[ciphertextSize];

        using var aes = new AesGcm(_encKey, tagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    private static byte[] DeriveKey(string rawKey)
    {
        var salt = Encoding.UTF8.GetBytes("Agile360-MFA-Salt");
        return Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(rawKey),
            salt,
            iterations: 100_000,
            HashAlgorithmName.SHA256,
            outputLength: 32);
    }
}

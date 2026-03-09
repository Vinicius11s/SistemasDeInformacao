using Agile360.Application.Auth.DTOs;
using Agile360.Application.Interfaces;

namespace Agile360.Infrastructure.Auth;

/// <summary>
/// Stub used when MfaSettings:EncryptionKey is not configured.
/// MFA endpoints remain available but return "not configured" behaviour so the API does not fail at startup.
/// </summary>
public class DisabledMfaService : IMfaService
{
    private const string Message = "MFA não está configurado. Adicione MfaSettings:EncryptionKey em appsettings ou user-secrets.";

    public Task<MfaSetupResponse> BeginSetupAsync(Guid advogadoId, string email, CancellationToken ct = default) =>
        throw new InvalidOperationException(Message);

    public Task<bool> CompleteSetupAsync(Guid advogadoId, string code, CancellationToken ct = default) =>
        Task.FromResult(false);

    public Task<bool> DisableAsync(Guid advogadoId, string code, CancellationToken ct = default) =>
        Task.FromResult(false);

    public Task<bool> ValidateCodeAsync(Guid advogadoId, string code, CancellationToken ct = default) =>
        Task.FromResult(false);

    public Task<MfaStatusResponse> GetStatusAsync(Guid advogadoId, CancellationToken ct = default) =>
        Task.FromResult(new MfaStatusResponse(MfaEnabled: false));
}

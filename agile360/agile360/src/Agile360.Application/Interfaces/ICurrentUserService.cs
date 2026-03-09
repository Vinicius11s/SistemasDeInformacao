namespace Agile360.Application.Interfaces;

public interface ICurrentUserService
{
    Guid AdvogadoId { get; }
    string Email { get; }
    string Nome { get; }
    bool IsAuthenticated { get; }

    /// <summary>
    /// Bearer token bruto extraído do cabeçalho Authorization da requisição atual.
    /// Enviado diretamente para a Data API do Supabase: Authorization: Bearer &lt;AccessToken&gt;.
    /// </summary>
    string? AccessToken { get; }
}

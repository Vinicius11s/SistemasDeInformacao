namespace Agile360.Application.Integration;

/// <summary>
/// Gateway for AI/n8n calls. Story 1.4. Implementations may call n8n or, in the future, AIOS.
/// </summary>
public interface IAiGatewayService
{
    /// <summary>
    /// Sends an extraction request to the AI workflow and returns typed result or error.
    /// </summary>
    Task<AiExtractionResult<T>> ExtractAsync<T>(AiExtractionRequest request, CancellationToken ct = default);
}

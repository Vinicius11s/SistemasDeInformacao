namespace Agile360.Application.Integration;

/// <summary>
/// Validates webhook payload signature (e.g. HMAC-SHA256 from n8n). Story 1.4.
/// </summary>
public interface IWebhookSignatureValidator
{
    /// <summary>
    /// Validates that the signature matches the computed HMAC of the payload with the given secret.
    /// </summary>
    /// <param name="payload">Raw request body (e.g. JSON).</param>
    /// <param name="signature">Signature from header (e.g. X-Webhook-Signature: sha256=...).</param>
    /// <param name="secret">Configured webhook secret.</param>
    /// <returns>True if valid; false otherwise.</returns>
    bool Validate(string payload, string signature, string secret);
}

using System.Security.Cryptography;
using System.Text;
using Agile360.Application.Integration;

namespace Agile360.Infrastructure.Integration;

/// <summary>
/// HMAC-SHA256 webhook signature validation. Story 1.4.
/// Expects header format: X-Webhook-Signature: sha256=&lt;hex&gt;
/// </summary>
public class WebhookSignatureValidator : IWebhookSignatureValidator
{
    private const string Prefix = "sha256=";

    public bool Validate(string payload, string signature, string secret)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(signature))
            return false;

        var trimmed = signature.Trim();
        if (!trimmed.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var expectedHex = trimmed[Prefix.Length..].Trim();
        var payloadBytes = Encoding.UTF8.GetBytes(payload ?? "");
        var secretBytes = Encoding.UTF8.GetBytes(secret);

        using var hmac = new HMACSHA256(secretBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        var computedHex = ToHexString(hash);

        return string.Equals(computedHex, expectedHex, StringComparison.OrdinalIgnoreCase);
    }

    private static string ToHexString(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}

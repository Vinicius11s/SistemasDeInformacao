using System.Security.Cryptography;
using System.Text;

namespace Agile360.Infrastructure.Auth;

/// <summary>
/// Computes a SHA-256 hex hash of a raw token value.
/// Only the hash is stored in the DB — the raw token stays in the HttpOnly cookie.
/// </summary>
public static class TokenHasher
{
    public static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

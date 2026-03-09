using System;
using System.Security.Cryptography;

namespace Agile360.Infrastructure.Auth;

/// <summary>
/// Simple PBKDF2 password hasher. Produces string in format: {iterations}.{saltBase64}.{hashBase64}
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public static string Hash(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var derive = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var key = derive.GetBytes(KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string hashed)
    {
        if (string.IsNullOrEmpty(hashed)) return false;
        var parts = hashed.Split('.', 3);
        if (parts.Length != 3) return false;
        if (!int.TryParse(parts[0], out var iterations)) return false;
        var salt = Convert.FromBase64String(parts[1]);
        var key = Convert.FromBase64String(parts[2]);

        using var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var keyToCheck = derive.GetBytes(key.Length);
        return CryptographicOperations.FixedTimeEquals(keyToCheck, key);
    }
}


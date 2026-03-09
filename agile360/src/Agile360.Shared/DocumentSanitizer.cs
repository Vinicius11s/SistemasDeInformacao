using System.Text.RegularExpressions;

namespace Agile360.Shared;

/// <summary>
/// Removes every non-digit character from document strings (CPF, CNPJ, RG, phone).
/// Clean Data policy: only raw digits are persisted in the database.
/// Formatting is the sole responsibility of the presentation layer.
/// </summary>
public static partial class DocumentSanitizer
{
    [GeneratedRegex(@"[^\d]")]
    private static partial Regex NonDigitPattern();

    /// <summary>
    /// Returns a string containing only the digit characters of <paramref name="value"/>.
    /// Returns <see langword="null"/> when the input is null or becomes empty after stripping.
    /// </summary>
    public static string? OnlyDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = NonDigitPattern().Replace(value, string.Empty);
        return digits.Length == 0 ? null : digits;
    }

    /// <summary>Sanitizes the raw value and returns it, or null if empty.</summary>
    public static string? Sanitize(string? value) => OnlyDigits(value);
}

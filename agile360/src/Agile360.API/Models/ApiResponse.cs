namespace Agile360.API.Models;

/// <summary>
/// Standard API response wrapper for consistent JSON responses.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public static ApiResponse<T> Ok(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static ApiResponse<T> Fail(string message, string? code = null, int? statusCode = null) => new()
    {
        Success = false,
        Error = new ApiError(message, code, statusCode)
    };
}

public record ApiError(string Message, string? Code = null, int? StatusCode = null);

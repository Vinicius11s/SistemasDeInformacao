namespace Agile360.Application.Integration;

/// <summary>
/// Result of an AI extraction call. Story 1.4.
/// </summary>
public class AiExtractionResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }

    public static AiExtractionResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static AiExtractionResult<T> Fail(string error) => new() { Success = false, Error = error };
}

namespace Agile360.Application.Integration;

/// <summary>
/// Request for AI extraction (e.g. text, audio URL, context). Story 1.4.
/// </summary>
public class AiExtractionRequest
{
    public string? Text { get; set; }
    public string? AudioUrl { get; set; }
    public string? Context { get; set; }
    public string? WorkflowKey { get; set; }
}

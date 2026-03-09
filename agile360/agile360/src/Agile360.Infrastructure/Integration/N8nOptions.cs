namespace Agile360.Infrastructure.Integration;

/// <summary>
/// Options for outbound calls to n8n. Story 1.4.
/// </summary>
public class N8nOptions
{
    public const string SectionName = "N8n";
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
}

using System.Net.Http.Json;
using System.Text.Json;
using Agile360.Application.Integration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Agile360.Infrastructure.Integration;

/// <summary>
/// Story 1.4: Delegates AI extraction to n8n workflow via HTTP. Uses named client "Agile360.AI" (Polly).
/// </summary>
public class N8nAiGatewayService : IAiGatewayService
{
    private const string ClientName = "Agile360.AI";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<N8nOptions> _options;
    private readonly ILogger<N8nAiGatewayService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public N8nAiGatewayService(IHttpClientFactory httpClientFactory, IOptions<N8nOptions> options, ILogger<N8nAiGatewayService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<AiExtractionResult<T>> ExtractAsync<T>(AiExtractionRequest request, CancellationToken ct = default)
    {
        var baseUrl = _options.Value.BaseUrl;
        if (string.IsNullOrEmpty(baseUrl))
        {
            _logger.LogWarning("N8n BaseUrl not configured; returning stub failure");
            return AiExtractionResult<T>.Fail("AI Gateway not configured (N8n:BaseUrl)");
        }

        try
        {
            var client = _httpClientFactory.CreateClient(ClientName);
            var response = await client.PostAsJsonAsync("webhook/agile360-ai", request, JsonOptions, ct);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
            return data != null ? AiExtractionResult<T>.Ok(data) : AiExtractionResult<T>.Fail("Empty response from AI workflow");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "AI Gateway HTTP error");
            return AiExtractionResult<T>.Fail(ex.Message);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("AI Gateway request timed out");
            return AiExtractionResult<T>.Fail("Request timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI Gateway error");
            return AiExtractionResult<T>.Fail(ex.Message);
        }
    }
}

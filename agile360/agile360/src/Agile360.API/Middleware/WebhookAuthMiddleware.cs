using Agile360.Application.Integration;
using Microsoft.Extensions.Options;

namespace Agile360.API.Middleware;

/// <summary>
/// Story 1.4: Validates webhook signature (HMAC) for requests under /api/webhooks/.
/// Returns 401 if secret is not configured; 403 if signature is invalid.
/// </summary>
public class WebhookAuthMiddleware
{
    public const string WebhookSignatureHeader = "X-Webhook-Signature";
    public const string WebhookPathPrefix = "/api/webhooks/";

    private readonly RequestDelegate _next;
    private readonly IWebhookSignatureValidator _validator;
    private readonly string _secret;

    public WebhookAuthMiddleware(RequestDelegate next, IWebhookSignatureValidator validator, IOptions<WebhookOptions> options)
    {
        _next = next;
        _validator = validator;
        _secret = options.Value.N8nSecret ?? "";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments(WebhookPathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (string.IsNullOrEmpty(_secret))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Webhook secret not configured" });
            return;
        }

        context.Request.EnableBuffering();
        string body;
        using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var signature = context.Request.Headers[WebhookSignatureHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(signature) || !_validator.Validate(body, signature, _secret))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid webhook signature" });
            return;
        }

        await _next(context);
    }
}

public class WebhookOptions
{
    public const string SectionName = "Webhooks";
    public string? N8nSecret { get; set; }
}

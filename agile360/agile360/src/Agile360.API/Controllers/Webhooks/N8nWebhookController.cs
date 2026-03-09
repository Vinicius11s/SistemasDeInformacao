using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Controllers.Webhooks;

/// <summary>
/// Base for webhook endpoints called by n8n. Story 1.4.
/// All actions are protected by WebhookAuthMiddleware (HMAC validation) when under /api/webhooks/.
/// </summary>
[ApiController]
[Route("api/webhooks/n8n/[action]")]
public class N8nWebhookController : ControllerBase
{
    /// <summary>
    /// Placeholder for n8n webhook. Replace with actual workflow handlers (e.g. WhatsApp, Intimações).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        return Ok(new { received = true, source = "n8n" });
    }
}

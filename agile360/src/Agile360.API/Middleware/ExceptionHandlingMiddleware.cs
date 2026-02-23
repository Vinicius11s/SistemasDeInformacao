using System.Net;
using System.Text.Json;
using Agile360.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Agile360.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.TraceIdentifier;
            _logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);

            await WriteErrorResponseAsync(context, ex, correlationId);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception ex, string correlationId)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Error = new ApiError(message, "ERROR", (int)statusCode),
            Timestamp = DateTimeOffset.UtcNow
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var payload = new Dictionary<string, object?>
        {
            ["success"] = false,
            ["data"] = (object?)null,
            ["error"] = new { message = response.Error!.Message, code = response.Error.Code, statusCode = response.Error.StatusCode },
            ["timestamp"] = response.Timestamp,
            ["correlationId"] = correlationId
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, options));
    }
}

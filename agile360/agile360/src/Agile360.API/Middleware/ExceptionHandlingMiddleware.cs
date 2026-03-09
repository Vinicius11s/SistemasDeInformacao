using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Agile360.API.Models;

namespace Agile360.API.Middleware;

/// <summary>
/// Middleware global de tratamento de exceções.
///
/// Além de ArgumentException / KeyNotFoundException / UnauthorizedAccessException,
/// trata HttpRequestException vindos do SupabaseDataClient — em especial:
///   • 23502 — null value in column (campo NOT NULL sem valor)
///   • 23505 — unique constraint violation (ex.: num_processo duplicado)
///   • 23503 — foreign key violation
///   • PGRST102 — batch insert com chaves diferentes
///   • 401 Supabase — token expirado / inválido
/// </summary>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.TraceIdentifier;
            logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);
            await WriteErrorResponseAsync(context, ex, correlationId);
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context, Exception ex, string correlationId)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ResolveError(ex);

        context.Response.StatusCode = (int)statusCode;

        var payload = new Dictionary<string, object?>
        {
            ["success"]       = false,
            ["data"]          = (object?)null,
            ["error"]         = new
            {
                message    = message,
                code       = statusCode.ToString(),
                statusCode = (int)statusCode
            },
            ["timestamp"]     = DateTimeOffset.UtcNow,
            ["correlationId"] = correlationId
        };

        var opts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented        = false,
            // Evita escape de caracteres acentuados (ã, ç, é…) no JSON de erro,
            // garantindo que mensagens em português apareçam legíveis no frontend.
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, opts));
    }

    // ─── Mapeamento de exceções → (HTTP status, mensagem amigável) ────────────
    private static (HttpStatusCode status, string message) ResolveError(Exception ex) => ex switch
    {
        ArgumentException       => (HttpStatusCode.BadRequest,           ex.Message),
        KeyNotFoundException    => (HttpStatusCode.NotFound,             ex.Message),
        UnauthorizedAccessException
                                => (HttpStatusCode.Unauthorized,         "Acesso não autorizado."),
        HttpRequestException httpEx => ResolveSupabaseError(httpEx),
        _                       => (HttpStatusCode.InternalServerError,  "Ocorreu um erro inesperado. Tente novamente.")
    };

    /// <summary>
    /// Traduz códigos de erro do Supabase/PostgreSQL em respostas HTTP amigáveis.
    ///
    /// Códigos PostgreSQL (campo "code" no JSON de erro do PostgREST):
    ///   23502 — NOT NULL violation        → 400 com mensagem descritiva
    ///   23505 — unique constraint          → 409 Conflict
    ///   23503 — foreign key violation      → 422 Unprocessable Entity
    ///   PGRST102 — batch keys mismatch     → 400 com orientação
    ///
    /// O Supabase retorna 401 quando o JWT está expirado ou inválido.
    /// </summary>
    private static (HttpStatusCode status, string message) ResolveSupabaseError(
        HttpRequestException ex)
    {
        var body = ex.Message;

        // ── 401 — Token expirado/inválido ─────────────────────────────────
        if (ex.StatusCode == HttpStatusCode.Unauthorized ||
            body.Contains("401"))
            return (HttpStatusCode.Unauthorized,
                    "Sua sessão expirou. Faça login novamente.");

        // ── 23502 — Campo NOT NULL sem valor ──────────────────────────────
        if (body.Contains("23502"))
        {
            var campo = ExtractPgField(body, "column");
            var tabela = ExtractPgField(body, "relation");
            var descricao = campo is not null
                ? $"O campo '{campo}' é obrigatório e não pode ficar em branco."
                : "Um campo obrigatório não foi preenchido.";
            return (HttpStatusCode.BadRequest, descricao);
        }

        // ── 23505 — Unique constraint violation ───────────────────────────
        if (body.Contains("23505"))
        {
            // Ex.: "Key (num_processo)=(0000000-00.2026.8.26.0000) already exists."
            var detalhe = ExtractPgField(body, "details") ?? body;

            if (detalhe.Contains("num_processo") || body.Contains("num_processo"))
                return (HttpStatusCode.Conflict,
                        "Já existe um processo cadastrado com este número. Verifique o campo 'Número do Processo'.");

            if (detalhe.Contains("cpf") || body.Contains("cpf"))
                return (HttpStatusCode.Conflict,
                        "Já existe um cliente cadastrado com este CPF.");

            return (HttpStatusCode.Conflict,
                    $"Já existe um registro com este valor. {detalhe}".Trim());
        }

        // ── 23503 — Foreign key violation ─────────────────────────────────
        if (body.Contains("23503"))
        {
            var campo = ExtractPgField(body, "column");
            return (HttpStatusCode.UnprocessableEntity,
                    campo is not null
                        ? $"Referência inválida no campo '{campo}': o registro vinculado não existe."
                        : "Um dos registros referenciados não existe no sistema.");
        }

        // ── PGRST102 — Batch insert: chaves JSON diferentes ───────────────
        if (body.Contains("PGRST102") || body.Contains("All object keys must match"))
            return (HttpStatusCode.BadRequest,
                    "Erro de estrutura na importação em lote. Todos os registros devem possuir os mesmos campos.");

        // ── Outros erros 4xx do Supabase ──────────────────────────────────
        if (ex.StatusCode.HasValue && (int)ex.StatusCode.Value is >= 400 and < 500)
            return ((HttpStatusCode)ex.StatusCode.Value,
                    $"Erro de validação: {body.TruncateTo(200)}");

        // ── Falha de rede / 5xx ───────────────────────────────────────────
        return (HttpStatusCode.BadGateway,
                "Não foi possível conectar ao banco de dados. Tente novamente em instantes.");
    }

    /// <summary>
    /// Extrai valores de chaves conhecidas do JSON de erro do Supabase.
    /// Ex.: {"code":"23502","message":"null value in column \"criado_em\"...","details":null}
    /// </summary>
    private static string? ExtractPgField(string body, string field)
    {
        try
        {
            // O body pode ser: "Supabase PostgREST 400 Bad Request: {json}"
            var jsonStart = body.IndexOf('{');
            if (jsonStart < 0) return null;
            var json = body[jsonStart..];
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(field, out var el) &&
                el.ValueKind == JsonValueKind.String)
                return el.GetString();
        }
        catch { /* ignora JSON inválido */ }
        return null;
    }
}

// ─── Extension helper ────────────────────────────────────────────────────────
file static class StringExtensions
{
    internal static string TruncateTo(this string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}

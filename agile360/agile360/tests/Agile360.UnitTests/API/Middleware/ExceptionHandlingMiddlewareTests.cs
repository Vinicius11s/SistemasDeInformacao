using System.Net;
using System.Text;
using System.Text.Json;
using Agile360.API.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Agile360.UnitTests.API.Middleware;

/// <summary>
/// Bateria de testes QA — ExceptionHandlingMiddleware
///
/// Valida que cada código de erro Supabase/PostgreSQL é mapeado
/// corretamente para o HTTP status e a mensagem amigável esperada.
///
/// Cenários cobertos:
///   1. 23502 — campo NOT NULL sem valor           → 400 + mensagem descritiva
///   2. 23505 — num_processo duplicado             → 409 Conflict
///   3. 23503 — foreign key inválida               → 422
///   4. PGRST102 — batch insert com chaves diff    → 400
///   5. 401 Supabase (token expirado)              → 401
///   6. ArgumentException                          → 400
///   7. KeyNotFoundException                       → 404
///   8. Exceção genérica                           → 500
/// </summary>
public class ExceptionHandlingMiddlewareTests
{
    // ─── Infraestrutura de teste ──────────────────────────────────────────────

    /// <summary>
    /// Executa o middleware com uma exceção simulada e retorna
    /// (statusCode, body) da resposta.
    /// </summary>
    private static async Task<(int StatusCode, string Body)> InvokeComExcecao(Exception ex)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            next: _ => throw ex,
            logger: Microsoft.Extensions.Logging.Abstractions
                             .NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        return (context.Response.StatusCode, body);
    }

    private static HttpRequestException SupabaseEx(string jsonBody, HttpStatusCode? status = null)
    {
        var msg = $"Supabase PostgREST {(int)(status ?? HttpStatusCode.BadRequest)} Bad Request: {jsonBody}";
        return new HttpRequestException(msg, inner: null, statusCode: status ?? HttpStatusCode.BadRequest);
    }

    private static string PgErrorJson(string code, string message, string? details = null, string? column = null)
        => JsonSerializer.Serialize(new
        {
            code,
            message,
            details,
            hint = (string?)null,
            column,
            relation = "compromisso"
        });

    // ════════════════════════════════════════════════════════════════════════
    //  23502 — NOT NULL violation
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "23502 sem coluna → 400 com mensagem genérica de campo obrigatório")]
    public async Task Handle_23502_SemColuna_Retorna400Generico()
    {
        var ex = SupabaseEx(PgErrorJson("23502",
            "null value in column \"criado_em\" of relation \"processo\" violates not-null constraint"));

        var (status, body) = await InvokeComExcecao(ex);

        status.Should().Be(400);
        body.Should().Contain("obrigatório");
    }

    [Fact(DisplayName = "23502 com coluna explícita → 400 mencionando o campo")]
    public async Task Handle_23502_ComColuna_Retorna400ComNomeDoCampo()
    {
        var ex = SupabaseEx(PgErrorJson("23502",
            "null value in column \"criado_em\" violates not-null constraint",
            column: "criado_em"));

        var (status, body) = await InvokeComExcecao(ex);

        status.Should().Be(400);
        body.Should().Contain("criado_em");
        body.Should().Contain("obrigatório");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  23505 — Unique constraint violation
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "23505 num_processo → 409 Conflict com mensagem sobre número do processo")]
    public async Task Handle_23505_NumProcesso_Retorna409()
    {
        var ex = SupabaseEx(PgErrorJson("23505",
            "duplicate key value violates unique constraint \"processo_num_processo_key\"",
            details: "Key (num_processo)=(0000001-01.2026.8.26.0001) already exists."));

        var (status, body) = await InvokeComExcecao(ex);

        status.Should().Be(409);
        body.Should().Contain("processo");
        body.Should().Contain("409");
    }

    [Fact(DisplayName = "23505 CPF → 409 Conflict com mensagem sobre CPF")]
    public async Task Handle_23505_Cpf_Retorna409()
    {
        var ex = SupabaseEx(PgErrorJson("23505",
            "duplicate key value violates unique constraint \"cliente_cpf_key\"",
            details: "Key (cpf)=(123.456.789-00) already exists."));

        var (status, body) = await InvokeComExcecao(ex);

        status.Should().Be(409);
        body.Should().Contain("CPF");
    }

    [Fact(DisplayName = "23505 genérico → 409 Conflict")]
    public async Task Handle_23505_Generico_Retorna409()
    {
        var ex = SupabaseEx(PgErrorJson("23505",
            "duplicate key value violates unique constraint \"alguma_chave_key\"",
            details: "Key (campo)=(valor) already exists."));

        var (status, body) = await InvokeComExcecao(ex);

        status.Should().Be(409);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  23503 — Foreign key violation
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "23503 → 422 Unprocessable Entity com menção ao campo")]
    public async Task Handle_23503_Retorna422()
    {
        var ex = SupabaseEx(PgErrorJson("23503",
            "insert or update on table \"compromisso\" violates foreign key constraint",
            column: "id_processo"));

        var (status, body) = await InvokeComExcecao(ex);

        status.Should().Be(422);
        body.Should().Contain("id_processo");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  PGRST102 — Batch insert com chaves JSON diferentes
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "PGRST102 → 400 com mensagem sobre importação em lote")]
    public async Task Handle_PGRST102_Retorna400()
    {
        var ex = SupabaseEx("{\"code\":\"PGRST102\",\"message\":\"All object keys must match\"}");

        var (status, body) = await InvokeComExcecao(ex);

        status.Should().Be(400);
        body.Should().Contain("lote");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  401 — Token expirado / inválido (simula sessão Supabase expirada)
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "HttpRequestException 401 → 401 com mensagem de sessão expirada")]
    public async Task Handle_401_Retorna401ComMensagemSessaoExpirada()
    {
        var ex = new HttpRequestException(
            "Supabase PostgREST 401 Unauthorized: {\"message\":\"JWT expired\"}",
            inner: null,
            statusCode: HttpStatusCode.Unauthorized);

        var (status, body) = await InvokeComExcecao(ex);

        status.Should().Be(401);
        body.Should().Contain("sessão");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  EXCEÇÕES DE DOMÍNIO
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "ArgumentException → 400 com a mensagem da exceção")]
    public async Task Handle_ArgumentException_Retorna400()
    {
        var ex = new ArgumentException("Número do processo inválido.");

        var (status, body) = await InvokeComExcecao(ex);

        status.Should().Be(400);
        body.Should().Contain("Número do processo inválido");
    }

    [Fact(DisplayName = "KeyNotFoundException → 404")]
    public async Task Handle_KeyNotFoundException_Retorna404()
    {
        var ex = new KeyNotFoundException("Registro não encontrado.");

        var (status, body) = await InvokeComExcecao(ex);

        status.Should().Be(404);
    }

    [Fact(DisplayName = "UnauthorizedAccessException → 401")]
    public async Task Handle_UnauthorizedAccessException_Retorna401()
    {
        var (status, _) = await InvokeComExcecao(new UnauthorizedAccessException());

        status.Should().Be(401);
    }

    [Fact(DisplayName = "Exceção genérica → 500 com mensagem genérica")]
    public async Task Handle_ExcecaoGenerica_Retorna500()
    {
        var ex = new InvalidOperationException("Erro interno qualquer.");

        var (status, body) = await InvokeComExcecao(ex);

        status.Should().Be(500);
        body.Should().Contain("inesperado");
    }

    // ════════════════════════════════════════════════════════════════════════
    //  ESTRUTURA DO JSON DE RESPOSTA
    // ════════════════════════════════════════════════════════════════════════

    [Fact(DisplayName = "Resposta sempre contém success:false, error.message e correlationId")]
    public async Task Handle_RespostaContemEstruturaPadrao()
    {
        var (_, body) = await InvokeComExcecao(new Exception("test"));

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeFalse();
        root.TryGetProperty("error", out var error).Should().BeTrue();
        error.TryGetProperty("message", out _).Should().BeTrue();
        root.TryGetProperty("correlationId", out _).Should().BeTrue();
    }
}

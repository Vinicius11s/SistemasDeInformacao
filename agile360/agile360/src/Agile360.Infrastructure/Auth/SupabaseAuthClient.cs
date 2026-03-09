using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Agile360.Infrastructure.Auth;

public class SupabaseAuthClient
{
    private readonly HttpClient _http;
    private readonly SupabaseAuthOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public SupabaseAuthClient(HttpClient http, IOptions<SupabaseAuthOptions> options)
    {
        _http = http;
        _options = options.Value;
        _http.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/auth/v1/");
        _http.DefaultRequestHeaders.Add("apikey", _options.AnonKey);
        _http.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<SupabaseSignUpResponse?> SignUpAsync(string email, string password, object? data = null, CancellationToken ct = default)
    {
        var body = new { email, password, data };
        var res = await _http.PostAsJsonAsync("signup", body, JsonOptions, ct);
        return await ReadAsResultAsync<SupabaseSignUpResponse>(res, ct);
    }

    public async Task<SupabaseTokenResponse?> TokenAsync(string email, string password, CancellationToken ct = default)
    {
        var body = new { email, password, grant_type = "password" };
        var res = await _http.PostAsJsonAsync("token?grant_type=password", body, JsonOptions, ct); 
        return await ReadAsResultAsync<SupabaseTokenResponse>(res, ct);
    }

    public async Task<SupabaseTokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var body = new { refresh_token = refreshToken, grant_type = "refresh_token" };
        var res = await _http.PostAsJsonAsync("token?grant_type=refresh_token", body, JsonOptions, ct);
        return await ReadAsResultAsync<SupabaseTokenResponse>(res, ct);
    }

    public async Task<bool> LogoutAsync(string accessToken, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "logout");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        req.Content = new StringContent("{}", Encoding.UTF8, "application/json");
        var res = await _http.SendAsync(req, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> RecoverAsync(string email, CancellationToken ct = default)
    {
        var body = new { email };
        var res = await _http.PostAsJsonAsync("recover", body, JsonOptions, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<SupabaseUserResponse?> GetUserAsync(string accessToken, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "user");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var res = await _http.SendAsync(req, ct);
        return await ReadAsResultAsync<SupabaseUserResponse>(res, ct);
    }

    public async Task<bool> UpdatePasswordAsync(string accessToken, string newPassword, CancellationToken ct = default)
    {
        var body = new { password = newPassword };
        using var req = new HttpRequestMessage(HttpMethod.Put, "user");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        req.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        var res = await _http.SendAsync(req, ct);
        return res.IsSuccessStatusCode;
    }

    private static async Task<T?> ReadAsResultAsync<T>(HttpResponseMessage res, CancellationToken ct)
    {
        var json = await res.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrEmpty(json)) return default;
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch
        {
            return default;
        }
    }
}

public class SupabaseSignUpResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string? TokenType { get; set; }
    public SupabaseUser? User { get; set; }
}

public class SupabaseTokenResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string? TokenType { get; set; }
    public SupabaseUser? User { get; set; }
}

public class SupabaseUser
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public JsonElement? UserMetadata { get; set; }
}

public class SupabaseUserResponse
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public JsonElement? UserMetadata { get; set; }
}

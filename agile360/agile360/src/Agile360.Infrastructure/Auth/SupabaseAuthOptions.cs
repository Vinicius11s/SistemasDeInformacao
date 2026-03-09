namespace Agile360.Infrastructure.Auth;

public class SupabaseAuthOptions
{
    public const string SectionName = "Supabase";

    public string BaseUrl { get; set; } = string.Empty;
    public string AnonKey { get; set; } = string.Empty;

    /// <summary>
    /// Service role key — bypassa o RLS do Supabase.
    /// Use exclusivamente em operações server-side (AuthService, admin).
    /// NUNCA exponha esta chave ao cliente.
    /// </summary>
    public string ServiceRoleKey { get; set; } = string.Empty;
}

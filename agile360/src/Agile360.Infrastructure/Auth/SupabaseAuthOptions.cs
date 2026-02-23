namespace Agile360.Infrastructure.Auth;

public class SupabaseAuthOptions
{
    public const string SectionName = "Supabase";
    public string BaseUrl { get; set; } = string.Empty;
    public string AnonKey { get; set; } = string.Empty;
}

namespace AppFleetNexus.Security.Authentication;

/// <summary>
/// Configuration options for Supabase integration.
/// Bound from the "Supabase" section of appsettings.json.
/// </summary>
public class SupabaseOptions
{
    public const string SectionName = "Supabase";

    public string Url { get; set; } = string.Empty;
    public string AnonKey { get; set; } = string.Empty;
    public string ServiceRoleKey { get; set; } = string.Empty;
    public string JwtSecret { get; set; } = string.Empty;
}

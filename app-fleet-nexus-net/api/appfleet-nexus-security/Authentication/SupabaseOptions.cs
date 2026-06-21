namespace AppFleetNexus.Security.Authentication;

public class SupabaseOptions
{
    public string Url { get; set; } = string.Empty;
    public string AnonKey { get; set; } = string.Empty;
    public string ServiceRoleKey { get; set; } = string.Empty;
    public string JwtSecret { get; set; } = string.Empty;
}

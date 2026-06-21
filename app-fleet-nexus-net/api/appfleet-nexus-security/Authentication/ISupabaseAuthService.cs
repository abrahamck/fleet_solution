namespace AppFleetNexus.Security.Authentication;

public interface ISupabaseAuthService
{
    Task<AuthResponse> SignUpAsync(SignupRequest request);
    Task<AuthResponse> SignInAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}

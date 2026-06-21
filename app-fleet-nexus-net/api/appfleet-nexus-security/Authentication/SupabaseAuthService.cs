namespace AppFleetNexus.Security.Authentication;

public class SupabaseAuthService : ISupabaseAuthService
{
    public Task<AuthResponse> SignUpAsync(SignupRequest request) => throw new NotImplementedException();
    public Task<AuthResponse> SignInAsync(LoginRequest request) => throw new NotImplementedException();
    public Task<AuthResponse> RefreshTokenAsync(string refreshToken) => throw new NotImplementedException();
}

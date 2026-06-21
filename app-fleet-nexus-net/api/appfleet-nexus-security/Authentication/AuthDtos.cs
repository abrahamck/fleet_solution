namespace AppFleetNexus.Security.Authentication;

/// <summary>
/// Request DTO for user signup.
/// </summary>
public record SignupRequest(string Email, string Password, string? FirstName, string? LastName);

/// <summary>
/// Request DTO for user login.
/// </summary>
public record LoginRequest(string Email, string Password);

/// <summary>
/// Request DTO for token refresh.
/// </summary>
public record RefreshRequest(string RefreshToken);

/// <summary>
/// Response DTO returned by auth operations (signup, login, refresh).
/// </summary>
public record AuthResponse(string AccessToken, string RefreshToken, Guid UserId, string Email);

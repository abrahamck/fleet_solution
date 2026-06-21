using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace AppFleetNexus.Security.Authentication;

public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _options;
    private readonly ILogger<SupabaseAuthService> _logger;

    public SupabaseAuthService(
        HttpClient httpClient, 
        IOptions<SupabaseOptions> options,
        ILogger<SupabaseAuthService> _logger = null)
    {
        _httpClient = httpClient;
        _options = options.Value;
        this._logger = _logger;

        // Normalize URL: remove trailing slashes and any trailing /auth/v1 or /rest/v1 if misconfigured
        string baseUrl = _options.Url.TrimEnd('/');
        if (baseUrl.EndsWith("/auth/v1", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = baseUrl.Substring(0, baseUrl.Length - "/auth/v1".Length).TrimEnd('/');
        }
        else if (baseUrl.EndsWith("/rest/v1", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = baseUrl.Substring(0, baseUrl.Length - "/rest/v1".Length).TrimEnd('/');
        }

        _httpClient.BaseAddress = new Uri(baseUrl + "/");
        _httpClient.DefaultRequestHeaders.Add("apikey", _options.AnonKey);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AnonKey);

        _logger?.LogInformation("SupabaseAuthService initialized with BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
    }


    public async Task<AuthResponse> SignUpAsync(SignupRequest request)
    {
        var payload = new
        {
            email = request.Email,
            password = request.Password,
            data = new
            {
                first_name = request.FirstName,
                last_name = request.LastName
            }
        };

        var targetUri = new Uri(_httpClient.BaseAddress, "auth/v1/signup");
        _logger?.LogInformation("Sending SignUp request to {Uri}", targetUri);

        var response = await _httpClient.PostAsJsonAsync("auth/v1/signup", payload);
        await EnsureSuccessStatusCodeAsync(response);

        return await ParseAuthResponseAsync(response);
    }

    public async Task<AuthResponse> SignInAsync(LoginRequest request)
    {
        var payload = new
        {
            email = request.Email,
            password = request.Password
        };

        var targetUri = new Uri(_httpClient.BaseAddress, "auth/v1/token?grant_type=password");
        _logger?.LogInformation("Sending SignIn request to {Uri}", targetUri);

        var response = await _httpClient.PostAsJsonAsync("auth/v1/token?grant_type=password", payload);
        await EnsureSuccessStatusCodeAsync(response);

        return await ParseAuthResponseAsync(response);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var payload = new
        {
            refresh_token = refreshToken
        };

        var targetUri = new Uri(_httpClient.BaseAddress, "auth/v1/token?grant_type=refresh_token");
        _logger?.LogInformation("Sending RefreshToken request to {Uri}", targetUri);

        var response = await _httpClient.PostAsJsonAsync("auth/v1/token?grant_type=refresh_token", payload);
        await EnsureSuccessStatusCodeAsync(response);

        return await ParseAuthResponseAsync(response);
    }

    private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            string errorMessage = $"Supabase Auth error ({response.StatusCode})";

            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.TryGetProperty("error_description", out var descProp))
                {
                    errorMessage = descProp.GetString() ?? errorMessage;
                }
                else if (root.TryGetProperty("msg", out var msgProp))
                {
                    errorMessage = msgProp.GetString() ?? errorMessage;
                }
                else if (root.TryGetProperty("error", out var errProp))
                {
                    errorMessage = errProp.GetString() ?? errorMessage;
                }
            }
            catch
            {
                // If content is not valid JSON, fall back to default message
            }

            throw new HttpRequestException(errorMessage, null, response.StatusCode);
        }
    }

    private async Task<AuthResponse> ParseAuthResponseAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        string accessToken = "";
        string refreshToken = "";
        Guid userId = Guid.Empty;
        string email = "";

        if (root.TryGetProperty("access_token", out var tokenProp))
        {
            accessToken = tokenProp.GetString() ?? "";
        }
        if (root.TryGetProperty("refresh_token", out var refreshProp))
        {
            refreshToken = refreshProp.GetString() ?? "";
        }

        if (root.TryGetProperty("user", out var userProp))
        {
            if (userProp.TryGetProperty("id", out var idProp) && Guid.TryParse(idProp.GetString(), out var id))
            {
                userId = id;
            }
            if (userProp.TryGetProperty("email", out var emailProp))
            {
                email = emailProp.GetString() ?? "";
            }
        }
        else
        {
            if (root.TryGetProperty("id", out var idProp) && Guid.TryParse(idProp.GetString(), out var id))
            {
                userId = id;
            }
            if (root.TryGetProperty("email", out var emailProp))
            {
                email = emailProp.GetString() ?? "";
            }
        }

        return new AuthResponse(accessToken, refreshToken, userId, email);
    }
}


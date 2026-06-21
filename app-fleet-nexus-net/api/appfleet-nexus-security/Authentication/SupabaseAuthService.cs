using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace AppFleetNexus.Security.Authentication;

public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _options;

    public SupabaseAuthService(HttpClient httpClient, IOptions<SupabaseOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        // Base address
        _httpClient.BaseAddress = new Uri(_options.Url.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Add("apikey", _options.AnonKey);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AnonKey);
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


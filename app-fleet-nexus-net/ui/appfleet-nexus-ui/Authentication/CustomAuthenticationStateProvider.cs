using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using appfleet_nexus_ui.Models;

namespace appfleet_nexus_ui.Authentication;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private const string SessionKey = "auth_session";

    public CustomAuthenticationStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var sessionJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", SessionKey);
            if (string.IsNullOrWhiteSpace(sessionJson))
            {
                return CreateAnonymousState();
            }

            var session = JsonSerializer.Deserialize<UserSession>(sessionJson);
            if (session == null || string.IsNullOrEmpty(session.AccessToken))
            {
                return CreateAnonymousState();
            }

            // Decode JWT to check for expiration
            var claims = ParseClaimsFromJwt(session.AccessToken).ToList();
            var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
            if (expClaim != null && long.TryParse(expClaim.Value, out var exp))
            {
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                if (expirationTime <= DateTimeOffset.UtcNow)
                {
                    // Expired session - log out the user
                    await LogoutAsync();
                    return CreateAnonymousState();
                }
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);

            // Set Authorization header for API calls
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", session.AccessToken);

            return new AuthenticationState(principal);
        }
        catch
        {
            return CreateAnonymousState();
        }
    }

    public async Task LoginAsync(UserSession session)
    {
        var sessionJson = JsonSerializer.Serialize(session);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", SessionKey, sessionJson);

        var claims = ParseClaimsFromJwt(session.AccessToken);
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", session.AccessToken);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", SessionKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;

        var anonymousPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousPrincipal)));
    }

    private static AuthenticationState CreateAnonymousState()
    {
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2)
        {
            return Enumerable.Empty<Claim>();
        }

        var payload = parts[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs == null)
        {
            return Enumerable.Empty<Claim>();
        }

        var claims = new List<Claim>();

        foreach (var kvp in keyValuePairs)
        {
            if (kvp.Value is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        claims.Add(new Claim(kvp.Key, item.ToString()));
                    }
                }
                else
                {
                    claims.Add(new Claim(kvp.Key, element.ToString()));
                }
            }
            else
            {
                claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));
            }
        }

        // Standard mapping for Blazor Authorization components
        if (keyValuePairs.TryGetValue("email", out var email))
        {
            claims.Add(new Claim(ClaimTypes.Name, email.ToString()!));
        }
        if (keyValuePairs.TryGetValue("sub", out var sub))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.ToString()!));
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frontend.Services;

public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    private const string AccessTokenKey = "tw_access_token";

    public JwtAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync(AccessTokenKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var principal = CreateClaimsPrincipalFromToken(token);
        return new AuthenticationState(principal);
    }

    public async Task SetAccessTokenAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            await _localStorage.RemoveItemAsync(AccessTokenKey);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
            return;
        }

        await _localStorage.SetItemAsStringAsync(AccessTokenKey, token);
        var principal = CreateClaimsPrincipalFromToken(token);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    private ClaimsPrincipal CreateClaimsPrincipalFromToken(string token)
    {
        try
        {
            var jwt = _tokenHandler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}


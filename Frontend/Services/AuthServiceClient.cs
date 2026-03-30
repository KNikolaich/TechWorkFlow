using System.Net.Http.Json;
using Shared.Contracts.Auth;

namespace Frontend.Services;

public interface IAuthServiceClient
{
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}

public sealed class AuthServiceClient(HttpClient httpClient) : IAuthServiceClient
{
    private const string AuthEndpoint = "api/auth/login";

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(AuthEndpoint, request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        // Backend может реализовать явный логаут; пока заглушка
        return Task.CompletedTask;
    }
}


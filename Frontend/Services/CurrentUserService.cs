using Microsoft.AspNetCore.Components.Authorization;

namespace Frontend.Services;

public interface ICurrentUserService
{
    Task<CurrentUser> GetCurrentUserAsync();
}

public sealed class CurrentUserService(AuthenticationStateProvider authenticationStateProvider) : ICurrentUserService
{
    public async Task<CurrentUser> GetCurrentUserAsync()
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        return state.User.ToCurrentUser();
    }
}


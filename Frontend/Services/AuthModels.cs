using System.Security.Claims;

namespace Frontend.Services;

public static class AuthRoles
{
    public const string Admin = "Admin";
    public const string Supervisor = "Supervisor";
    public const string Worker = "Worker";
}

public sealed class CurrentUser
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
}

public static class ClaimsExtensions
{
    public static CurrentUser ToCurrentUser(this ClaimsPrincipal principal)
    {
        if (!principal.Identity?.IsAuthenticated ?? true)
        {
            return new CurrentUser { IsAuthenticated = false };
        }

        var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = principal.Identity?.Name ?? string.Empty;
        var fullName = principal.FindFirst("full_name")?.Value ?? string.Empty;
        var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        return new CurrentUser
        {
            Id = Guid.TryParse(id, out var guid) ? guid : Guid.Empty,
            UserName = userName,
            FullName = fullName,
            Role = role,
            IsAuthenticated = true
        };
    }
}


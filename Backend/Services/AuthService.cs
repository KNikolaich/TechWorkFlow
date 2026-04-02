using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Common.Security;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Contracts.Auth;
using Shared.Domain;

namespace Backend.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(request.Login)
                   ?? await _userManager.FindByEmailAsync(request.Login);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var valid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!valid)
        {
            return null;
        }

        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // check if user with same username or email already exists
        var existingByName = await _userManager.FindByNameAsync(request.UserName);
        if (existingByName is not null)
        {
            return null;
        }

        var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingByEmail is not null)
        {
            return null;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName,
            Role = Common.Security.AuthConstants.WorkerRole,
            Status = UserEmploymentStatus.Working,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return null;
        }

        // assign default role
        await _userManager.AddToRoleAsync(user, Common.Security.AuthConstants.WorkerRole);

        // create initial auth session
        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var existingToken = await _dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

        if (existingToken is null || !existingToken.IsActive || existingToken.User is null || !existingToken.User.IsActive)
        {
            return null;
        }

        existingToken.RevokedAt = DateTime.UtcNow;

        var response = await CreateSessionAsync(existingToken.User, cancellationToken);
        existingToken.ReplacedByToken = response.RefreshToken;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var existingToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

        if (existingToken is null || existingToken.IsRevoked)
        {
            return;
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> CreateSessionAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var accessExpires = now.AddMinutes(_jwtSettings.AccessTokenMinutes);
        var refreshExpires = now.AddDays(_jwtSettings.RefreshTokenDays);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = CreateAccessToken(user, roles, accessExpires);
        var refreshToken = CreateSecureToken();

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            CreatedAt = now,
            ExpiresAt = refreshExpires
        });
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExpires,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpires
        };
    }

    private string CreateAccessToken(ApplicationUser user, IEnumerable<string> roles, DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string CreateSecureToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}

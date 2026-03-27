using Common.Security;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Domain;

namespace Backend.Infrastructure;

public sealed class DbInitializer
{
    private readonly AppDbContext _dbContext;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AdminBootstrapSettings _adminSettings;

    public DbInitializer(
        AppDbContext dbContext,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<AdminBootstrapSettings> adminOptions)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
        _adminSettings = adminOptions.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        await EnsureRoleAsync(AuthConstants.AdminRole);
        await EnsureRoleAsync(AuthConstants.SupervisorRole);
        await EnsureRoleAsync(AuthConstants.WorkerRole);

        var admin = await _userManager.FindByNameAsync(_adminSettings.UserName);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = _adminSettings.UserName,
                Email = _adminSettings.Email,
                FullName = _adminSettings.FullName,
                IsActive = true,
                Role = AuthConstants.AdminRole
            };

            var result = await _userManager.CreateAsync(admin, _adminSettings.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create bootstrap admin: {string.Join(", ", result.Errors.Select(x => x.Description))}");
            }
        }

        if (!await _userManager.IsInRoleAsync(admin, AuthConstants.AdminRole))
        {
            await _userManager.AddToRoleAsync(admin, AuthConstants.AdminRole);
        }
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }
}

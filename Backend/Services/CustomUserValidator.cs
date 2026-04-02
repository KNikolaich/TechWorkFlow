using Microsoft.AspNetCore.Identity;
using Shared.Domain;
using System.Text.RegularExpressions;

namespace Backend.Services;

public class CustomUserValidator : IUserValidator<ApplicationUser>
{
    public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        if (string.IsNullOrWhiteSpace(user.UserName))
        {
            return IdentityResult.Failed(new IdentityError { Code = "InvalidUserName", Description = "Username is required." });
        }

        // Allow Unicode letters, digits, and common symbols
        if (!Regex.IsMatch(user.UserName, @"^[\w\d\-._@]+$", RegexOptions.None))
        {
            return IdentityResult.Failed(new IdentityError { Code = "InvalidUserName", Description = "Username can only contain letters, digits, hyphens, underscores, periods, and at signs." });
        }

        // Check for existing username
        var existingUser = await manager.FindByNameAsync(user.UserName);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            return IdentityResult.Failed(new IdentityError { Code = "DuplicateUserName", Description = "Username is already taken." });
        }

        return IdentityResult.Success;
    }
}
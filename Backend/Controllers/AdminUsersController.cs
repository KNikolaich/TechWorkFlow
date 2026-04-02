using Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Admin;
using Shared.Domain;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize]
public sealed class AdminUsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.OrderBy(x => x.UserName).ToListAsync(cancellationToken);
        var result = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(ToDto(user, roles.FirstOrDefault() ?? user.Role));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        var role = NormalizeRole(request.Role);
        if (role is null)
        {
            return BadRequest("Unsupported role.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName,
            Role = role,
            Status = UserEmploymentStatus.Working,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(string.Join("; ", createResult.Errors.Select(x => x.Description)));
        }

        await _userManager.AddToRoleAsync(user, role);
        return Ok(ToDto(user, role));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        user.Email = request.Email;
        user.FullName = request.FullName;
        user.IsActive = request.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return BadRequest(string.Join("; ", updateResult.Errors.Select(x => x.Description)));
        }

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? user.Role;
        return Ok(ToDto(user, role));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        await _userManager.DeleteAsync(user);
        return NoContent();
    }

    private static UserDto ToDto(ApplicationUser user, string role)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = role,
            Status = user.Status.ToString(),
            IsActive = user.IsActive
        };
    }

    private static string? NormalizeRole(string role)
    {
        return role switch
        {
            AuthConstants.AdminRole => AuthConstants.AdminRole,
            AuthConstants.SupervisorRole => AuthConstants.SupervisorRole,
            AuthConstants.WorkerRole => AuthConstants.WorkerRole,
            _ => null
        };
    }
}

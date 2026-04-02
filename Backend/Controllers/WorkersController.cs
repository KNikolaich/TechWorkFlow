using Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Admin;
using Shared.Domain;

namespace Backend.Controllers;

[ApiController]
[Route("api/workers")]
[Authorize]
public sealed class WorkersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public WorkersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAssignable(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .Where(x => x.IsActive && x.Status != UserEmploymentStatus.Dismissed)
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();
            if (role is AuthConstants.WorkerRole or AuthConstants.SupervisorRole)
            {
                result.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Role = role,
                    Status = user.Status.ToString(),
                    IsActive = user.IsActive
                });
            }
        }

        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "SupervisorOnly")]
    public async Task<ActionResult<UserDto>> UpdateStatus(Guid id, [FromBody] UpdateWorkerStatusRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        if (!Enum.TryParse<UserEmploymentStatus>(request.Status, true, out var status))
        {
            return BadRequest("Invalid status.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? user.Role;
        if (role is not (AuthConstants.WorkerRole or AuthConstants.SupervisorRole))
        {
            return BadRequest("Only Worker or Supervisor status can be managed here.");
        }

        user.Status = status;
        user.IsActive = status != UserEmploymentStatus.Dismissed;
        await _userManager.UpdateAsync(user);

        return Ok(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = role,
            Status = user.Status.ToString(),
            IsActive = user.IsActive
        });
    }
}

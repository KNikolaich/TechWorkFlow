using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Admin;
using Shared.Domain;

namespace Backend.Controllers;

[ApiController]
[Route("api/clubs")]
[Authorize]
public sealed class AdminClubsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AdminClubsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClubDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _dbContext.Clubs
            .OrderBy(x => x.Name)
            .Select(x => new ClubDto { Id = x.Id, Name = x.Name, Comment = x.Comment })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{clubId:guid}/zones")]
    public async Task<ActionResult<IReadOnlyList<ZoneDto>>> GetZones(Guid clubId, CancellationToken cancellationToken)
    {
        var zones = await _dbContext.Zones
            .Where(x => x.ClubId == clubId)
            .OrderBy(x => x.Name)
            .Select(x => new ZoneDto { Id = x.Id, Name = x.Name, Comment = x.Comment, ClubId = x.ClubId })
            .ToListAsync(cancellationToken);
        return Ok(zones);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ClubDto>> Create([FromBody] UpsertClubRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        var entity = new Club { Id = Guid.NewGuid(), Name = request.Name.Trim(), Comment = request.Comment };
        _dbContext.Clubs.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new ClubDto { Id = entity.Id, Name = entity.Name, Comment = entity.Comment });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ClubDto>> Update(Guid id, [FromBody] UpsertClubRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Clubs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Name = request.Name.Trim();
        entity.Comment = request.Comment;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new ClubDto { Id = entity.Id, Name = entity.Name, Comment = entity.Comment });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Clubs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        _dbContext.Clubs.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

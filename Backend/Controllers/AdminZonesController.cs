using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Admin;
using Shared.Domain;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/zones")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminZonesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AdminZonesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ZoneDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _dbContext.Zones
            .OrderBy(x => x.Name)
            .Select(x => new ZoneDto { Id = x.Id, Name = x.Name, Comment = x.Comment, ClubId = x.ClubId })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<ZoneDto>> Create([FromBody] UpsertZoneRequest request, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Clubs.AnyAsync(x => x.Id == request.ClubId, cancellationToken))
        {
            return BadRequest("Club does not exist.");
        }

        var entity = new Zone { Id = Guid.NewGuid(), Name = request.Name.Trim(), Comment = request.Comment, ClubId = request.ClubId };
        _dbContext.Zones.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new ZoneDto { Id = entity.Id, Name = entity.Name, Comment = entity.Comment, ClubId = entity.ClubId });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ZoneDto>> Update(Guid id, [FromBody] UpsertZoneRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Zones.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Name = request.Name.Trim();
        entity.Comment = request.Comment;
        entity.ClubId = request.ClubId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new ZoneDto { Id = entity.Id, Name = entity.Name, Comment = entity.Comment, ClubId = entity.ClubId });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Zones.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        _dbContext.Zones.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

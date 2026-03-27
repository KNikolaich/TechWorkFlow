using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Admin;
using Shared.Domain;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/equipments")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminEquipmentController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AdminEquipmentController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EquipmentDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _dbContext.Equipments
            .OrderBy(x => x.Name)
            .Select(x => new EquipmentDto { Id = x.Id, Name = x.Name, Comment = x.Comment })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<EquipmentDto>> Create([FromBody] UpsertEquipmentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        var entity = new Equipment { Id = Guid.NewGuid(), Name = request.Name.Trim(), Comment = request.Comment };
        _dbContext.Equipments.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new EquipmentDto { Id = entity.Id, Name = entity.Name, Comment = entity.Comment });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EquipmentDto>> Update(Guid id, [FromBody] UpsertEquipmentRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Equipments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Name = request.Name.Trim();
        entity.Comment = request.Comment;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new EquipmentDto { Id = entity.Id, Name = entity.Name, Comment = entity.Comment });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Equipments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        _dbContext.Equipments.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

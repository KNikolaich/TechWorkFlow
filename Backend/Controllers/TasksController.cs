using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Tasks;

namespace Backend.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    [Authorize(Policy = "SupervisorOnly")]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _taskService.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "SupervisorOnly")]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskService.CreateAsync(request, TryGetCurrentUserId(), cancellationToken);
            return CreatedAtAction(nameof(GetAll), new { id = task.Id }, task);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SupervisorOnly")]
    public async Task<ActionResult<TaskDto>> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskService.UpdateAsync(id, request, TryGetCurrentUserId(), cancellationToken);
            if (task is null)
            {
                return NotFound();
            }
            return Ok(task);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "WorkerOnly")]
    public async Task<ActionResult<TaskDto>> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var task = await _taskService.UpdateStatusAsync(id, request, TryGetCurrentUserId(), cancellationToken);
        if (task is null)
        {
            return NotFound();
        }
        return Ok(task);
    }

    private Guid TryGetCurrentUserId()
    {
        var rawId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        return Guid.TryParse(rawId, out var userId) ? userId : Guid.Empty;
    }
}

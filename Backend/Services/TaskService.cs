using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Tasks;
using Shared.Domain;

namespace Backend.Services;

public sealed class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;
    private readonly ITaskNumberGenerator _taskNumberGenerator;
    private readonly ITaskHistoryService _taskHistoryService;

    public TaskService(
        AppDbContext dbContext,
        ITaskNumberGenerator taskNumberGenerator,
        ITaskHistoryService taskHistoryService)
    {
        _dbContext = dbContext;
        _taskNumberGenerator = taskNumberGenerator;
        _taskHistoryService = taskHistoryService;
    }

    public async Task<IReadOnlyList<TaskDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks
            .OrderByDescending(x => x.CreatedAt)
            .Select(MapToDtoExpr())
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        ValidateCreate(request);
        await ValidateExecutorAsync(request.ExecutorId, cancellationToken);

        var now = DateTime.UtcNow;
        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            TaskNumber = await _taskNumberGenerator.GenerateNextAsync(cancellationToken),
            CreatedAt = now,
            EquipmentId = request.EquipmentId,
            ClubId = request.ClubId,
            ZoneId = request.ZoneId,
            Description = request.Description,
            ExecutorId = request.ExecutorId,
            EstimatedTime = request.EstimatedTime,
            Deadline = request.Deadline ?? now.AddDays(3),
            StatusText = request.StatusText,
            IsArchived = false,
            CreatedById = userId,
            UpdatedById = null
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _taskHistoryService.LogCreateAsync(task, userId, cancellationToken);
        return MapToDto(task);
    }

    public async Task<TaskDto?> UpdateAsync(Guid taskId, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        await ValidateExecutorAsync(request.ExecutorId, cancellationToken);
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        var snapshot = Clone(task);

        task.EquipmentId = request.EquipmentId;
        task.ClubId = request.ClubId;
        task.ZoneId = request.ZoneId;
        task.Description = request.Description;
        task.ExecutorId = request.ExecutorId;
        task.EstimatedTime = request.EstimatedTime;
        task.Deadline = request.Deadline;
        task.StatusText = request.StatusText;
        task.IsArchived = request.IsArchived;
        task.UpdatedById = userId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _taskHistoryService.LogUpdateAsync(snapshot, task, userId, cancellationToken);
        return MapToDto(task);
    }

    public async Task<TaskDto?> UpdateStatusAsync(Guid taskId, UpdateTaskStatusRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == taskId && x.ExecutorId == userId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        var snapshot = Clone(task);

        task.StatusText = request.StatusText;
        task.UpdatedById = userId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _taskHistoryService.LogUpdateAsync(snapshot, task, userId, cancellationToken);
        return MapToDto(task);
    }

    private static void ValidateCreate(CreateTaskRequest request)
    {
        if (request.EquipmentId == Guid.Empty || request.ClubId == Guid.Empty || request.ZoneId == Guid.Empty)
        {
            throw new ArgumentException("EquipmentId, ClubId and ZoneId are required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length < 10)
        {
            throw new ArgumentException("Description must contain at least 10 characters.");
        }
    }

    private async Task ValidateExecutorAsync(Guid? executorId, CancellationToken cancellationToken)
    {
        if (!executorId.HasValue)
        {
            return;
        }

        var isAssignable = await _dbContext.Users.AnyAsync(
            x => x.Id == executorId.Value && x.IsActive && x.Status != UserEmploymentStatus.Dismissed,
            cancellationToken);

        if (!isAssignable)
        {
            throw new ArgumentException("Executor must be active and not dismissed.");
        }
    }

    private static System.Linq.Expressions.Expression<Func<WorkTask, TaskDto>> MapToDtoExpr()
    {
        return x => new TaskDto
        {
            Id = x.Id,
            TaskNumber = x.TaskNumber,
            CreatedAt = x.CreatedAt,
            EquipmentId = x.EquipmentId,
            ClubId = x.ClubId,
            ZoneId = x.ZoneId,
            Description = x.Description,
            ExecutorId = x.ExecutorId,
            EstimatedTime = x.EstimatedTime,
            Deadline = x.Deadline,
            StatusText = x.StatusText,
            IsArchived = x.IsArchived,
            CreatedById = x.CreatedById,
            UpdatedById = x.UpdatedById
        };
    }

    private static TaskDto MapToDto(WorkTask x)
    {
        return new TaskDto
        {
            Id = x.Id,
            TaskNumber = x.TaskNumber,
            CreatedAt = x.CreatedAt,
            EquipmentId = x.EquipmentId,
            ClubId = x.ClubId,
            ZoneId = x.ZoneId,
            Description = x.Description,
            ExecutorId = x.ExecutorId,
            EstimatedTime = x.EstimatedTime,
            Deadline = x.Deadline,
            StatusText = x.StatusText,
            IsArchived = x.IsArchived,
            CreatedById = x.CreatedById,
            UpdatedById = x.UpdatedById
        };
    }

    private static WorkTask Clone(WorkTask x)
    {
        return new WorkTask
        {
            Id = x.Id,
            TaskNumber = x.TaskNumber,
            CreatedAt = x.CreatedAt,
            EquipmentId = x.EquipmentId,
            ClubId = x.ClubId,
            ZoneId = x.ZoneId,
            Description = x.Description,
            ExecutorId = x.ExecutorId,
            EstimatedTime = x.EstimatedTime,
            Deadline = x.Deadline,
            StatusText = x.StatusText,
            IsArchived = x.IsArchived,
            CreatedById = x.CreatedById,
            UpdatedById = x.UpdatedById
        };
    }
}

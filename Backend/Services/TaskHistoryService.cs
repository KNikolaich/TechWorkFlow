using System.Text.Json;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Backend.Services;

public sealed class TaskHistoryService : ITaskHistoryService
{
    private readonly AppDbContext _dbContext;

    public TaskHistoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogCreateAsync(WorkTask task, Guid changedById, CancellationToken cancellationToken = default)
    {
        var history = new TaskHistory
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            ChangedById = changedById,
            ChangedAt = DateTime.UtcNow,
            Action = TaskHistoryAction.Created,
            OldValues = "{}",
            NewValues = JsonSerializer.Serialize(ToAuditObject(task))
        };

        _dbContext.TaskHistories.Add(history);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task LogUpdateAsync(WorkTask oldTask, WorkTask newTask, Guid changedById, CancellationToken cancellationToken = default)
    {
        var history = new TaskHistory
        {
            Id = Guid.NewGuid(),
            TaskId = newTask.Id,
            ChangedById = changedById,
            ChangedAt = DateTime.UtcNow,
            Action = TaskHistoryAction.Updated,
            OldValues = JsonSerializer.Serialize(ToAuditObject(oldTask)),
            NewValues = JsonSerializer.Serialize(ToAuditObject(newTask))
        };

        _dbContext.TaskHistories.Add(history);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CleanupOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        var oldRecords = await _dbContext.TaskHistories
            .Where(x => x.ChangedAt < cutoffUtc)
            .ToListAsync(cancellationToken);

        if (oldRecords.Count == 0)
        {
            return 0;
        }

        _dbContext.TaskHistories.RemoveRange(oldRecords);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return oldRecords.Count;
    }

    private static object ToAuditObject(WorkTask task)
    {
        return new
        {
            task.Id,
            task.TaskNumber,
            task.CreatedAt,
            task.EquipmentId,
            task.ClubId,
            task.ZoneId,
            task.Description,
            task.ExecutorId,
            task.EstimatedTime,
            task.Deadline,
            task.StatusText,
            task.IsArchived,
            task.CreatedById,
            task.UpdatedById
        };
    }
}

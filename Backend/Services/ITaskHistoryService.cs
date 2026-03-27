using Shared.Domain;

namespace Backend.Services;

public interface ITaskHistoryService
{
    Task LogCreateAsync(WorkTask task, Guid changedById, CancellationToken cancellationToken = default);
    Task LogUpdateAsync(WorkTask oldTask, WorkTask newTask, Guid changedById, CancellationToken cancellationToken = default);
    Task<int> CleanupOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);
}

using Shared.Contracts.Tasks;

namespace Backend.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TaskDto> CreateAsync(CreateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskDto?> UpdateAsync(Guid taskId, UpdateTaskRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskDto?> UpdateStatusAsync(Guid taskId, UpdateTaskStatusRequest request, Guid userId, CancellationToken cancellationToken = default);
}

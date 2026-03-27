using Backend.Services;
using Quartz;

namespace Backend.Jobs;

public sealed class TaskHistoryCleanupJob : IJob
{
    private readonly ITaskHistoryService _taskHistoryService;

    public TaskHistoryCleanupJob(ITaskHistoryService taskHistoryService)
    {
        _taskHistoryService = taskHistoryService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var cutoff = DateTime.UtcNow.AddDays(-365);
        await _taskHistoryService.CleanupOlderThanAsync(cutoff, context.CancellationToken);
    }
}

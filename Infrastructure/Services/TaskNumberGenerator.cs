using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class TaskNumberGenerator : ITaskNumberGenerator
{
    private readonly AppDbContext _dbContext;

    public TaskNumberGenerator(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> GenerateNextAsync(CancellationToken cancellationToken = default)
    {
        var provider = _dbContext.Database.ProviderName ?? string.Empty;

        if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                "CREATE SEQUENCE IF NOT EXISTS task_number_seq START WITH 1 INCREMENT BY 1;",
                cancellationToken);

            var result = await _dbContext.Database.SqlQueryRaw<long>("SELECT nextval('task_number_seq')")
                .SingleAsync(cancellationToken);

            return (int)result;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var currentMax = await _dbContext.Tasks.MaxAsync(x => (int?)x.TaskNumber, cancellationToken) ?? 0;
        var next = currentMax + 1;
        await transaction.CommitAsync(cancellationToken);
        return next;
    }
}

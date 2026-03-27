namespace Infrastructure.Services;

public interface ITaskNumberGenerator
{
    Task<int> GenerateNextAsync(CancellationToken cancellationToken = default);
}

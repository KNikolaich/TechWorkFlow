using Backend.Services;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Backend.Tests.Services;

public sealed class TaskHistoryServiceTests
{
    [Fact]
    public async Task CleanupOlderThanAsync_Should_Delete_Old_Records()
    {
        // Arrange
        await using var db = CreateDbContext();
        db.TaskHistories.AddRange(
            new TaskHistory
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                ChangedById = Guid.NewGuid(),
                ChangedAt = DateTime.UtcNow.AddDays(-400),
                OldValues = "{}",
                NewValues = "{}",
                Action = TaskHistoryAction.Updated
            },
            new TaskHistory
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                ChangedById = Guid.NewGuid(),
                ChangedAt = DateTime.UtcNow.AddDays(-10),
                OldValues = "{}",
                NewValues = "{}",
                Action = TaskHistoryAction.Updated
            });
        await db.SaveChangesAsync();

        var service = new TaskHistoryService(db);

        // Act
        var deleted = await service.CleanupOlderThanAsync(DateTime.UtcNow.AddDays(-365));

        // Assert
        deleted.Should().Be(1);
        (await db.TaskHistories.CountAsync()).Should().Be(1);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"twf-history-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }
}

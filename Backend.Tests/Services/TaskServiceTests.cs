using Backend.Services;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Contracts.Tasks;

namespace Backend.Tests.Services;

public sealed class TaskServiceTests
{
    [Fact]
    public async Task CreateAsync_Should_Create_Task_With_Generated_Number_And_Default_Deadline()
    {
        // Arrange
        await using var db = CreateDbContext();
        var generator = new Mock<ITaskNumberGenerator>();
        generator.Setup(x => x.GenerateNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(42);

        var history = new Mock<ITaskHistoryService>();
        var service = new TaskService(db, generator.Object, history.Object);

        var request = new CreateTaskRequest
        {
            EquipmentId = Guid.NewGuid(),
            ClubId = Guid.NewGuid(),
            ZoneId = Guid.NewGuid(),
            Description = "Need urgent equipment service",
            StatusText = "New"
        };

        // Act
        var result = await service.CreateAsync(request, Guid.NewGuid());

        // Assert
        result.TaskNumber.Should().Be(42);
        result.Deadline.Should().BeAfter(result.CreatedAt.AddDays(3).AddMinutes(-1));
        result.Deadline.Should().BeBefore(result.CreatedAt.AddDays(3).AddMinutes(1));
        history.Verify(x => x.LogCreateAsync(It.IsAny<Shared.Domain.WorkTask>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Required_Fields_Missing()
    {
        // Arrange
        await using var db = CreateDbContext();
        var service = new TaskService(
            db,
            Mock.Of<ITaskNumberGenerator>(),
            Mock.Of<ITaskHistoryService>());

        var request = new CreateTaskRequest
        {
            EquipmentId = Guid.Empty,
            ClubId = Guid.Empty,
            ZoneId = Guid.Empty,
            Description = "short"
        };

        // Act
        var action = async () => await service.CreateAsync(request, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateStatusAsync_Should_Update_Only_Executor_Task()
    {
        // Arrange
        await using var db = CreateDbContext();
        var executorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var task = new Shared.Domain.WorkTask
        {
            Id = Guid.NewGuid(),
            TaskNumber = 1,
            CreatedAt = DateTime.UtcNow,
            EquipmentId = Guid.NewGuid(),
            ClubId = Guid.NewGuid(),
            ZoneId = Guid.NewGuid(),
            Description = "Initial description text",
            ExecutorId = executorId,
            Deadline = DateTime.UtcNow.AddDays(1),
            StatusText = "New",
            CreatedById = Guid.NewGuid()
        };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var history = new Mock<ITaskHistoryService>();
        var service = new TaskService(db, Mock.Of<ITaskNumberGenerator>(), history.Object);

        // Act
        var denied = await service.UpdateStatusAsync(task.Id, new UpdateTaskStatusRequest { StatusText = "Done" }, otherUserId);
        var allowed = await service.UpdateStatusAsync(task.Id, new UpdateTaskStatusRequest { StatusText = "In progress" }, executorId);

        // Assert
        denied.Should().BeNull();
        allowed.Should().NotBeNull();
        allowed!.StatusText.Should().Be("In progress");
        history.Verify(x => x.LogUpdateAsync(It.IsAny<Shared.Domain.WorkTask>(), It.IsAny<Shared.Domain.WorkTask>(), executorId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"twf-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }
}

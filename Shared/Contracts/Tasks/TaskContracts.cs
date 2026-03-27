namespace Shared.Contracts.Tasks;

public sealed class CreateTaskRequest
{
    public Guid EquipmentId { get; set; }
    public Guid ClubId { get; set; }
    public Guid ZoneId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? ExecutorId { get; set; }
    public int? EstimatedTime { get; set; }
    public DateTime? Deadline { get; set; }
    public string StatusText { get; set; } = string.Empty;
}

public sealed class TaskDto
{
    public Guid Id { get; set; }
    public int TaskNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid EquipmentId { get; set; }
    public Guid ClubId { get; set; }
    public Guid ZoneId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? ExecutorId { get; set; }
    public int? EstimatedTime { get; set; }
    public DateTime Deadline { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}

public sealed class UpdateTaskRequest
{
    public Guid EquipmentId { get; set; }
    public Guid ClubId { get; set; }
    public Guid ZoneId { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? ExecutorId { get; set; }
    public int? EstimatedTime { get; set; }
    public DateTime Deadline { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
}

public sealed class UpdateTaskStatusRequest
{
    public string StatusText { get; set; } = string.Empty;
}

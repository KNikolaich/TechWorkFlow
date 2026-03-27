using Microsoft.AspNetCore.Identity;

namespace Shared.Domain;

public enum UserEmploymentStatus
{
    Working = 0,
    OnVacation = 1,
    Dismissed = 2
}

public enum TaskHistoryAction
{
    Created = 0,
    Updated = 1,
    Deleted = 2,
    StatusChanged = 3,
    Archived = 4
}

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public UserEmploymentStatus Status { get; set; } = UserEmploymentStatus.Working;
    public bool IsActive { get; set; } = true;
}

public sealed class Equipment
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public sealed class Club
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
}

public sealed class Zone
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public Guid ClubId { get; set; }
    public Club? Club { get; set; }
}

public sealed class WorkTask
{
    public Guid Id { get; set; }
    public int TaskNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }
    public Guid ClubId { get; set; }
    public Club? Club { get; set; }
    public Guid ZoneId { get; set; }
    public Zone? Zone { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? ExecutorId { get; set; }
    public ApplicationUser? Executor { get; set; }
    public int? EstimatedTime { get; set; }
    public DateTime Deadline { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public Guid CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }
    public Guid? UpdatedById { get; set; }
    public ApplicationUser? UpdatedBy { get; set; }
}

public sealed class TaskHistory
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public WorkTask? Task { get; set; }
    public Guid ChangedById { get; set; }
    public ApplicationUser? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string OldValues { get; set; } = "{}";
    public string NewValues { get; set; } = "{}";
    public TaskHistoryAction Action { get; set; }
}

public sealed class TaskComment
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public WorkTask? Task { get; set; }
    public Guid CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Text { get; set; } = string.Empty;
}

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;
}

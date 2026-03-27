namespace Shared.Contracts.Admin;

public sealed class EquipmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public sealed class ClubDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public sealed class ZoneDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public Guid ClubId { get; set; }
}

public sealed class UpsertEquipmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public sealed class UpsertClubRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public sealed class UpsertZoneRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public Guid ClubId { get; set; }
}

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class CreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class UpdateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class UpdateWorkerStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

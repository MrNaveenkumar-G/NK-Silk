using NKSilk.Domain.Enums;

namespace NKSilk.Application.ViewModels;

public class RoleVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MemberCount { get; set; }
}

/// <summary>Role-assignment view for one customer.</summary>
public class CustomerAccessVm
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IReadOnlyList<RoleVm> AllRoles { get; set; } = new List<RoleVm>();
    public IReadOnlyList<string> AssignedRoles { get; set; } = new List<string>();
}

public class AuditLogVm
{
    public int Id { get; set; }
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

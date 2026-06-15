using NKSilk.Domain.Common;
using NKSilk.Domain.Enums;

namespace NKSilk.Domain.Entities;

/// <summary>
/// An immutable record of a create/update/delete on a tracked entity, captured automatically
/// in the DbContext. Provides a change trail for compliance and back-office troubleshooting.
/// </summary>
public class AuditLog : BaseEntity
{
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int EntityId { get; set; }

    /// <summary>Customer id of the acting user, if known.</summary>
    public int? UserId { get; set; }
    public string UserName { get; set; } = "system";

    /// <summary>Short human-readable summary (e.g. changed fields).</summary>
    public string? Details { get; set; }
}

namespace NKSilk.Domain.Common;

/// <summary>
/// Base type for all persisted entities. Provides identity, audit timestamps
/// and a soft-delete flag (rows are filtered out globally, never hard-deleted).
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
}

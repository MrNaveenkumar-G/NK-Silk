using NKSilk.Domain.Common;
using NKSilk.Domain.Enums;

namespace NKSilk.Domain.Entities;

/// <summary>
/// A customer-initiated return request against a delivered order. Moves through
/// Requested → Approved/Rejected → PickedUp → Refunded under admin control.
/// </summary>
public class Return : BaseEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public ReturnStatus Status { get; set; } = ReturnStatus.Requested;
    public ReturnReason Reason { get; set; }
    public string? Comments { get; set; }

    /// <summary>Amount to refund once the return is settled (sum of returned lines).</summary>
    public decimal RefundAmount { get; set; }

    /// <summary>Admin note set when approving/rejecting (e.g. rejection rationale).</summary>
    public string? ResolutionNote { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public ICollection<ReturnItem> Items { get; set; } = new List<ReturnItem>();
}

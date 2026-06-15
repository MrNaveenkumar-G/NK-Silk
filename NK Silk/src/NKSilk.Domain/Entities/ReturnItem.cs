using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>
/// A single returned line. Descriptors are snapshotted from the original order line so the
/// return record stays accurate even if the catalogue changes; OrderItemId links back to it.
/// </summary>
public class ReturnItem : BaseEntity
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    // Snapshot of the returned product/variant.
    public string ProductName { get; set; } = string.Empty;
    public string VariantSku { get; set; } = string.Empty;
    public string? ColorName { get; set; }
    public string? SizeName { get; set; }

    public int ReturnId { get; set; }
    public Return Return { get; set; } = null!;

    public int OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; } = null!;

    /// <summary>The variant to restock on refund.</summary>
    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
}

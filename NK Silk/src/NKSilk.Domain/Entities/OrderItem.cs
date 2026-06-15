using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>
/// A line in an order. Product/variant descriptors are snapshotted so the order
/// stays accurate even if the catalogue later changes.
/// </summary>
public class OrderItem : BaseEntity
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    // Snapshot fields (immutable historical record of what was bought)
    public string ProductName { get; set; } = string.Empty;
    public string VariantSku { get; set; } = string.Empty;
    public string? ColorName { get; set; }
    public string? SizeName { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
}

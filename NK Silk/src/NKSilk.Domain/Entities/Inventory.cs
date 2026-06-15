using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>Stock record for a single variant. One-to-one with ProductVariant.</summary>
public class Inventory : BaseEntity
{
    public int QuantityOnHand { get; set; }
    /// <summary>Units held by in-flight (unconfirmed) orders / carts.</summary>
    public int QuantityReserved { get; set; }
    public int ReorderLevel { get; set; }

    public int QuantityAvailable => QuantityOnHand - QuantityReserved;

    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
}

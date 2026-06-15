using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>A line in a cart: a variant + quantity. Unit price is snapshotted at add time.</summary>
public class CartItem : BaseEntity
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public decimal LineTotal => UnitPrice * Quantity;
}

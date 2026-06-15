using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>A product a customer has saved to their wishlist. Unique per (customer, product).</summary>
public class WishlistItem : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}

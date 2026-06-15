using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>
/// A shopping cart. Guest carts are keyed by an opaque cookie token (CartKey);
/// once a customer logs in the cart can be associated to CustomerId.
/// </summary>
public class Cart : BaseEntity
{
    /// <summary>Opaque GUID stored in the visitor's cookie for guest carts.</summary>
    public string CartKey { get; set; } = string.Empty;

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

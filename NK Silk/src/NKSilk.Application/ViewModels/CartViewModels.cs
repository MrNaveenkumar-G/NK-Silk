namespace NKSilk.Application.ViewModels;

public class CartLineVm
{
    public int CartItemId { get; set; }
    public int ProductVariantId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ColorName { get; set; }
    public string? SizeName { get; set; }
    /// <summary>Effective (offer-adjusted) unit price actually charged.</summary>
    public decimal UnitPrice { get; set; }
    /// <summary>List price before any offer (shown struck-through when discounted).</summary>
    public decimal OriginalUnitPrice { get; set; }
    public string? OfferTitle { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
    public bool HasOffer => OriginalUnitPrice > UnitPrice;
}

public class CartVm
{
    public IReadOnlyList<CartLineVm> Lines { get; set; } = new List<CartLineVm>();
    /// <summary>Sum of effective line totals (offers already applied).</summary>
    public decimal SubTotal => Lines.Sum(l => l.LineTotal);
    /// <summary>Savings from per-product offers (already reflected in SubTotal).</summary>
    public decimal OfferSavings { get; set; }
    /// <summary>Additional savings from matched combo packs (subtracted from SubTotal).</summary>
    public decimal ComboSavings { get; set; }
    public IReadOnlyList<string> AppliedCombos { get; set; } = new List<string>();
    public decimal Payable => SubTotal - ComboSavings;
    public int ItemCount => Lines.Sum(l => l.Quantity);
    public bool IsEmpty => Lines.Count == 0;
}

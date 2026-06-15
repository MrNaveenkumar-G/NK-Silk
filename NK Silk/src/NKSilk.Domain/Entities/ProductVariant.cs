using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>
/// A concrete buyable SKU = product + colour + size, with its own price and stock.
/// </summary>
public class ProductVariant : BaseEntity
{
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? MrpPrice { get; set; }
    public bool IsActive { get; set; } = true;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int? ColorId { get; set; }
    public Color? Color { get; set; }

    public int? SizeId { get; set; }
    public Size? Size { get; set; }

    public Inventory? Inventory { get; set; }
}

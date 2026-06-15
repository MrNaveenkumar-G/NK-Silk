using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>Gallery image for a product. One image per row, ordered for the carousel.</summary>
public class ProductImage : BaseEntity
{
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}

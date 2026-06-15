using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>
/// A sellable catalogue item. Textile-specific attributes (fabric, GSM, wash care,
/// occasion) live here; price/stock that vary by colour &amp; size live on ProductVariant.
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }

    // Pricing shown on the card; authoritative per-variant pricing is on ProductVariant.
    public decimal BasePrice { get; set; }
    public decimal? MrpPrice { get; set; }

    public string Sku { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }

    // ---- Textile-specific attributes ----
    public string? FabricType { get; set; }          // Silk, Cotton, Linen, Georgette...
    public string? MaterialComposition { get; set; } // "100% Pure Silk"
    public int? Gsm { get; set; }                    // grams per square metre
    public string? WashCare { get; set; }            // "Dry clean only"
    public string? Occasion { get; set; }            // Wedding, Festive, Casual
    public string? Collection { get; set; }          // Seasonal collection name

    // Relationships
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? SubCategoryId { get; set; }
    public SubCategory? SubCategory { get; set; }

    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    /// <summary>Marketplace seller; null for first-party (house) products.</summary>
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

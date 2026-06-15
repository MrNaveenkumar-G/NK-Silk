using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>Top-level catalog grouping, e.g. "Sarees", "Men's Wear", "Kids".</summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

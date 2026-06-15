using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>Second-level grouping under a Category, e.g. "Kanchipuram Silk" under "Sarees".</summary>
public class SubCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

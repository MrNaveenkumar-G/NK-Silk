using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>Manufacturer / label of a product (own-label or vendor brand).</summary>
public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

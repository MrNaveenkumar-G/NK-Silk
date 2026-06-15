using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>A curated bundle of products sold together at a special combo price.</summary>
public class ComboPack : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    /// <summary>The bundle price charged when all components are in the cart.</summary>
    public decimal ComboPrice { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ComboPackItem> Items { get; set; } = new List<ComboPackItem>();
}

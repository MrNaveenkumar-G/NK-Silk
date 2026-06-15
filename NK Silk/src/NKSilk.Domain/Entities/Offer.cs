using NKSilk.Domain.Common;
using NKSilk.Domain.Enums;

namespace NKSilk.Domain.Entities;

/// <summary>
/// A time-boxed promotional campaign. Applies a percentage or flat discount to the whole
/// store, a category, or a single product. Higher <see cref="Priority"/> wins on ties.
/// </summary>
public class Offer : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BannerImageUrl { get; set; }

    public OfferType OfferType { get; set; }
    public decimal Value { get; set; }              // percent (0-100) or flat ₹ amount

    public OfferScope Scope { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
}

using System.ComponentModel.DataAnnotations;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.ViewModels;

/// <summary>Active offer shown as a storefront banner / on the offers page.</summary>
public class OfferCardVm
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BannerImageUrl { get; set; }
    public OfferType OfferType { get; set; }
    public decimal Value { get; set; }
    public OfferScope Scope { get; set; }
    public string? ScopeName { get; set; }      // category or product name
    public string? TargetSlug { get; set; }     // category slug or product slug for the CTA
    public DateTime EndsAtUtc { get; set; }

    public string Headline => OfferType == OfferType.PercentageOff
        ? $"{Value:0.##}% OFF"
        : $"₹{Value:N0} OFF";
}

public class AdminOfferVm
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(160)]
    public string? Slug { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    [Display(Name = "Banner image URL")]
    public string? BannerImageUrl { get; set; }

    [Display(Name = "Discount type")]
    public OfferType OfferType { get; set; } = OfferType.PercentageOff;

    [Range(0.01, 1000000)]
    public decimal Value { get; set; }

    public OfferScope Scope { get; set; } = OfferScope.EntireStore;
    [Display(Name = "Category")] public int? CategoryId { get; set; }
    [Display(Name = "Product id")] public int? ProductId { get; set; }

    [DataType(DataType.Date)][Display(Name = "Starts")] public DateTime StartsAtUtc { get; set; } = DateTime.UtcNow.Date;
    [DataType(DataType.Date)][Display(Name = "Ends")] public DateTime EndsAtUtc { get; set; } = DateTime.UtcNow.Date.AddDays(14);
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;

    // list-only display fields
    public string? ScopeName { get; set; }
    public bool IsLive { get; set; }

    public IReadOnlyList<CategoryVm> Categories { get; set; } = new List<CategoryVm>();
}

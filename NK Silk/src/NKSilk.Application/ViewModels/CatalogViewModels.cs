namespace NKSilk.Application.ViewModels;

/// <summary>A product card as shown on listing/home pages.</summary>
public class ProductCardVm
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? FabricType { get; set; }
    public decimal Price { get; set; }
    public decimal? MrpPrice { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    /// <summary>Set when an active promotional offer applies; the offer price to show.</summary>
    public decimal? OfferPrice { get; set; }
    public string? OfferTitle { get; set; }
    public bool HasOffer => OfferPrice is decimal p && p < Price;

    public int DiscountPercent =>
        MrpPrice is > 0 && MrpPrice > Price
            ? (int)Math.Round((decimal)(MrpPrice - Price) / MrpPrice.Value * 100)
            : 0;
}

/// <summary>Paged, filterable product listing.</summary>
public class ProductListVm
{
    public IReadOnlyList<ProductCardVm> Products { get; set; } = new List<ProductCardVm>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    // Active filter context
    public string? CategorySlug { get; set; }
    public string? CategoryName { get; set; }
    public string? SearchTerm { get; set; }
}

public class VariantVm
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? MrpPrice { get; set; }
    public string? ColorName { get; set; }
    public string? ColorHex { get; set; }
    public string? SizeName { get; set; }
    public int Available { get; set; }
}

public class ProductDetailVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? MrpPrice { get; set; }

    public string? FabricType { get; set; }
    public string? MaterialComposition { get; set; }
    public int? Gsm { get; set; }
    public string? WashCare { get; set; }
    public string? Occasion { get; set; }

    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }

    public int CategoryId { get; set; }
    public IReadOnlyList<string> ImageUrls { get; set; } = new List<string>();
    public IReadOnlyList<VariantVm> Variants { get; set; } = new List<VariantVm>();
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    /// <summary>Active offer applied to the base price (null when none).</summary>
    public decimal? OfferPrice { get; set; }
    public string? OfferTitle { get; set; }
    public bool HasOffer => OfferPrice is decimal p && p < BasePrice;

    // Populated by the controller from IReviewService / IWishlistService.
    public IReadOnlyList<ReviewVm> Reviews { get; set; } = new List<ReviewVm>();
    public bool InWishlist { get; set; }
}

public class CategoryVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int ProductCount { get; set; }
}

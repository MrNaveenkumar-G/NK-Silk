using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;

namespace NKSilk.Application.Services;

/// <summary>Read-side catalogue queries projected straight into view models.</summary>
public class CatalogService : ICatalogService
{
    private readonly IUnitOfWork _uow;
    private readonly IPromotionService _promotions;

    public CatalogService(IUnitOfWork uow, IPromotionService promotions)
    {
        _uow = uow;
        _promotions = promotions;
    }

    /// <summary>Sets OfferPrice/OfferTitle on cards that have an active promotional offer.</summary>
    private async Task ApplyOffersAsync(IReadOnlyList<ProductCardVm> cards, CancellationToken ct)
    {
        if (cards.Count == 0) return;
        var resolved = await _promotions.ResolveProductOffersAsync(
            cards.Select(c => (c.Id, c.CategoryId, c.Price)).ToList(), ct);
        foreach (var c in cards)
            if (resolved.TryGetValue(c.Id, out var hit))
            {
                c.OfferPrice = hit.price;
                c.OfferTitle = hit.title;
            }
    }

    public async Task<IReadOnlyList<CategoryVm>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return await _uow.Repository<Category>().Query()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryVm
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductCardVm>> GetFeaturedAsync(int take = 8, CancellationToken ct = default)
    {
        var cards = await _uow.Repository<Product>().Query()
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(take)
            .Select(ToCard)
            .ToListAsync(ct);
        await ApplyOffersAsync(cards, ct);
        return cards;
    }

    public async Task<ProductListVm> GetProductsAsync(
        string? categorySlug = null, string? search = null,
        int page = 1, int pageSize = 12, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 60) pageSize = 12;

        var query = _uow.Repository<Product>().Query().Where(p => p.IsActive);

        string? categoryName = null;
        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            query = query.Where(p => p.Category.Slug == categorySlug);
            categoryName = await _uow.Repository<Category>().Query()
                .Where(c => c.Slug == categorySlug)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(ct);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Name.Contains(term) ||
                (p.FabricType != null && p.FabricType.Contains(term)) ||
                (p.Occasion != null && p.Occasion.Contains(term)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToCard)
            .ToListAsync(ct);
        await ApplyOffersAsync(items, ct);

        return new ProductListVm
        {
            Products = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            CategorySlug = categorySlug,
            CategoryName = categoryName,
            SearchTerm = search
        };
    }

    public async Task<ProductDetailVm?> GetProductBySlugAsync(string slug, CancellationToken ct = default)
    {
        var vm = await _uow.Repository<Product>().Query()
            .Where(p => p.IsActive && p.Slug == slug)
            .Select(p => new ProductDetailVm
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                BasePrice = p.BasePrice,
                MrpPrice = p.MrpPrice,
                FabricType = p.FabricType,
                MaterialComposition = p.MaterialComposition,
                Gsm = p.Gsm,
                WashCare = p.WashCare,
                Occasion = p.Occasion,
                CategoryName = p.Category.Name,
                BrandName = p.Brand != null ? p.Brand.Name : null,
                ImageUrls = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).ToList(),
                Variants = p.Variants.Where(v => v.IsActive).Select(v => new VariantVm
                {
                    Id = v.Id,
                    Sku = v.Sku,
                    Price = v.Price,
                    MrpPrice = v.MrpPrice,
                    ColorName = v.Color != null ? v.Color.Name : null,
                    ColorHex = v.Color != null ? v.Color.HexCode : null,
                    SizeName = v.Size != null ? v.Size.Name : null,
                    Available = v.Inventory != null ? v.Inventory.QuantityOnHand - v.Inventory.QuantityReserved : 0
                }).ToList(),
                ReviewCount = p.Reviews.Count(r => r.IsApproved),
                AverageRating = p.Reviews.Where(r => r.IsApproved).Select(r => (double?)r.Rating).Average() ?? 0
            })
            .FirstOrDefaultAsync(ct);

        if (vm is not null)
        {
            var resolved = await _promotions.ResolveProductOffersAsync(
                new[] { (vm.Id, vm.CategoryId, vm.BasePrice) }, ct);
            if (resolved.TryGetValue(vm.Id, out var hit))
            {
                vm.OfferPrice = hit.price;
                vm.OfferTitle = hit.title;
            }
        }
        return vm;
    }

    // Shared projection for product cards.
    private static readonly System.Linq.Expressions.Expression<Func<Product, ProductCardVm>> ToCard =
        p => new ProductCardVm
        {
            Id = p.Id,
            CategoryId = p.CategoryId,
            Name = p.Name,
            Slug = p.Slug,
            FabricType = p.FabricType,
            Price = p.BasePrice,
            MrpPrice = p.MrpPrice,
            PrimaryImageUrl = p.Images
                .OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder)
                .Select(i => i.Url).FirstOrDefault(),
            ReviewCount = p.Reviews.Count(r => r.IsApproved),
            AverageRating = p.Reviews.Where(r => r.IsApproved).Select(r => (double?)r.Rating).Average() ?? 0
        };
}

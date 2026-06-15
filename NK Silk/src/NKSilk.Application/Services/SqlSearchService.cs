using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;

namespace NKSilk.Application.Services;

/// <summary>
/// SQL-backed catalogue search (name / fabric / occasion / category) with category facets.
/// Swappable for a search engine via <see cref="ISearchService"/> at scale.
/// </summary>
public class SqlSearchService : ISearchService
{
    private readonly IUnitOfWork _uow;
    public SqlSearchService(IUnitOfWork uow) => _uow = uow;

    public async Task<SearchResultVm> SearchAsync(string? query, string? categorySlug, int page = 1, int pageSize = 12, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 60) pageSize = 12;

        var q = _uow.Repository<Product>().Query().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(p =>
                p.Name.Contains(term) ||
                (p.FabricType != null && p.FabricType.Contains(term)) ||
                (p.Occasion != null && p.Occasion.Contains(term)) ||
                (p.Collection != null && p.Collection.Contains(term)) ||
                p.Category.Name.Contains(term));
        }

        // Category facets over the (term-filtered) result set, computed before paging.
        var facets = await q
            .GroupBy(p => new { p.Category.Name, p.Category.Slug })
            .Select(g => new CategoryFacetVm { Name = g.Key.Name, Slug = g.Key.Slug, Count = g.Count() })
            .OrderByDescending(f => f.Count)
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(categorySlug))
            q = q.Where(p => p.Category.Slug == categorySlug);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new ProductCardVm
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Slug = p.Slug,
                FabricType = p.FabricType,
                Price = p.BasePrice,
                MrpPrice = p.MrpPrice,
                PrimaryImageUrl = p.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                ReviewCount = p.Reviews.Count(r => r.IsApproved),
                AverageRating = p.Reviews.Where(r => r.IsApproved).Select(r => (double?)r.Rating).Average() ?? 0
            })
            .ToListAsync(ct);

        return new SearchResultVm
        {
            Query = query,
            CategoryFacets = facets,
            Results = new ProductListVm
            {
                Products = items, Page = page, PageSize = pageSize, TotalCount = total,
                CategorySlug = categorySlug, SearchTerm = query
            }
        };
    }
}

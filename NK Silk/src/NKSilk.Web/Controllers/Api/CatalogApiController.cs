using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Models;

namespace NKSilk.Web.Controllers.Api;

[ApiController]
[Route("api/v1")]
[Produces("application/json")]
[OutputCache(PolicyName = "catalog")]
public class CatalogApiController : ControllerBase
{
    private readonly ICatalogService _catalog;
    private readonly IOfferService _offers;
    private readonly ISearchService _search;

    public CatalogApiController(ICatalogService catalog, IOfferService offers, ISearchService search)
    {
        _catalog = catalog;
        _offers = offers;
        _search = search;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string? q, string? category, int page = 1, int pageSize = 12, CancellationToken ct = default)
    {
        var r = await _search.SearchAsync(q, category, page, pageSize, ct);
        return Ok(ApiResponse<object>.Ok(new
        {
            query = r.Query,
            r.Results.Page, r.Results.PageSize, r.Results.TotalCount, r.Results.TotalPages,
            facets = r.CategoryFacets.Select(f => new { f.Name, f.Slug, f.Count }),
            items = r.Results.Products.Select(p => new { p.Id, p.Name, p.Slug, p.FabricType, p.Price, p.PrimaryImageUrl })
        }));
    }

    [HttpGet("products")]
    public async Task<IActionResult> Products(string? category, string? search, int page = 1, int pageSize = 12, CancellationToken ct = default)
    {
        var list = await _catalog.GetProductsAsync(category, search, page, pageSize, ct);
        return Ok(ApiResponse<object>.Ok(new
        {
            list.Page, list.PageSize, list.TotalCount, list.TotalPages,
            items = list.Products.Select(p => new
            {
                p.Id, p.Name, p.Slug, p.FabricType, p.Price, p.MrpPrice,
                offerPrice = p.HasOffer ? p.OfferPrice : null,
                p.PrimaryImageUrl, p.AverageRating, p.ReviewCount
            })
        }));
    }

    [HttpGet("products/{slug}")]
    public async Task<IActionResult> Product(string slug, CancellationToken ct)
    {
        var p = await _catalog.GetProductBySlugAsync(slug, ct);
        if (p is null) return NotFound(ApiResponse<object>.Fail("not_found", "Product not found."));
        return Ok(ApiResponse<object>.Ok(new
        {
            p.Id, p.Name, p.Slug, p.ShortDescription, p.Description,
            p.BasePrice, p.MrpPrice, offerPrice = p.HasOffer ? p.OfferPrice : null,
            p.FabricType, p.MaterialComposition, p.Gsm, p.WashCare, p.Occasion,
            p.CategoryName, p.BrandName, p.ImageUrls, p.AverageRating, p.ReviewCount,
            variants = p.Variants.Select(v => new { v.Id, v.Sku, v.Price, v.ColorName, v.SizeName, v.Available })
        }));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories(CancellationToken ct)
    {
        var cats = await _catalog.GetCategoriesAsync(ct);
        return Ok(ApiResponse<object>.Ok(cats.Select(c => new { c.Id, c.Name, c.Slug, c.ProductCount })));
    }

    [HttpGet("offers")]
    public async Task<IActionResult> Offers(CancellationToken ct)
    {
        var offers = await _offers.GetActiveOffersAsync(ct);
        return Ok(ApiResponse<object>.Ok(offers.Select(o => new
        {
            o.Title, o.Slug, o.Headline, o.Scope, o.ScopeName, o.EndsAtUtc
        })));
    }
}

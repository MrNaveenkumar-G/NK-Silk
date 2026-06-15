using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

/// <summary>Public product browsing: listing, category filter, search, detail.</summary>
public class CatalogController : Controller
{
    private readonly ICatalogService _catalog;
    private readonly IReviewService _reviews;
    private readonly IWishlistService _wishlist;

    public CatalogController(ICatalogService catalog, IReviewService reviews, IWishlistService wishlist)
    {
        _catalog = catalog;
        _reviews = reviews;
        _wishlist = wishlist;
    }

    // /Catalog?category=sarees&search=silk&page=1
    public async Task<IActionResult> Index(string? category, string? search, int page = 1, CancellationToken ct = default)
    {
        var vm = await _catalog.GetProductsAsync(category, search, page, 12, ct);
        return View(vm);
    }

    // /Catalog/Details/kanchipuram-pure-silk-saree
    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var product = await _catalog.GetProductBySlugAsync(id, ct);
        if (product is null) return NotFound();

        product.Reviews = await _reviews.GetApprovedAsync(product.Id, ct);
        if (User.Identity?.IsAuthenticated == true)
            product.InWishlist = await _wishlist.ContainsAsync(User.GetCustomerId(), product.Id, ct);

        return View(product);
    }
}

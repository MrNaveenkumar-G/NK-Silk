using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

[Authorize]
public class WishlistController : Controller
{
    private readonly IWishlistService _wishlist;
    public WishlistController(IWishlistService wishlist) => _wishlist = wishlist;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _wishlist.GetAsync(User.GetCustomerId(), ct));

    // AJAX toggle from product cards / detail page.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int productId, CancellationToken ct)
    {
        var saved = await _wishlist.ToggleAsync(User.GetCustomerId(), productId, ct);
        var count = await _wishlist.CountAsync(User.GetCustomerId(), ct);
        return Json(new { success = true, saved, count });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId, CancellationToken ct)
    {
        await _wishlist.RemoveAsync(User.GetCustomerId(), productId, ct);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Count(CancellationToken ct)
        => Json(new { count = await _wishlist.CountAsync(User.GetCustomerId(), ct) });
}

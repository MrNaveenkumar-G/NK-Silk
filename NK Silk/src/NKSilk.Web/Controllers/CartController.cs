using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

/// <summary>Cart page + AJAX endpoints (add/update/remove return JSON).</summary>
public class CartController : Controller
{
    private readonly ICartService _cart;

    public CartController(ICartService cart) => _cart = cart;

    private string Key => CartCookie.GetOrCreateKey(HttpContext);

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _cart.GetCartAsync(Key, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productVariantId, int quantity = 1, CancellationToken ct = default)
    {
        try
        {
            var cart = await _cart.AddItemAsync(Key, productVariantId, quantity, ct);
            return Json(new { success = true, itemCount = cart.ItemCount, subTotal = cart.SubTotal });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int cartItemId, int quantity, CancellationToken ct)
    {
        var cart = await _cart.UpdateQuantityAsync(Key, cartItemId, quantity, ct);
        return Json(new { success = true, itemCount = cart.ItemCount, subTotal = cart.SubTotal });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int cartItemId, CancellationToken ct)
    {
        var cart = await _cart.RemoveItemAsync(Key, cartItemId, ct);
        return Json(new { success = true, itemCount = cart.ItemCount, subTotal = cart.SubTotal });
    }

    // Used by the navbar badge.
    public async Task<IActionResult> Count(CancellationToken ct)
        => Json(new { itemCount = await _cart.GetItemCountAsync(Key, ct) });
}

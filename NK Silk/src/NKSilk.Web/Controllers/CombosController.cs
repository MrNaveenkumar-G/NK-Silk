using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

public class CombosController : Controller
{
    private readonly IComboService _combos;
    private readonly ICartService _cart;

    public CombosController(IComboService combos, ICartService cart)
    {
        _combos = combos;
        _cart = cart;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _combos.GetActiveAsync(ct));

    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var vm = await _combos.GetBySlugAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int comboId, CancellationToken ct)
    {
        var key = CartCookie.GetOrCreateKey(HttpContext);
        var variants = await _combos.GetCartVariantsAsync(comboId, ct);
        if (variants.Count == 0)
        {
            TempData["Error"] = "This combo has no buyable items right now.";
            return RedirectToAction(nameof(Index));
        }
        foreach (var (variantId, qty) in variants)
        {
            try { await _cart.AddItemAsync(key, variantId, qty, ct); }
            catch (InvalidOperationException) { /* skip inactive variant */ }
        }
        TempData["Success"] = "Combo added to your cart — the bundle price is applied automatically.";
        return RedirectToAction("Index", "Cart");
    }
}

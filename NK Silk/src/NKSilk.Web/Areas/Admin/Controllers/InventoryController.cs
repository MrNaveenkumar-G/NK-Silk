using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class InventoryController : AdminBaseController
{
    private readonly IAdminService _admin;
    public InventoryController(IAdminService admin) => _admin = admin;

    public async Task<IActionResult> Index(string? search, CancellationToken ct)
    {
        ViewData["Search"] = search;
        return View(await _admin.GetInventoryAsync(search, ct));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int variantId, int quantityOnHand, int reorderLevel, CancellationToken ct)
    {
        var result = await _admin.UpdateStockAsync(variantId, quantityOnHand, reorderLevel, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Stock updated." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}

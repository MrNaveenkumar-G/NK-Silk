using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Areas.Vendor.Controllers;

public class InventoryController : VendorBaseController
{
    private readonly IVendorService _vendor;
    public InventoryController(IVendorService vendor) => _vendor = vendor;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _vendor.GetInventoryAsync(User.GetVendorId(), ct));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int variantId, int quantityOnHand, int reorderLevel, CancellationToken ct)
    {
        var result = await _vendor.UpdateStockAsync(User.GetVendorId(), variantId, quantityOnHand, reorderLevel, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Stock updated." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}

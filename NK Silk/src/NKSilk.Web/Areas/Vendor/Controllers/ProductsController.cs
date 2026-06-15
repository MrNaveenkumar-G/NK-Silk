using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Areas.Vendor.Controllers;

public class ProductsController : VendorBaseController
{
    private readonly IVendorService _vendor;
    public ProductsController(IVendorService vendor) => _vendor = vendor;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _vendor.GetProductsAsync(User.GetVendorId(), ct));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken ct)
    {
        var result = await _vendor.ToggleProductActiveAsync(User.GetVendorId(), id, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Product updated." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}

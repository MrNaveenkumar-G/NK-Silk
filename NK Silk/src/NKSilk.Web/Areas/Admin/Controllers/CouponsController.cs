using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class CouponsController : AdminBaseController
{
    private readonly IAdminService _admin;
    public CouponsController(IAdminService admin) => _admin = admin;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _admin.GetCouponsAsync(ct));

    [HttpGet]
    public async Task<IActionResult> Edit(int? id, CancellationToken ct)
    {
        var vm = await _admin.GetCouponForEditAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminCouponVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _admin.SaveCouponAsync(vm, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }
        TempData["Success"] = "Coupon saved.";
        return RedirectToAction(nameof(Index));
    }
}

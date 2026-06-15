using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class OffersController : AdminBaseController
{
    private readonly IOfferService _offers;
    public OffersController(IOfferService offers) => _offers = offers;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _offers.GetAllAsync(ct));

    [HttpGet]
    public async Task<IActionResult> Edit(int? id, CancellationToken ct)
    {
        var vm = await _offers.GetForEditAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminOfferVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var reload = await _offers.GetForEditAsync(vm.Id == 0 ? null : vm.Id, ct);
            vm.Categories = reload?.Categories ?? new List<CategoryVm>();
            return View(vm);
        }
        var result = await _offers.SaveAsync(vm, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            var reload = await _offers.GetForEditAsync(vm.Id == 0 ? null : vm.Id, ct);
            vm.Categories = reload?.Categories ?? new List<CategoryVm>();
            return View(vm);
        }
        TempData["Success"] = "Offer saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken ct)
    {
        var result = await _offers.ToggleActiveAsync(id, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Offer updated." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _offers.DeleteAsync(id, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Offer deleted." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}

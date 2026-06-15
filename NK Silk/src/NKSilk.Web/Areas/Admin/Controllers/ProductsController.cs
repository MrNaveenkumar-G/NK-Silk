using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class ProductsController : AdminBaseController
{
    private readonly IAdminService _admin;
    public ProductsController(IAdminService admin) => _admin = admin;

    public async Task<IActionResult> Index(string? search, CancellationToken ct)
    {
        ViewData["Search"] = search;
        return View(await _admin.GetProductsAsync(search, ct));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id, CancellationToken ct)
    {
        var vm = await _admin.GetProductForEditAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminProductEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var reload = await _admin.GetProductForEditAsync(vm.Id == 0 ? null : vm.Id, ct);
            vm.Categories = reload?.Categories ?? vm.Categories;
            return View(vm);
        }

        var result = await _admin.SaveProductAsync(vm, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            var reload = await _admin.GetProductForEditAsync(vm.Id == 0 ? null : vm.Id, ct);
            vm.Categories = reload?.Categories ?? vm.Categories;
            return View(vm);
        }

        TempData["Success"] = "Product saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken ct)
    {
        var result = await _admin.ToggleProductActiveAsync(id, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Product visibility updated." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}

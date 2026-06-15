using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class CombosController : AdminBaseController
{
    private readonly IComboService _combos;
    public CombosController(IComboService combos) => _combos = combos;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _combos.GetAllAsync(ct));

    [HttpGet]
    public async Task<IActionResult> Edit(int? id, CancellationToken ct)
    {
        var vm = await _combos.GetForEditAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminComboVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await _combos.SaveAsync(vm, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }
        TempData["Success"] = "Combo saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, CancellationToken ct)
    {
        var result = await _combos.ToggleActiveAsync(id, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Combo updated." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}

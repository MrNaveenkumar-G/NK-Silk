using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class CategoriesController : AdminBaseController
{
    private readonly IAdminService _admin;
    public CategoriesController(IAdminService admin) => _admin = admin;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _admin.GetCategoriesAsync(ct));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(AdminCategoryVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please provide a valid category name.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _admin.SaveCategoryAsync(vm, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Category saved." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}

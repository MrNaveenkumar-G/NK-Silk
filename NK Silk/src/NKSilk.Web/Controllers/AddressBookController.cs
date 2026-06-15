using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

[Authorize]
public class AddressBookController : Controller
{
    private readonly IAddressService _addresses;
    public AddressBookController(IAddressService addresses) => _addresses = addresses;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _addresses.GetForCustomerAsync(User.GetCustomerId(), ct));

    [HttpGet]
    public async Task<IActionResult> Edit(int? id, CancellationToken ct)
    {
        var vm = await _addresses.GetForEditAsync(User.GetCustomerId(), id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AddressFormVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);
        await _addresses.SaveAsync(User.GetCustomerId(), vm, ct);
        TempData["Success"] = "Address saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _addresses.DeleteAsync(User.GetCustomerId(), id, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefault(int id, CancellationToken ct)
    {
        await _addresses.SetDefaultAsync(User.GetCustomerId(), id, ct);
        return RedirectToAction(nameof(Index));
    }
}

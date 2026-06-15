using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

[Authorize]
public class ReturnsController : Controller
{
    private readonly IReturnService _returns;
    public ReturnsController(IReturnService returns) => _returns = returns;

    // My Returns
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _returns.GetForCustomerAsync(User.GetCustomerId(), ct));

    // /Returns/Details/RMA2026...
    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var vm = await _returns.GetForCustomerAsync(User.GetCustomerId(), id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    // /Returns/Create/NK2026...  (id = order number)
    public async Task<IActionResult> Create(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var form = await _returns.GetRequestFormAsync(User.GetCustomerId(), id, ct);
        if (form is null)
        {
            TempData["Error"] = "This order isn't eligible for a return, or all items have already been returned.";
            return RedirectToAction("Details", "Orders", new { id });
        }
        return View(form);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReturnRequestVm form, CancellationToken ct)
    {
        var result = await _returns.CreateAsync(User.GetCustomerId(), form, ct);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Create), new { id = form.OrderNumber });
        }
        TempData["Success"] = $"Return {result.ReturnNumber} has been submitted.";
        return RedirectToAction(nameof(Details), new { id = result.ReturnNumber });
    }
}

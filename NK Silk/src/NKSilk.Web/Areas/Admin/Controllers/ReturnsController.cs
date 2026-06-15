using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Domain.Enums;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class ReturnsController : AdminBaseController
{
    private readonly IReturnService _returns;
    public ReturnsController(IReturnService returns) => _returns = returns;

    public async Task<IActionResult> Index(ReturnStatus? status, CancellationToken ct)
    {
        ViewData["Status"] = status;
        return View(await _returns.GetAllAsync(status, ct));
    }

    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var vm = await _returns.GetAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string returnNumber, CancellationToken ct)
    {
        var result = await _returns.SetApprovalAsync(returnNumber, approved: true, note: null, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Return approved." : result.Error;
        return RedirectToAction(nameof(Details), new { id = returnNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(string returnNumber, string? note, CancellationToken ct)
    {
        var result = await _returns.SetApprovalAsync(returnNumber, approved: false, note, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Return rejected." : result.Error;
        return RedirectToAction(nameof(Details), new { id = returnNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PickedUp(string returnNumber, CancellationToken ct)
    {
        var result = await _returns.MarkPickedUpAsync(returnNumber, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Marked as picked up." : result.Error;
        return RedirectToAction(nameof(Details), new { id = returnNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(string returnNumber, CancellationToken ct)
    {
        var result = await _returns.RefundAsync(returnNumber, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Refund issued and stock restored." : result.Error;
        return RedirectToAction(nameof(Details), new { id = returnNumber });
    }
}

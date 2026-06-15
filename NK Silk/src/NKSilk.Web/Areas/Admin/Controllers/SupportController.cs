using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Domain.Enums;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class SupportController : AdminBaseController
{
    private readonly ISupportService _support;
    public SupportController(ISupportService support) => _support = support;

    public async Task<IActionResult> Index(TicketStatus? status, CancellationToken ct)
    {
        ViewData["Status"] = status;
        return View(await _support.GetAllAsync(status, ct));
    }

    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var vm = await _support.GetAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(string ticketNumber, string body, CancellationToken ct)
    {
        var result = await _support.StaffReplyAsync(ticketNumber, User.Identity?.Name ?? "Support", body, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Reply sent." : result.Error;
        return RedirectToAction(nameof(Details), new { id = ticketNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(string ticketNumber, TicketStatus status, CancellationToken ct)
    {
        var result = await _support.SetStatusAsync(ticketNumber, status, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Status updated." : result.Error;
        return RedirectToAction(nameof(Details), new { id = ticketNumber });
    }
}

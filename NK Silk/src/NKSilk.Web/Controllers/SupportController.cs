using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

[Authorize]
public class SupportController : Controller
{
    private readonly ISupportService _support;
    public SupportController(ISupportService support) => _support = support;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _support.GetForCustomerAsync(User.GetCustomerId(), ct));

    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var vm = await _support.GetForCustomerAsync(User.GetCustomerId(), id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpGet]
    public IActionResult Create(string? orderNumber) => View(new SupportTicketFormVm { OrderNumber = orderNumber });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupportTicketFormVm form, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(form);
        var result = await _support.CreateAsync(User.GetCustomerId(), form, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(form);
        }
        TempData["Success"] = $"Ticket {result.TicketNumber} created. We'll get back to you soon.";
        return RedirectToAction(nameof(Details), new { id = result.TicketNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(string ticketNumber, string body, CancellationToken ct)
    {
        var result = await _support.CustomerReplyAsync(User.GetCustomerId(), ticketNumber, body, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Reply sent." : result.Error;
        return RedirectToAction(nameof(Details), new { id = ticketNumber });
    }
}

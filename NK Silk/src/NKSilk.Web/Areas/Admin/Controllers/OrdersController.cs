using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Domain.Enums;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class OrdersController : AdminBaseController
{
    private readonly IAdminService _admin;
    private readonly IPaymentService _payments;

    public OrdersController(IAdminService admin, IPaymentService payments)
    {
        _admin = admin;
        _payments = payments;
    }

    public async Task<IActionResult> Index(OrderStatus? status, CancellationToken ct)
    {
        ViewData["Status"] = status;
        return View(await _admin.GetOrdersAsync(status, ct));
    }

    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var vm = await _admin.GetOrderAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string orderNumber, OrderStatus status, CancellationToken ct)
    {
        var result = await _admin.UpdateOrderStatusAsync(orderNumber, status, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Order status updated." : result.Error;
        return RedirectToAction(nameof(Details), new { id = orderNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(string orderNumber, CancellationToken ct)
    {
        var result = await _payments.RefundAsync(orderNumber, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Order refunded." : result.Error;
        return RedirectToAction(nameof(Details), new { id = orderNumber });
    }
}

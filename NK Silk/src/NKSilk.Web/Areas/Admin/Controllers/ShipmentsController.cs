using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Domain.Enums;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class ShipmentsController : AdminBaseController
{
    private readonly ILogisticsService _logistics;
    public ShipmentsController(ILogisticsService logistics) => _logistics = logistics;

    public async Task<IActionResult> Index(ShipmentStatus? status, CancellationToken ct)
    {
        ViewData["Status"] = status;
        return View(await _logistics.GetAllAsync(status, ct));
    }

    // Manage (create / add events for) a single order's shipment.
    public async Task<IActionResult> Manage(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var vm = await _logistics.GetForOrderAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string orderNumber, string courier, string trackingNumber,
        DateTime? estimatedDelivery, CancellationToken ct)
    {
        var result = await _logistics.CreateShipmentAsync(orderNumber, courier, trackingNumber, estimatedDelivery, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Shipment created." : result.Error;
        return RedirectToAction(nameof(Manage), new { id = orderNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEvent(string orderNumber, ShipmentStatus status, string? note, CancellationToken ct)
    {
        var result = await _logistics.AddEventAsync(orderNumber, status, note, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Tracking updated." : result.Error;
        return RedirectToAction(nameof(Manage), new { id = orderNumber });
    }
}

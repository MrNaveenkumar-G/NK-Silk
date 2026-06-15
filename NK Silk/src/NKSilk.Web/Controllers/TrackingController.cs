using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

[Authorize]
public class TrackingController : Controller
{
    private readonly ILogisticsService _logistics;
    public TrackingController(ILogisticsService logistics) => _logistics = logistics;

    // /Tracking/Order/NK2026...
    public async Task<IActionResult> Order(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var vm = await _logistics.GetTrackingForCustomerAsync(User.GetCustomerId(), id, ct);
        return vm is null ? NotFound() : View(vm);
    }
}

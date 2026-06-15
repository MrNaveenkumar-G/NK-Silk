using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Areas.Vendor.Controllers;

public class DashboardController : VendorBaseController
{
    private readonly IVendorService _vendor;
    public DashboardController(IVendorService vendor) => _vendor = vendor;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = await _vendor.GetDashboardAsync(User.GetVendorId(), ct);
        return vm is null ? NotFound() : View(vm);
    }
}

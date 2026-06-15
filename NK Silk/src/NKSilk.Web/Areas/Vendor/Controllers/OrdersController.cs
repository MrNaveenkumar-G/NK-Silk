using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Areas.Vendor.Controllers;

public class OrdersController : VendorBaseController
{
    private readonly IVendorService _vendor;
    public OrdersController(IVendorService vendor) => _vendor = vendor;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _vendor.GetOrderItemsAsync(User.GetVendorId(), ct));
}

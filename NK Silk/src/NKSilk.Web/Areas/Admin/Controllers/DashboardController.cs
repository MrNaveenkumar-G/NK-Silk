using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class DashboardController : AdminBaseController
{
    private readonly IAdminService _admin;
    public DashboardController(IAdminService admin) => _admin = admin;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _admin.GetDashboardAsync(ct));
}

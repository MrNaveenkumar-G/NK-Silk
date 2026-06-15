using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class CustomersController : AdminBaseController
{
    private readonly IAdminService _admin;
    public CustomersController(IAdminService admin) => _admin = admin;

    public async Task<IActionResult> Index(string? search, CancellationToken ct)
    {
        ViewData["Search"] = search;
        return View(await _admin.GetCustomersAsync(search, ct));
    }
}

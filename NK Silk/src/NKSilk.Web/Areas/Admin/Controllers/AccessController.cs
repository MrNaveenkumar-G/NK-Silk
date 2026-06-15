using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;

namespace NKSilk.Web.Areas.Admin.Controllers;

/// <summary>RBAC role management + audit-trail viewer.</summary>
public class AccessController : AdminBaseController
{
    private readonly IAccessService _access;
    public AccessController(IAccessService access) => _access = access;

    // Roles list
    public async Task<IActionResult> Roles(CancellationToken ct)
        => View(await _access.GetRolesAsync(ct));

    // Manage a single customer's roles
    public async Task<IActionResult> Customer(int id, CancellationToken ct)
    {
        var vm = await _access.GetCustomerAccessAsync(id, ct);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRoles(int customerId, string[]? roles, CancellationToken ct)
    {
        var result = await _access.SetCustomerRolesAsync(customerId, roles ?? Array.Empty<string>(), ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Roles updated." : result.Error;
        return RedirectToAction(nameof(Customer), new { id = customerId });
    }

    // Audit trail
    public async Task<IActionResult> Audit(string? entity, CancellationToken ct)
    {
        ViewData["Entity"] = entity;
        return View(await _access.GetRecentAuditAsync(entity, 200, ct));
    }
}

using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class ReviewsController : AdminBaseController
{
    private readonly IAdminService _admin;
    public ReviewsController(IAdminService admin) => _admin = admin;

    // ?approved=false (default) shows the moderation queue.
    public async Task<IActionResult> Index(bool? approved, CancellationToken ct)
    {
        ViewData["Approved"] = approved;
        return View(await _admin.GetReviewsAsync(approved, ct));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetApproval(int id, bool approved, CancellationToken ct)
    {
        var result = await _admin.SetReviewApprovalAsync(id, approved, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? (approved ? "Review approved." : "Review unpublished.") : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _admin.DeleteReviewAsync(id, ct);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded ? "Review deleted." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly IReviewService _reviews;
    private readonly ICatalogService _catalog;

    public ReviewsController(IReviewService reviews, ICatalogService catalog)
    {
        _reviews = reviews;
        _catalog = catalog;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewFormVm form, string slug, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            TempData["ReviewError"] = "Please provide a rating (1–5) and a short comment.";
        else
        {
            var ok = await _reviews.AddAsync(User.GetCustomerId(), form, ct);
            TempData[ok ? "ReviewMsg" : "ReviewError"] = ok
                ? "Thanks! Your review has been submitted and will appear once approved."
                : "Could not submit review.";
        }
        return RedirectToAction("Details", "Catalog", new { id = slug });
    }
}

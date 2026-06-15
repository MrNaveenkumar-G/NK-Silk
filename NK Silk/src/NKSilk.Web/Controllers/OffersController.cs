using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;

namespace NKSilk.Web.Controllers;

public class OffersController : Controller
{
    private readonly IOfferService _offers;
    public OffersController(IOfferService offers) => _offers = offers;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _offers.GetActiveOffersAsync(ct));
}

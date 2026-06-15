using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Models;

namespace NKSilk.Web.Controllers;

public class HomeController : Controller
{
    private readonly ICatalogService _catalog;
    private readonly IOfferService _offers;

    public HomeController(ICatalogService catalog, IOfferService offers)
    {
        _catalog = catalog;
        _offers = offers;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = new HomeViewModel
        {
            Categories = await _catalog.GetCategoriesAsync(ct),
            Featured = await _catalog.GetFeaturedAsync(8, ct),
            Offers = await _offers.GetActiveOffersAsync(ct)
        };
        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}

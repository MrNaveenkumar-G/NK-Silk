using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    // My Orders
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _orders.GetOrdersForCustomerAsync(User.GetCustomerId(), ct));

    // /Orders/Details/NK20260611...
    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var order = await _orders.GetOrderForCustomerAsync(User.GetCustomerId(), id, ct);
        return order is null ? NotFound() : View(order);
    }
}

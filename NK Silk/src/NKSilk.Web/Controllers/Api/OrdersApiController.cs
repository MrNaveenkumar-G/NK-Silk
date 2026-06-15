using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Web.Infrastructure;
using NKSilk.Web.Models;

namespace NKSilk.Web.Controllers.Api;

[ApiController]
[Route("api/v1/orders")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces("application/json")]
public class OrdersApiController : ControllerBase
{
    private readonly IOrderService _orders;
    public OrdersApiController(IOrderService orders) => _orders = orders;

    [HttpPost]
    public async Task<IActionResult> Place([FromBody] PlaceOrderVm form, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.Fail("validation", "Missing or invalid shipping/payment details."));
        // Orders placed via the API use the customer's API cart (see CartApiController).
        var result = await _orders.PlaceOrderAsync($"api:{User.GetCustomerId()}", User.GetCustomerId(), form, null, ct);
        return result.Succeeded
            ? Ok(ApiResponse<object>.Ok(new { result.OrderNumber }))
            : BadRequest(ApiResponse<object>.Fail("order_failed", result.Error!));
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var orders = await _orders.GetOrdersForCustomerAsync(User.GetCustomerId(), ct);
        return Ok(ApiResponse<object>.Ok(orders.Select(o => new
        {
            o.OrderNumber, o.PlacedAtUtc, status = o.Status.ToString(), o.GrandTotal, o.ItemCount
        })));
    }

    [HttpGet("{orderNumber}")]
    public async Task<IActionResult> Detail(string orderNumber, CancellationToken ct)
    {
        var o = await _orders.GetOrderForCustomerAsync(User.GetCustomerId(), orderNumber, ct);
        if (o is null) return NotFound(ApiResponse<object>.Fail("not_found", "Order not found."));
        return Ok(ApiResponse<object>.Ok(new
        {
            o.OrderNumber, o.PlacedAtUtc, status = o.Status.ToString(),
            o.SubTotal, o.DiscountAmount, o.ShippingFee, o.GrandTotal,
            payment = new { method = o.PaymentMethod.ToString(), status = o.PaymentStatus.ToString() },
            lines = o.Lines.Select(l => new { l.ProductName, l.ColorName, l.SizeName, l.Quantity, l.UnitPrice, l.LineTotal })
        }));
    }
}

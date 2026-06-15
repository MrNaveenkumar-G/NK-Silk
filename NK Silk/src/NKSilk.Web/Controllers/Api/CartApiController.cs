using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;
using NKSilk.Web.Models;

namespace NKSilk.Web.Controllers.Api;

[ApiController]
[Route("api/v1/cart")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces("application/json")]
public class CartApiController : ControllerBase
{
    private readonly ICartService _cart;
    public CartApiController(ICartService cart) => _cart = cart;

    // A stable per-customer cart key for API clients (distinct from the web cookie cart).
    private string Key => $"api:{User.GetCustomerId()}";

    public record AddItemRequest(int VariantId, int Qty);
    public record UpdateItemRequest(int Qty);

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(ApiResponse<object>.Ok(await Snapshot(ct)));

    [HttpPost("items")]
    public async Task<IActionResult> Add([FromBody] AddItemRequest req, CancellationToken ct)
    {
        try { await _cart.AddItemAsync(Key, req.VariantId, req.Qty < 1 ? 1 : req.Qty, ct); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail("add_failed", ex.Message)); }
        return Ok(ApiResponse<object>.Ok(await Snapshot(ct)));
    }

    [HttpPut("items/{cartItemId:int}")]
    public async Task<IActionResult> Update(int cartItemId, [FromBody] UpdateItemRequest req, CancellationToken ct)
    {
        await _cart.UpdateQuantityAsync(Key, cartItemId, req.Qty, ct);
        return Ok(ApiResponse<object>.Ok(await Snapshot(ct)));
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> Remove(int cartItemId, CancellationToken ct)
    {
        await _cart.RemoveItemAsync(Key, cartItemId, ct);
        return Ok(ApiResponse<object>.Ok(await Snapshot(ct)));
    }

    private async Task<object> Snapshot(CancellationToken ct)
    {
        var c = await _cart.GetCartAsync(Key, ct);
        return new
        {
            itemCount = c.ItemCount,
            subTotal = c.SubTotal,
            offerSavings = c.OfferSavings,
            comboSavings = c.ComboSavings,
            payable = c.Payable,
            lines = c.Lines.Select(l => new
            {
                l.CartItemId, l.ProductVariantId, l.ProductName, l.ColorName, l.SizeName,
                l.UnitPrice, l.OriginalUnitPrice, l.Quantity, l.LineTotal
            })
        };
    }
}

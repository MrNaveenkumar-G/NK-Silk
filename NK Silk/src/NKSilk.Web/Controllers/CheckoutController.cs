using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private const string CouponSessionKey = "AppliedCoupon";

    private readonly IOrderService _orders;
    private readonly ICouponService _coupons;
    private readonly IAddressService _addresses;

    public CheckoutController(IOrderService orders, ICouponService coupons, IAddressService addresses)
    {
        _orders = orders;
        _coupons = coupons;
        _addresses = addresses;
    }

    private string CartKey => CartCookie.GetOrCreateKey(HttpContext);

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = await _orders.GetCheckoutAsync(CartKey, ct);
        if (vm.Cart.IsEmpty) return RedirectToAction("Index", "Cart");

        await ApplySessionCouponAsync(vm, ct);

        var addresses = await _addresses.GetForCustomerAsync(User.GetCustomerId(), ct);
        ViewBag.SavedAddresses = addresses;

        // Prefill from the default saved address, else just the name.
        var def = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses.FirstOrDefault();
        if (def is not null)
        {
            vm.Form.ContactName = def.ContactName;
            vm.Form.PhoneNumber = def.PhoneNumber;
            vm.Form.Line1 = def.Line1;
            vm.Form.Line2 = def.Line2;
            vm.Form.City = def.City;
            vm.Form.State = def.State;
            vm.Form.PostalCode = def.PostalCode;
        }
        else
        {
            vm.Form.ContactName = User.Identity?.Name ?? string.Empty;
        }
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyCoupon(string code, CancellationToken ct)
    {
        var vm = await _orders.GetCheckoutAsync(CartKey, ct);
        var result = await _coupons.ValidateAsync(code ?? "", vm.SubTotal, ct);
        if (result.IsValid)
        {
            HttpContext.Session.SetString(CouponSessionKey, result.Code);
            TempData["CouponMsg"] = $"Coupon {result.Code} applied — you saved ₹{result.DiscountAmount:N0}.";
        }
        else
        {
            HttpContext.Session.Remove(CouponSessionKey);
            TempData["CouponError"] = result.Error;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RemoveCoupon()
    {
        HttpContext.Session.Remove(CouponSessionKey);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Place(PlaceOrderVm form, CancellationToken ct)
    {
        var vm = await _orders.GetCheckoutAsync(CartKey, ct);
        if (vm.Cart.IsEmpty) return RedirectToAction("Index", "Cart");

        if (!ModelState.IsValid)
        {
            await ApplySessionCouponAsync(vm, ct);
            vm.Form = form;
            return View(nameof(Index), vm);
        }

        var coupon = HttpContext.Session.GetString(CouponSessionKey);
        var result = await _orders.PlaceOrderAsync(CartKey, User.GetCustomerId(), form, coupon, ct);
        if (!result.Succeeded)
        {
            await ApplySessionCouponAsync(vm, ct);
            ModelState.AddModelError(string.Empty, result.Error!);
            vm.Form = form;
            return View(nameof(Index), vm);
        }

        HttpContext.Session.Remove(CouponSessionKey);

        if (form.PaymentMethod == Domain.Enums.PaymentMethod.CashOnDelivery)
            return RedirectToAction(nameof(Confirmation), new { orderNumber = result.OrderNumber });

        return RedirectToAction("Pay", "Payment", new { orderNumber = result.OrderNumber });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(string orderNumber, CancellationToken ct)
    {
        var order = await _orders.GetOrderForCustomerAsync(User.GetCustomerId(), orderNumber, ct);
        return order is null ? RedirectToAction("Index", "Home") : View(order);
    }

    // Validates the coupon currently in session against the live subtotal and fills the VM.
    private async Task ApplySessionCouponAsync(CheckoutVm vm, CancellationToken ct)
    {
        var code = HttpContext.Session.GetString(CouponSessionKey);
        if (string.IsNullOrEmpty(code)) return;

        var result = await _coupons.ValidateAsync(code, vm.SubTotal, ct);
        if (result.IsValid)
        {
            vm.AppliedCouponCode = result.Code;
            vm.DiscountAmount = result.DiscountAmount;
        }
        else
        {
            HttpContext.Session.Remove(CouponSessionKey); // no longer valid (e.g. cart changed)
        }
    }
}

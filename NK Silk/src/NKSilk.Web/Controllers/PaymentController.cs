using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly IPaymentService _payments;

    public PaymentController(IPaymentService payments) => _payments = payments;

    // Shows the Razorpay checkout widget (or the dev simulator) for an unpaid order.
    [HttpGet]
    public async Task<IActionResult> Pay(string orderNumber, CancellationToken ct)
    {
        var session = await _payments.CreateSessionAsync(User.GetCustomerId(), orderNumber, ct);
        if (session is null) return RedirectToAction("Index", "Home");
        if (session.AlreadyPaid)
            return RedirectToAction("Confirmation", "Checkout", new { orderNumber });
        return View(session);
    }

    // Razorpay redirects/posts the payment result here (also used by the dev simulator).
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Callback(string orderNumber, string razorpay_payment_id,
        string razorpay_order_id, string razorpay_signature, CancellationToken ct)
    {
        var result = await _payments.CaptureAsync(User.GetCustomerId(), orderNumber,
            razorpay_payment_id, razorpay_order_id, razorpay_signature, ct);

        if (!result.Succeeded)
        {
            TempData["PaymentError"] = result.Error;
            return RedirectToAction(nameof(Pay), new { orderNumber });
        }
        return RedirectToAction("Confirmation", "Checkout", new { orderNumber });
    }
}

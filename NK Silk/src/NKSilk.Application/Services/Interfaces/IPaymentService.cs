using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface IPaymentService
{
    /// <summary>Creates (or returns) a gateway order for an unpaid order and builds the checkout session.</summary>
    Task<PaymentSessionVm?> CreateSessionAsync(int customerId, string orderNumber, CancellationToken ct = default);

    /// <summary>Verifies the gateway callback signature and, on success, marks the order paid &amp; confirmed.</summary>
    Task<PaymentResult> CaptureAsync(int customerId, string orderNumber, string gatewayPaymentId,
        string gatewayOrderId, string signature, CancellationToken ct = default);

    /// <summary>Refunds a paid order (admin/customer-service action).</summary>
    Task<PaymentResult> RefundAsync(string orderNumber, CancellationToken ct = default);
}

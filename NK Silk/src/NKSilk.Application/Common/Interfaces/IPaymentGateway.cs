namespace NKSilk.Application.Common.Interfaces;

/// <summary>A created gateway order awaiting customer payment.</summary>
public record GatewayOrder(string Id, long AmountPaise, string Currency);

/// <summary>
/// Abstraction over the payment provider (Razorpay). Implemented in Infrastructure;
/// falls back to a local simulation when credentials are not configured.
/// </summary>
public interface IPaymentGateway
{
    bool IsLive { get; }
    string PublicKeyId { get; }

    Task<GatewayOrder> CreateOrderAsync(long amountPaise, string currency, string receipt, CancellationToken ct = default);

    /// <summary>Validates the Razorpay callback signature (HMAC-SHA256 of "orderId|paymentId").</summary>
    bool VerifySignature(string gatewayOrderId, string gatewayPaymentId, string signature);

    Task<bool> RefundAsync(string gatewayPaymentId, long amountPaise, CancellationToken ct = default);
}

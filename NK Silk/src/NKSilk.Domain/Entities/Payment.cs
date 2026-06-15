using NKSilk.Domain.Common;
using NKSilk.Domain.Enums;

namespace NKSilk.Domain.Entities;

/// <summary>Payment record for an order. One-to-one with Order.</summary>
public class Payment : BaseEntity
{
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";

    // Gateway correlation (Razorpay/PhonePe order & payment ids, signature)
    public string? GatewayOrderId { get; set; }
    public string? GatewayPaymentId { get; set; }
    public string? GatewaySignature { get; set; }
    public DateTime? PaidAtUtc { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
}

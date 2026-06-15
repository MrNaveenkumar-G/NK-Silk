namespace NKSilk.Application.ViewModels;

/// <summary>Everything the Razorpay checkout widget (or the dev simulator) needs.</summary>
public class PaymentSessionVm
{
    public string OrderNumber { get; set; } = string.Empty;
    public string GatewayOrderId { get; set; } = string.Empty;
    public string KeyId { get; set; } = string.Empty;
    public long AmountPaise { get; set; }
    public string Currency { get; set; } = "INR";
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>False ⇒ no real credentials ⇒ the page shows the dev simulator instead of Razorpay.</summary>
    public bool IsLive { get; set; }
    public bool AlreadyPaid { get; set; }

    public decimal Amount => AmountPaise / 100m;
}

public class PaymentResult
{
    public bool Succeeded { get; private set; }
    public string? Error { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;

    public static PaymentResult Success(string orderNumber) => new() { Succeeded = true, OrderNumber = orderNumber };
    public static PaymentResult Fail(string error) => new() { Succeeded = false, Error = error };
}

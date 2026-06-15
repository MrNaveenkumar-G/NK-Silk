namespace NKSilk.Application.Common.Settings;

/// <summary>Razorpay gateway credentials (bound from configuration section "Razorpay").</summary>
public class RazorpayOptions
{
    public const string SectionName = "Razorpay";

    public string KeyId { get; set; } = string.Empty;
    public string KeySecret { get; set; } = string.Empty;

    /// <summary>Live only when both keys are present; otherwise the gateway runs in simulation mode.</summary>
    public bool IsLive => !string.IsNullOrWhiteSpace(KeyId) && !string.IsNullOrWhiteSpace(KeySecret);
}

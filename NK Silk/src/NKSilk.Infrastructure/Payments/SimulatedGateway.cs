using NKSilk.Application.Common.Interfaces;

namespace NKSilk.Infrastructure.Payments;

/// <summary>
/// A self-contained simulator for gateways that aren't wired to a live provider yet
/// (PhonePe, UPI, cards, net-banking). It mints fake gateway orders and trusts the dev
/// callback — the same contract a real gateway implements, so swapping one in is a drop-in.
/// </summary>
public class SimulatedGateway : IPaymentGateway
{
    private readonly string _name;
    public SimulatedGateway(string name) => _name = name;

    public bool IsLive => false;
    public string PublicKeyId => $"{_name.ToLowerInvariant()}_sim";

    public Task<GatewayOrder> CreateOrderAsync(long amountPaise, string currency, string receipt, CancellationToken ct = default)
        => Task.FromResult(new GatewayOrder($"{_name.ToLowerInvariant()}_sim_{Guid.NewGuid():N}", amountPaise, currency));

    public bool VerifySignature(string gatewayOrderId, string gatewayPaymentId, string signature) => true;

    public Task<bool> RefundAsync(string gatewayPaymentId, long amountPaise, CancellationToken ct = default)
        => Task.FromResult(true);
}

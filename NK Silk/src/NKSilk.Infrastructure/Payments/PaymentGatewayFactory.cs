using NKSilk.Application.Common.Interfaces;
using NKSilk.Domain.Enums;

namespace NKSilk.Infrastructure.Payments;

/// <summary>
/// Maps a chosen <see cref="PaymentMethod"/> to a gateway implementation. Razorpay uses the
/// real/simulated Razorpay client; the other online methods use named simulators until a
/// live provider is configured.
/// </summary>
public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly RazorpayGateway _razorpay;
    private readonly Dictionary<PaymentMethod, IPaymentGateway> _simulators;

    public PaymentGatewayFactory(RazorpayGateway razorpay)
    {
        _razorpay = razorpay;
        _simulators = new Dictionary<PaymentMethod, IPaymentGateway>
        {
            [PaymentMethod.PhonePe] = new SimulatedGateway("PhonePe"),
            [PaymentMethod.Upi] = new SimulatedGateway("UPI"),
            [PaymentMethod.CreditCard] = new SimulatedGateway("Card"),
            [PaymentMethod.DebitCard] = new SimulatedGateway("Card"),
            [PaymentMethod.NetBanking] = new SimulatedGateway("NetBanking")
        };
    }

    public IPaymentGateway Resolve(PaymentMethod method)
        => _simulators.TryGetValue(method, out var gateway) ? gateway : _razorpay;
}

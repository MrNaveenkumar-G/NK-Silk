using NKSilk.Domain.Enums;

namespace NKSilk.Application.Common.Interfaces;

/// <summary>Resolves the right payment gateway for a chosen payment method.</summary>
public interface IPaymentGatewayFactory
{
    IPaymentGateway Resolve(PaymentMethod method);
}

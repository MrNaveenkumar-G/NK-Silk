using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services;

/// <summary>Orchestrates the order ↔ gateway ↔ payment-row lifecycle.</summary>
public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly IPaymentGatewayFactory _gateways;
    private readonly INotificationService _notifications;

    public PaymentService(IUnitOfWork uow, IPaymentGatewayFactory gateways, INotificationService notifications)
    {
        _uow = uow;
        _gateways = gateways;
        _notifications = notifications;
    }

    public async Task<PaymentSessionVm?> CreateSessionAsync(int customerId, string orderNumber, CancellationToken ct = default)
    {
        var order = await LoadOrderAsync(customerId, orderNumber, ct);
        if (order?.Payment is null) return null;

        var gateway = _gateways.Resolve(order.Payment.Method);
        var customer = await _uow.Repository<Customer>().GetByIdAsync(customerId, ct);
        var amountPaise = (long)Math.Round(order.GrandTotal * 100m);

        var vm = new PaymentSessionVm
        {
            OrderNumber = order.OrderNumber,
            KeyId = gateway.PublicKeyId,
            AmountPaise = amountPaise,
            Currency = order.Payment.Currency,
            CustomerName = customer?.FullName ?? "",
            CustomerEmail = customer?.Email ?? "",
            IsLive = gateway.IsLive,
            AlreadyPaid = order.Payment.Status == PaymentStatus.Paid
        };

        if (vm.AlreadyPaid) return vm;

        // Create the gateway order once and remember its id for signature verification.
        if (string.IsNullOrEmpty(order.Payment.GatewayOrderId))
        {
            var go = await gateway.CreateOrderAsync(amountPaise, order.Payment.Currency, order.OrderNumber, ct);
            order.Payment.GatewayOrderId = go.Id;
            order.Payment.Status = PaymentStatus.Authorized; // order created at gateway, awaiting capture
            _uow.Repository<Payment>().Update(order.Payment);
            await _uow.SaveChangesAsync(ct);
        }

        vm.GatewayOrderId = order.Payment.GatewayOrderId!;
        return vm;
    }

    public async Task<PaymentResult> CaptureAsync(int customerId, string orderNumber, string gatewayPaymentId,
        string gatewayOrderId, string signature, CancellationToken ct = default)
    {
        var order = await LoadOrderAsync(customerId, orderNumber, ct);
        if (order?.Payment is null) return PaymentResult.Fail("Order not found.");
        if (order.Payment.Status == PaymentStatus.Paid) return PaymentResult.Success(order.OrderNumber);

        var gateway = _gateways.Resolve(order.Payment.Method);
        // In live mode the signature must validate; in simulation we trust the dev callback.
        if (gateway.IsLive && !gateway.VerifySignature(gatewayOrderId, gatewayPaymentId, signature))
            return PaymentResult.Fail("Payment signature verification failed.");

        order.Payment.GatewayPaymentId = gatewayPaymentId;
        order.Payment.GatewaySignature = signature;
        order.Payment.Status = PaymentStatus.Paid;
        order.Payment.PaidAtUtc = DateTime.UtcNow;
        order.Status = OrderStatus.Confirmed;

        _uow.Repository<Payment>().Update(order.Payment);
        _uow.Repository<Order>().Update(order);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(order.CustomerId, NotificationType.PaymentReceived,
            "Payment received",
            $"We've received your payment of ₹{order.GrandTotal:N0} for order {order.OrderNumber}. It's now confirmed.",
            $"/Orders/Details/{order.OrderNumber}", ct);

        return PaymentResult.Success(order.OrderNumber);
    }

    public async Task<PaymentResult> RefundAsync(string orderNumber, CancellationToken ct = default)
    {
        var order = await _uow.Repository<Order>().Query(asNoTracking: false)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (order?.Payment is null) return PaymentResult.Fail("Order not found.");
        if (order.Payment.Status != PaymentStatus.Paid)
            return PaymentResult.Fail("Only paid orders can be refunded.");

        var gateway = _gateways.Resolve(order.Payment.Method);
        var ok = await gateway.RefundAsync(order.Payment.GatewayPaymentId ?? "", (long)Math.Round(order.Payment.Amount * 100m), ct);
        if (!ok) return PaymentResult.Fail("Gateway refund failed.");

        order.Payment.Status = PaymentStatus.Refunded;
        order.Status = OrderStatus.Returned;
        _uow.Repository<Payment>().Update(order.Payment);
        _uow.Repository<Order>().Update(order);
        await _uow.SaveChangesAsync(ct);

        return PaymentResult.Success(order.OrderNumber);
    }

    private Task<Order?> LoadOrderAsync(int customerId, string orderNumber, CancellationToken ct) =>
        _uow.Repository<Order>().Query(asNoTracking: false)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.OrderNumber == orderNumber, ct);
}

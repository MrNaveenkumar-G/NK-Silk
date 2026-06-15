using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services;

/// <summary>
/// Customer returns lifecycle: request against a delivered order, admin approval/rejection,
/// pickup, then a settling refund that restocks inventory and reverses payment.
/// </summary>
public class ReturnService : IReturnService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notifications;
    private readonly IPaymentGatewayFactory _gateways;

    public ReturnService(IUnitOfWork uow, INotificationService notifications, IPaymentGatewayFactory gateways)
    {
        _uow = uow;
        _notifications = notifications;
        _gateways = gateways;
    }

    public async Task<ReturnRequestVm?> GetRequestFormAsync(int customerId, string orderNumber, CancellationToken ct = default)
    {
        var order = await _uow.Repository<Order>().Query()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.OrderNumber == orderNumber, ct);

        if (order is null || order.Status != OrderStatus.Delivered) return null;

        var alreadyReturned = await ReturnedQuantitiesAsync(order.Id, ct);

        var lines = order.Items
            .Select(i => new ReturnableLineVm
            {
                OrderItemId = i.Id,
                ProductName = i.ProductName,
                ColorName = i.ColorName,
                SizeName = i.SizeName,
                QuantityOrdered = i.Quantity - (alreadyReturned.TryGetValue(i.Id, out var r) ? r : 0),
                UnitPrice = i.UnitPrice
            })
            .Where(l => l.QuantityOrdered > 0)
            .ToList();

        if (lines.Count == 0) return null; // every line already returned

        return new ReturnRequestVm { OrderNumber = orderNumber, Lines = lines };
    }

    public async Task<ReturnResult> CreateAsync(int customerId, ReturnRequestVm form, CancellationToken ct = default)
    {
        var order = await _uow.Repository<Order>().Query()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.OrderNumber == form.OrderNumber, ct);

        if (order is null) return ReturnResult.Fail("Order not found.");
        if (order.Status != OrderStatus.Delivered)
            return ReturnResult.Fail("Only delivered orders can be returned.");

        var alreadyReturned = await ReturnedQuantitiesAsync(order.Id, ct);
        var now = DateTime.UtcNow;

        var ret = new Return
        {
            ReturnNumber = $"RMA{now:yyyyMMddHHmmssfff}",
            Status = ReturnStatus.Requested,
            Reason = form.Reason,
            Comments = string.IsNullOrWhiteSpace(form.Comments) ? null : form.Comments.Trim(),
            OrderId = order.Id,
            CustomerId = customerId,
            CreatedAtUtc = now
        };

        decimal refund = 0m;
        foreach (var item in order.Items)
        {
            if (!form.Quantities.TryGetValue(item.Id, out var qty) || qty <= 0) continue;

            var max = item.Quantity - (alreadyReturned.TryGetValue(item.Id, out var done) ? done : 0);
            if (qty > max)
                return ReturnResult.Fail($"You can return at most {max} of '{item.ProductName}'.");

            var lineTotal = item.UnitPrice * qty;
            refund += lineTotal;
            ret.Items.Add(new ReturnItem
            {
                OrderItemId = item.Id,
                ProductVariantId = item.ProductVariantId,
                ProductName = item.ProductName,
                VariantSku = item.VariantSku,
                ColorName = item.ColorName,
                SizeName = item.SizeName,
                Quantity = qty,
                UnitPrice = item.UnitPrice,
                LineTotal = lineTotal,
                CreatedAtUtc = now
            });
        }

        if (ret.Items.Count == 0)
            return ReturnResult.Fail("Select at least one item and quantity to return.");

        ret.RefundAmount = refund;

        await _uow.Repository<Return>().AddAsync(ret, ct);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(customerId, NotificationType.ReturnRequested,
            "Return request received",
            $"We've logged return {ret.ReturnNumber} for order {order.OrderNumber}. We'll review it shortly.",
            $"/Returns/Details/{ret.ReturnNumber}", ct);

        return ReturnResult.Success(ret.ReturnNumber);
    }

    public async Task<IReadOnlyList<ReturnListItemVm>> GetForCustomerAsync(int customerId, CancellationToken ct = default)
        => await ListQuery(_uow.Repository<Return>().Query().Where(r => r.CustomerId == customerId)).ToListAsync(ct);

    public async Task<ReturnDetailVm?> GetForCustomerAsync(int customerId, string returnNumber, CancellationToken ct = default)
        => await DetailQuery(r => r.CustomerId == customerId && r.ReturnNumber == returnNumber).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<ReturnListItemVm>> GetAllAsync(ReturnStatus? status, CancellationToken ct = default)
    {
        var q = _uow.Repository<Return>().Query();
        if (status is not null) q = q.Where(r => r.Status == status);
        return await ListQuery(q).ToListAsync(ct);
    }

    public async Task<ReturnDetailVm?> GetAsync(string returnNumber, CancellationToken ct = default)
        => await DetailQuery(r => r.ReturnNumber == returnNumber).FirstOrDefaultAsync(ct);

    public Task<int> CountPendingAsync(CancellationToken ct = default)
        => _uow.Repository<Return>().CountAsync(r => r.Status == ReturnStatus.Requested, ct);

    public async Task<ReturnResult> SetApprovalAsync(string returnNumber, bool approved, string? note, CancellationToken ct = default)
    {
        var ret = await LoadTrackedAsync(returnNumber, ct);
        if (ret is null) return ReturnResult.Fail("Return not found.");
        if (ret.Status != ReturnStatus.Requested)
            return ReturnResult.Fail("Only a requested return can be approved or rejected.");

        ret.Status = approved ? ReturnStatus.Approved : ReturnStatus.Rejected;
        ret.ResolutionNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        ret.ResolvedAtUtc = DateTime.UtcNow;
        _uow.Repository<Return>().Update(ret);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(ret.CustomerId, NotificationType.ReturnUpdate,
            approved ? "Return approved" : "Return declined",
            approved
                ? $"Return {ret.ReturnNumber} is approved. We'll arrange pickup of your item(s)."
                : $"Return {ret.ReturnNumber} couldn't be approved." + (ret.ResolutionNote is null ? "" : $" {ret.ResolutionNote}"),
            $"/Returns/Details/{ret.ReturnNumber}", ct);

        return ReturnResult.Success(ret.ReturnNumber);
    }

    public async Task<ReturnResult> MarkPickedUpAsync(string returnNumber, CancellationToken ct = default)
    {
        var ret = await LoadTrackedAsync(returnNumber, ct);
        if (ret is null) return ReturnResult.Fail("Return not found.");
        if (ret.Status != ReturnStatus.Approved)
            return ReturnResult.Fail("Only an approved return can be marked picked up.");

        ret.Status = ReturnStatus.PickedUp;
        _uow.Repository<Return>().Update(ret);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(ret.CustomerId, NotificationType.ReturnUpdate,
            "Return picked up",
            $"We've collected the item(s) for return {ret.ReturnNumber}. Your refund will follow shortly.",
            $"/Returns/Details/{ret.ReturnNumber}", ct);

        return ReturnResult.Success(ret.ReturnNumber);
    }

    public async Task<ReturnResult> RefundAsync(string returnNumber, CancellationToken ct = default)
    {
        var ret = await _uow.Repository<Return>().Query(asNoTracking: false)
            .Include(r => r.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Inventory)
            .Include(r => r.Order).ThenInclude(o => o.Payment)
            .FirstOrDefaultAsync(r => r.ReturnNumber == returnNumber, ct);

        if (ret is null) return ReturnResult.Fail("Return not found.");
        if (ret.Status is not (ReturnStatus.Approved or ReturnStatus.PickedUp))
            return ReturnResult.Fail("Approve (and collect) the return before issuing a refund.");

        var now = DateTime.UtcNow;

        // Restock the returned units.
        foreach (var item in ret.Items)
        {
            if (item.ProductVariant?.Inventory is { } inv)
            {
                inv.QuantityOnHand += item.Quantity;
                inv.UpdatedAtUtc = now;
            }
        }

        // Reverse the payment when it was actually captured online.
        var payment = ret.Order.Payment;
        if (payment is not null && payment.Status == PaymentStatus.Paid)
        {
            var gateway = _gateways.Resolve(payment.Method);
            if (gateway.IsLive && !string.IsNullOrEmpty(payment.GatewayPaymentId))
            {
                var ok = await gateway.RefundAsync(payment.GatewayPaymentId, (long)Math.Round(ret.RefundAmount * 100m), ct);
                if (!ok) return ReturnResult.Fail("Gateway refund failed.");
            }
            payment.Status = ret.RefundAmount < payment.Amount
                ? PaymentStatus.PartiallyRefunded
                : PaymentStatus.Refunded;
            _uow.Repository<Payment>().Update(payment);
        }

        ret.Status = ReturnStatus.Refunded;
        ret.ResolvedAtUtc = now;
        ret.Order.Status = OrderStatus.Returned;
        _uow.Repository<Return>().Update(ret);
        _uow.Repository<Order>().Update(ret.Order);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(ret.CustomerId, NotificationType.ReturnUpdate,
            "Refund issued",
            $"₹{ret.RefundAmount:N0} has been refunded for return {ret.ReturnNumber}.",
            $"/Returns/Details/{ret.ReturnNumber}", ct);

        return ReturnResult.Success(ret.ReturnNumber);
    }

    // ---------------- helpers ----------------

    /// <summary>Returned quantity per OrderItemId across all non-rejected returns for an order.</summary>
    private async Task<Dictionary<int, int>> ReturnedQuantitiesAsync(int orderId, CancellationToken ct)
    {
        var rows = await _uow.Repository<ReturnItem>().Query()
            .Where(ri => ri.Return.OrderId == orderId && ri.Return.Status != ReturnStatus.Rejected)
            .GroupBy(ri => ri.OrderItemId)
            .Select(g => new { OrderItemId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);
        return rows.ToDictionary(x => x.OrderItemId, x => x.Qty);
    }

    private Task<Return?> LoadTrackedAsync(string returnNumber, CancellationToken ct)
        => _uow.Repository<Return>().Query(asNoTracking: false)
            .FirstOrDefaultAsync(r => r.ReturnNumber == returnNumber, ct);

    private static IQueryable<ReturnListItemVm> ListQuery(IQueryable<Return> q)
        => q.OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new ReturnListItemVm
            {
                Id = r.Id,
                ReturnNumber = r.ReturnNumber,
                OrderNumber = r.Order.OrderNumber,
                Status = r.Status,
                Reason = r.Reason,
                RefundAmount = r.RefundAmount,
                RequestedAtUtc = r.CreatedAtUtc,
                CustomerName = r.Customer.FullName
            });

    private IQueryable<ReturnDetailVm> DetailQuery(System.Linq.Expressions.Expression<Func<Return, bool>> predicate)
        => _uow.Repository<Return>().Query()
            .Where(predicate)
            .Select(r => new ReturnDetailVm
            {
                Id = r.Id,
                ReturnNumber = r.ReturnNumber,
                OrderNumber = r.Order.OrderNumber,
                Status = r.Status,
                Reason = r.Reason,
                Comments = r.Comments,
                ResolutionNote = r.ResolutionNote,
                RefundAmount = r.RefundAmount,
                RequestedAtUtc = r.CreatedAtUtc,
                ResolvedAtUtc = r.ResolvedAtUtc,
                CustomerName = r.Customer.FullName,
                CustomerEmail = r.Customer.Email,
                Lines = r.Items.Select(i => new ReturnLineVm
                {
                    ProductName = i.ProductName,
                    ColorName = i.ColorName,
                    SizeName = i.SizeName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                }).ToList()
            });
}

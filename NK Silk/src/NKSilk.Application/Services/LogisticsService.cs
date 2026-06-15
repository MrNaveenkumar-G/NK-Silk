using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services;

/// <summary>
/// Shipment lifecycle for orders: create a shipment, append tracking events, and keep the
/// owning order's status in step. Customers get a notification on each tracking update.
/// </summary>
public class LogisticsService : ILogisticsService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notifications;

    public LogisticsService(IUnitOfWork uow, INotificationService notifications)
    {
        _uow = uow;
        _notifications = notifications;
    }

    public async Task<IReadOnlyList<AdminShipmentListItemVm>> GetAllAsync(ShipmentStatus? status, CancellationToken ct = default)
    {
        var q = _uow.Repository<Shipment>().Query();
        if (status is not null) q = q.Where(s => s.Status == status);
        return await q.OrderByDescending(s => s.CreatedAtUtc)
            .Select(s => new AdminShipmentListItemVm
            {
                Id = s.Id,
                OrderNumber = s.Order.OrderNumber,
                CustomerName = s.Order.Customer.FullName,
                Courier = s.Courier,
                TrackingNumber = s.TrackingNumber,
                Status = s.Status,
                ShippedAtUtc = s.ShippedAtUtc
            }).ToListAsync(ct);
    }

    public async Task<ShipmentDetailVm?> GetForOrderAsync(string orderNumber, CancellationToken ct = default)
    {
        var order = await _uow.Repository<Order>().Query()
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (order is null) return null;
        return await BuildDetailAsync(orderNumber, ct);
    }

    public async Task<ShipmentDetailVm?> GetTrackingForCustomerAsync(int customerId, string orderNumber, CancellationToken ct = default)
    {
        var owns = await _uow.Repository<Order>().Query()
            .AnyAsync(o => o.OrderNumber == orderNumber && o.CustomerId == customerId, ct);
        if (!owns) return null;
        return await BuildDetailAsync(orderNumber, ct);
    }

    public async Task<AdminResult> CreateShipmentAsync(string orderNumber, string courier, string trackingNumber,
        DateTime? estimatedDelivery, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(courier) || string.IsNullOrWhiteSpace(trackingNumber))
            return AdminResult.Fail("Courier and tracking number are required.");

        var order = await _uow.Repository<Order>().Query(asNoTracking: false)
            .Include(o => o.Shipment)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (order is null) return AdminResult.Fail("Order not found.");
        if (order.Shipment is not null) return AdminResult.Fail("This order already has a shipment.");
        if (order.Status is OrderStatus.Pending or OrderStatus.Cancelled)
            return AdminResult.Fail("Confirm or pay the order before shipping it.");

        var now = DateTime.UtcNow;
        var shipment = new Shipment
        {
            OrderId = order.Id,
            Courier = courier.Trim(),
            TrackingNumber = trackingNumber.Trim(),
            Status = ShipmentStatus.LabelCreated,
            EstimatedDeliveryUtc = estimatedDelivery is { } eta ? DateTime.SpecifyKind(eta, DateTimeKind.Utc) : null,
            CreatedAtUtc = now
        };
        shipment.Events.Add(new ShipmentEvent
        {
            Status = ShipmentStatus.LabelCreated,
            Note = $"Shipping label created with {courier.Trim()}",
            OccurredAtUtc = now,
            CreatedAtUtc = now
        });

        await _uow.Repository<Shipment>().AddAsync(shipment, ct);
        SyncOrderStatus(order, ShipmentStatus.LabelCreated, now);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(order.CustomerId, NotificationType.Shipment,
            "Shipment created",
            $"Order {order.OrderNumber} is packed. Track it with {courier.Trim()} ({trackingNumber.Trim()}).",
            $"/Tracking/Order/{order.OrderNumber}", ct);

        return AdminResult.Ok(shipment.Id);
    }

    public async Task<AdminResult> AddEventAsync(string orderNumber, ShipmentStatus status, string? note, CancellationToken ct = default)
    {
        var order = await _uow.Repository<Order>().Query(asNoTracking: false)
            .Include(o => o.Shipment)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (order?.Shipment is null) return AdminResult.Fail("Create a shipment before adding tracking events.");

        var now = DateTime.UtcNow;
        var shipment = order.Shipment;
        shipment.Status = status;
        if (status is ShipmentStatus.PickedUp or ShipmentStatus.InTransit && shipment.ShippedAtUtc is null)
            shipment.ShippedAtUtc = now;
        if (status == ShipmentStatus.Delivered)
            shipment.DeliveredAtUtc = now;

        shipment.Events.Add(new ShipmentEvent
        {
            ShipmentId = shipment.Id,
            Status = status,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            OccurredAtUtc = now,
            CreatedAtUtc = now
        });

        SyncOrderStatus(order, status, now);
        _uow.Repository<Shipment>().Update(shipment);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(order.CustomerId, NotificationType.Shipment,
            $"Shipment {status}",
            $"Order {order.OrderNumber}: {Describe(status)}" + (string.IsNullOrWhiteSpace(note) ? "" : $" — {note.Trim()}"),
            $"/Tracking/Order/{order.OrderNumber}", ct);

        return AdminResult.Ok(shipment.Id);
    }

    // ---------------- helpers ----------------

    private static void SyncOrderStatus(Order order, ShipmentStatus status, DateTime now)
    {
        var mapped = status switch
        {
            ShipmentStatus.LabelCreated => OrderStatus.Packed,
            ShipmentStatus.PickedUp => OrderStatus.Shipped,
            ShipmentStatus.InTransit => OrderStatus.Shipped,
            ShipmentStatus.OutForDelivery => OrderStatus.OutForDelivery,
            ShipmentStatus.Delivered => OrderStatus.Delivered,
            _ => order.Status // Failed leaves the order where it is
        };
        if (mapped != order.Status)
        {
            order.Status = mapped;
            order.UpdatedAtUtc = now;
        }
    }

    private static string Describe(ShipmentStatus status) => status switch
    {
        ShipmentStatus.LabelCreated => "shipping label created",
        ShipmentStatus.PickedUp => "picked up by the courier",
        ShipmentStatus.InTransit => "in transit",
        ShipmentStatus.OutForDelivery => "out for delivery",
        ShipmentStatus.Delivered => "delivered",
        ShipmentStatus.Failed => "a delivery attempt failed",
        _ => status.ToString()
    };

    private async Task<ShipmentDetailVm?> BuildDetailAsync(string orderNumber, CancellationToken ct)
    {
        var vm = await _uow.Repository<Shipment>().Query()
            .Where(s => s.Order.OrderNumber == orderNumber)
            .Select(s => new ShipmentDetailVm
            {
                OrderNumber = orderNumber,
                Exists = true,
                Courier = s.Courier,
                TrackingNumber = s.TrackingNumber,
                Status = s.Status,
                EstimatedDeliveryUtc = s.EstimatedDeliveryUtc,
                ShippedAtUtc = s.ShippedAtUtc,
                DeliveredAtUtc = s.DeliveredAtUtc,
                Events = s.Events.OrderByDescending(e => e.OccurredAtUtc)
                    .Select(e => new ShipmentEventVm { Status = e.Status, Note = e.Note, OccurredAtUtc = e.OccurredAtUtc })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        return vm ?? new ShipmentDetailVm { OrderNumber = orderNumber, Exists = false };
    }
}

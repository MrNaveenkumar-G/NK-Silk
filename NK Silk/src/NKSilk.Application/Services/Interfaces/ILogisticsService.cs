using NKSilk.Application.ViewModels;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services.Interfaces;

public interface ILogisticsService
{
    Task<IReadOnlyList<AdminShipmentListItemVm>> GetAllAsync(ShipmentStatus? status, CancellationToken ct = default);

    /// <summary>Admin view of an order's shipment (Exists=false when none created yet); null if the order is unknown.</summary>
    Task<ShipmentDetailVm?> GetForOrderAsync(string orderNumber, CancellationToken ct = default);

    /// <summary>Customer-scoped tracking for one of their own orders.</summary>
    Task<ShipmentDetailVm?> GetTrackingForCustomerAsync(int customerId, string orderNumber, CancellationToken ct = default);

    Task<AdminResult> CreateShipmentAsync(string orderNumber, string courier, string trackingNumber,
        DateTime? estimatedDelivery, CancellationToken ct = default);

    /// <summary>Adds a tracking event, advancing the shipment and syncing the order status.</summary>
    Task<AdminResult> AddEventAsync(string orderNumber, ShipmentStatus status, string? note, CancellationToken ct = default);
}

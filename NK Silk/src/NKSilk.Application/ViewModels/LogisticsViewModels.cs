using NKSilk.Domain.Enums;

namespace NKSilk.Application.ViewModels;

public class AdminShipmentListItemVm
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Courier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; }
    public DateTime? ShippedAtUtc { get; set; }
}

public class ShipmentEventVm
{
    public ShipmentStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}

/// <summary>Shipment view shared by the admin manager and the customer tracking page.</summary>
public class ShipmentDetailVm
{
    public string OrderNumber { get; set; } = string.Empty;
    public bool Exists { get; set; }

    public string Courier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; }

    public DateTime? EstimatedDeliveryUtc { get; set; }
    public DateTime? ShippedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }

    public IReadOnlyList<ShipmentEventVm> Events { get; set; } = new List<ShipmentEventVm>();
}

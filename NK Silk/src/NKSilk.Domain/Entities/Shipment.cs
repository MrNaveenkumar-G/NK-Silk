using NKSilk.Domain.Common;
using NKSilk.Domain.Enums;

namespace NKSilk.Domain.Entities;

/// <summary>
/// The logistics record for an order: courier, tracking number, current status and a
/// timeline of tracking events. One shipment per order in this model.
/// </summary>
public class Shipment : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public string Courier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; } = ShipmentStatus.LabelCreated;

    public DateTime? EstimatedDeliveryUtc { get; set; }
    public DateTime? ShippedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }

    public ICollection<ShipmentEvent> Events { get; set; } = new List<ShipmentEvent>();
}

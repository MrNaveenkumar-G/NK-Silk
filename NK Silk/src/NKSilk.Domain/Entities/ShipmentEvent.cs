using NKSilk.Domain.Common;
using NKSilk.Domain.Enums;

namespace NKSilk.Domain.Entities;

/// <summary>A single scan/checkpoint in a shipment's tracking timeline.</summary>
public class ShipmentEvent : BaseEntity
{
    public int ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public ShipmentStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}

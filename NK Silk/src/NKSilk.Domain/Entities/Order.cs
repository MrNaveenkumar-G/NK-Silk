using NKSilk.Domain.Common;
using NKSilk.Domain.Enums;

namespace NKSilk.Domain.Entities;

/// <summary>A placed order header with money totals and current status.</summary>
public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int ShippingAddressId { get; set; }
    public Address ShippingAddress { get; set; } = null!;

    public int? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
    public ICollection<Return> Returns { get; set; } = new List<Return>();
    public Shipment? Shipment { get; set; }
}

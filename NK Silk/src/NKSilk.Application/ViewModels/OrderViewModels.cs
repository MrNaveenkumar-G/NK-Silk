using System.ComponentModel.DataAnnotations;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.ViewModels;

/// <summary>Shipping details + payment choice captured on the checkout page.</summary>
public class PlaceOrderVm
{
    [Required, StringLength(150)]
    [Display(Name = "Full name")]
    public string ContactName { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    [Display(Name = "Mobile number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(250)]
    [Display(Name = "Address line 1")]
    public string Line1 { get; set; } = string.Empty;

    [StringLength(250)]
    [Display(Name = "Address line 2")]
    public string? Line2 { get; set; }

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string State { get; set; } = string.Empty;

    [Required, StringLength(12)]
    [Display(Name = "PIN code")]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Payment method")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
}

/// <summary>Checkout page model: cart lines + computed money totals.</summary>
public class CheckoutVm
{
    public CartVm Cart { get; set; } = new();
    public PlaceOrderVm Form { get; set; } = new();

    public string? AppliedCouponCode { get; set; }
    public decimal DiscountAmount { get; set; }

    public decimal SubTotal => Cart.SubTotal;
    public decimal ComboSavings => Cart.ComboSavings;
    public decimal ShippingFee => SubTotal - ComboSavings >= 999m || SubTotal == 0 ? 0m : 49m;
    public decimal GrandTotal => SubTotal - ComboSavings - DiscountAmount + ShippingFee;
}

public class OrderResult
{
    public bool Succeeded { get; private set; }
    public string? Error { get; private set; }
    public int OrderId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;

    public static OrderResult Success(int id, string number)
        => new() { Succeeded = true, OrderId = id, OrderNumber = number };
    public static OrderResult Fail(string error) => new() { Succeeded = false, Error = error };
}

public class OrderListItemVm
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime PlacedAtUtc { get; set; }
    public OrderStatus Status { get; set; }
    public decimal GrandTotal { get; set; }
    public int ItemCount { get; set; }
}

public class OrderLineVm
{
    public string ProductName { get; set; } = string.Empty;
    public string? ColorName { get; set; }
    public string? SizeName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class OrderDetailVm
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime PlacedAtUtc { get; set; }
    public OrderStatus Status { get; set; }

    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    public string ShipToName { get; set; } = string.Empty;
    public string ShipToPhone { get; set; } = string.Empty;
    public string ShipToAddress { get; set; } = string.Empty;

    public IReadOnlyList<OrderLineVm> Lines { get; set; } = new List<OrderLineVm>();

    /// <summary>True when the order is delivered and a return can still be raised.</summary>
    public bool CanRequestReturn { get; set; }
    /// <summary>The latest non-rejected return for this order, if any.</summary>
    public string? ActiveReturnNumber { get; set; }
}

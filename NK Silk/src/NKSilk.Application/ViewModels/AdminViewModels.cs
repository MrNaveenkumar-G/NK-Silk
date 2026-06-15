using System.ComponentModel.DataAnnotations;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.ViewModels;

public class AdminDashboardVm
{
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockVariants { get; set; }
    public int PendingReturns { get; set; }
    public IReadOnlyList<AdminOrderListItemVm> RecentOrders { get; set; } = new List<AdminOrderListItemVm>();
}

public class AdminProductListItemVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int VariantCount { get; set; }
    public int TotalStock { get; set; }
}

public class AdminProductEditVm
{
    public int Id { get; set; }

    [Required, StringLength(250)]
    public string Name { get; set; } = string.Empty;

    [StringLength(260)]
    [Display(Name = "Slug (URL)")]
    public string? Slug { get; set; }

    [Required, StringLength(64)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Range(0, 9999999)]
    [Display(Name = "Selling price (₹)")]
    public decimal BasePrice { get; set; }

    [Range(0, 9999999)]
    [Display(Name = "MRP (₹)")]
    public decimal? MrpPrice { get; set; }

    [Display(Name = "Fabric type")]
    public string? FabricType { get; set; }
    [Display(Name = "Material composition")]
    public string? MaterialComposition { get; set; }
    public int? Gsm { get; set; }
    [Display(Name = "Wash care")]
    public string? WashCare { get; set; }
    public string? Occasion { get; set; }

    [Display(Name = "Short description")]
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    [Display(Name = "Featured on home page")]
    public bool IsFeatured { get; set; }

    // Dropdown source
    public IReadOnlyList<CategoryVm> Categories { get; set; } = new List<CategoryVm>();
}

public class AdminCategoryVm
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(160)]
    public string? Slug { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public int ProductCount { get; set; }
}

public class AdminInventoryItemVm
{
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? ColorName { get; set; }
    public string? SizeName { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int ReorderLevel { get; set; }
    public int Available => QuantityOnHand - QuantityReserved;
    public bool IsLow => Available <= ReorderLevel;
}

public class AdminOrderListItemVm
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime PlacedAtUtc { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal GrandTotal { get; set; }
}

public class AdminOrderDetailVm
{
    public OrderDetailVm Order { get; set; } = new();
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
}

public class AdminCustomerVm
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime JoinedAtUtc { get; set; }
    public int OrderCount { get; set; }
}

public class AdminCouponVm
{
    public int Id { get; set; }

    [Required, StringLength(40)]
    public string Code { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }

    [Display(Name = "Discount type")]
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

    [Range(0, 1000000)]
    [Display(Name = "Discount value")]
    public decimal DiscountValue { get; set; }

    [Display(Name = "Min order amount (₹)")]
    public decimal? MinOrderAmount { get; set; }

    [Display(Name = "Max discount (₹)")]
    public decimal? MaxDiscountAmount { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Starts")]
    public DateTime StartsAtUtc { get; set; } = DateTime.UtcNow.Date;

    [DataType(DataType.Date)]
    [Display(Name = "Ends")]
    public DateTime EndsAtUtc { get; set; } = DateTime.UtcNow.Date.AddMonths(1);

    [Display(Name = "Usage limit")]
    public int? UsageLimit { get; set; }
    public int TimesUsed { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AdminReviewVm
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

/// <summary>Generic operation outcome for admin mutations.</summary>
public class AdminResult
{
    public bool Succeeded { get; private set; }
    public string? Error { get; private set; }
    public int Id { get; private set; }
    public static AdminResult Ok(int id = 0) => new() { Succeeded = true, Id = id };
    public static AdminResult Fail(string error) => new() { Succeeded = false, Error = error };
}

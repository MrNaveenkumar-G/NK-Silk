using NKSilk.Domain.Enums;

namespace NKSilk.Application.ViewModels;

public class VendorDashboardVm
{
    public string VendorName { get; set; } = string.Empty;
    public decimal CommissionRate { get; set; }
    public int ProductCount { get; set; }
    public int ActiveProductCount { get; set; }
    public int LowStockCount { get; set; }

    public int UnitsSold { get; set; }
    public decimal GrossSales { get; set; }
    public decimal Commission => Math.Round(GrossSales * CommissionRate / 100m, 2);
    public decimal NetPayout => GrossSales - Commission;
}

public class VendorProductVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public int VariantCount { get; set; }
    public int TotalStock { get; set; }
}

public class VendorOrderItemVm
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime PlacedAtUtc { get; set; }
    public OrderStatus Status { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ColorName { get; set; }
    public string? SizeName { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

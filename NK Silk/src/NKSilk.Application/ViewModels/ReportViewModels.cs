using NKSilk.Domain.Enums;

namespace NKSilk.Application.ViewModels;

public class DailySalesVm
{
    public DateTime Date { get; set; }
    public int Orders { get; set; }
    public decimal Revenue { get; set; }
}

public class TopProductVm
{
    public string ProductName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
}

public class TopCategoryVm
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class StatusCountVm
{
    public OrderStatus Status { get; set; }
    public int Count { get; set; }
}

public class LowStockVm
{
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int OnHand { get; set; }
    public int ReorderLevel { get; set; }
}

/// <summary>Aggregated sales analytics over a trailing window (read-only).</summary>
public class SalesReportVm
{
    public int Days { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue => OrderCount == 0 ? 0m : TotalRevenue / OrderCount;

    public int ReturnsCount { get; set; }
    public decimal ReturnsValue { get; set; }

    public IReadOnlyList<DailySalesVm> Daily { get; set; } = new List<DailySalesVm>();
    public IReadOnlyList<TopProductVm> TopProducts { get; set; } = new List<TopProductVm>();
    public IReadOnlyList<TopCategoryVm> TopCategories { get; set; } = new List<TopCategoryVm>();
    public IReadOnlyList<StatusCountVm> StatusBreakdown { get; set; } = new List<StatusCountVm>();
    public IReadOnlyList<LowStockVm> LowStock { get; set; } = new List<LowStockVm>();

    /// <summary>Peak daily revenue, used to scale the bar chart.</summary>
    public decimal MaxDailyRevenue => Daily.Count == 0 ? 0m : Daily.Max(d => d.Revenue);
}

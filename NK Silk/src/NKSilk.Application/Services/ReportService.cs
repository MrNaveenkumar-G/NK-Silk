using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services;

/// <summary>Read-only sales analytics aggregated over a trailing window.</summary>
public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;
    public ReportService(IUnitOfWork uow) => _uow = uow;

    public async Task<SalesReportVm> GetSalesReportAsync(int days, CancellationToken ct = default)
    {
        if (days <= 0) days = 30;
        var since = DateTime.UtcNow.Date.AddDays(-(days - 1));

        var orders = _uow.Repository<Order>().Query();
        // Revenue counts everything except cancelled orders.
        var revenueOrders = orders.Where(o => o.CreatedAtUtc >= since && o.Status != OrderStatus.Cancelled);

        var totalRevenue = await revenueOrders.SumAsync(o => (decimal?)o.GrandTotal, ct) ?? 0m;
        var orderCount = await revenueOrders.CountAsync(ct);

        var daily = (await revenueOrders
                .GroupBy(o => o.CreatedAtUtc.Date)
                .Select(g => new DailySalesVm
                {
                    Date = g.Key,
                    Orders = g.Count(),
                    Revenue = g.Sum(o => o.GrandTotal)
                })
                .ToListAsync(ct))
            .OrderBy(d => d.Date)
            .ToList();

        var statusBreakdown = await orders
            .Where(o => o.CreatedAtUtc >= since)
            .GroupBy(o => o.Status)
            .Select(g => new StatusCountVm { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        // Line-level aggregates: join order items of in-window, non-cancelled orders.
        var lines = _uow.Repository<OrderItem>().Query()
            .Where(i => i.Order.CreatedAtUtc >= since && i.Order.Status != OrderStatus.Cancelled);

        var topProducts = await lines
            .GroupBy(i => i.ProductName)
            .Select(g => new TopProductVm
            {
                ProductName = g.Key,
                UnitsSold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.LineTotal)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToListAsync(ct);

        var topCategories = await lines
            .GroupBy(i => i.ProductVariant.Product.Category.Name)
            .Select(g => new TopCategoryVm { CategoryName = g.Key, Revenue = g.Sum(i => i.LineTotal) })
            .OrderByDescending(c => c.Revenue)
            .Take(8)
            .ToListAsync(ct);

        var returnsInWindow = _uow.Repository<Return>().Query()
            .Where(r => r.CreatedAtUtc >= since && r.Status != ReturnStatus.Rejected);
        var returnsCount = await returnsInWindow.CountAsync(ct);
        var returnsValue = await returnsInWindow.SumAsync(r => (decimal?)r.RefundAmount, ct) ?? 0m;

        var lowStock = await _uow.Repository<Inventory>().Query()
            .Where(i => i.QuantityOnHand - i.QuantityReserved <= i.ReorderLevel)
            .OrderBy(i => i.QuantityOnHand)
            .Take(20)
            .Select(i => new LowStockVm
            {
                ProductName = i.ProductVariant.Product.Name,
                Sku = i.ProductVariant.Sku,
                OnHand = i.QuantityOnHand,
                ReorderLevel = i.ReorderLevel
            })
            .ToListAsync(ct);

        return new SalesReportVm
        {
            Days = days,
            TotalRevenue = totalRevenue,
            OrderCount = orderCount,
            ReturnsCount = returnsCount,
            ReturnsValue = returnsValue,
            Daily = daily,
            TopProducts = topProducts,
            TopCategories = topCategories,
            StatusBreakdown = statusBreakdown,
            LowStock = lowStock
        };
    }
}

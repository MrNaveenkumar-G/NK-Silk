using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Web.Infrastructure;

/// <summary>
/// Background worker that periodically scans inventory and notifies admins about variants
/// that have fallen to/below their reorder level. Demonstrates the background-worker seam
/// (queued notifications, invoicing and analytics rollups would run the same way).
/// </summary>
public class LowStockMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LowStockMonitorService> _log;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    public LowStockMonitorService(IServiceScopeFactory scopeFactory, ILogger<LowStockMonitorService> log)
    {
        _scopeFactory = scopeFactory;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small initial delay so startup migration/seeding finishes first.
        try { await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken); } catch { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await RunOnceAsync(stoppingToken); }
            catch (Exception ex) { _log.LogError(ex, "Low-stock monitor run failed"); }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var lowCount = await uow.Repository<Inventory>().Query()
            .CountAsync(i => i.QuantityOnHand - i.QuantityReserved <= i.ReorderLevel, ct);
        if (lowCount == 0) return;

        var admins = await uow.Repository<Customer>().Query()
            .Where(c => c.IsAdmin).Select(c => c.Id).ToListAsync(ct);

        foreach (var adminId in admins)
            await notifications.NotifyAsync(adminId, NotificationType.General,
                "Low stock alert",
                $"{lowCount} product variant(s) are at or below their reorder level.",
                "/Admin/Inventory", ct);

        _log.LogInformation("Low-stock monitor notified {Admins} admin(s) about {Count} variant(s)", admins.Count, lowCount);
    }
}

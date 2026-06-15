using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services;

/// <summary>
/// Persists in-app notifications and fans them out to the customer's email/SMS via the
/// configured sender. Transport failures are swallowed so they never break the calling
/// use case (placing an order, changing a status, etc.).
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationSender _sender;

    public NotificationService(IUnitOfWork uow, INotificationSender sender)
    {
        _uow = uow;
        _sender = sender;
    }

    public async Task NotifyAsync(int customerId, NotificationType type, string title, string message,
        string? linkUrl = null, CancellationToken ct = default)
    {
        if (customerId <= 0) return;

        await _uow.Repository<Notification>().AddAsync(new Notification
        {
            CustomerId = customerId,
            Type = type,
            Title = title,
            Message = message,
            LinkUrl = linkUrl,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        }, ct);
        await _uow.SaveChangesAsync(ct);

        // Best-effort email/SMS — a transport failure must not roll back the business action.
        try
        {
            var customer = await _uow.Repository<Customer>().GetByIdAsync(customerId, ct);
            if (customer is not null)
            {
                await _sender.SendEmailAsync(customer.Email, title, message, ct);
                await _sender.SendSmsAsync(customer.PhoneNumber, $"{title}: {message}", ct);
            }
        }
        catch
        {
            // Swallow — the in-app notification is the durable record.
        }
    }

    public async Task<NotificationListVm> GetForCustomerAsync(int customerId, CancellationToken ct = default)
    {
        var items = await _uow.Repository<Notification>().Query()
            .Where(n => n.CustomerId == customerId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(50)
            .Select(n => new NotificationVm
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                LinkUrl = n.LinkUrl,
                IsRead = n.IsRead,
                CreatedAtUtc = n.CreatedAtUtc
            })
            .ToListAsync(ct);

        return new NotificationListVm { Items = items };
    }

    public Task<int> UnreadCountAsync(int customerId, CancellationToken ct = default)
        => _uow.Repository<Notification>().CountAsync(n => n.CustomerId == customerId && !n.IsRead, ct);

    public async Task MarkReadAsync(int customerId, int notificationId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Notification>();
        var n = await repo.Query(asNoTracking: false)
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.CustomerId == customerId, ct);
        if (n is null || n.IsRead) return;
        n.IsRead = true;
        repo.Update(n);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task MarkAllReadAsync(int customerId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Notification>();
        var unread = await repo.Query(asNoTracking: false)
            .Where(n => n.CustomerId == customerId && !n.IsRead)
            .ToListAsync(ct);
        if (unread.Count == 0) return;
        foreach (var n in unread) n.IsRead = true;
        await _uow.SaveChangesAsync(ct);
    }
}

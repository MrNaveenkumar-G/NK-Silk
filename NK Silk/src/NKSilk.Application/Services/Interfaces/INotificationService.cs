using NKSilk.Application.ViewModels;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Records an in-app notification for the customer and dispatches an email/SMS copy
    /// through the configured sender. Never throws on transport failure — the in-app row
    /// is the source of truth.
    /// </summary>
    Task NotifyAsync(int customerId, NotificationType type, string title, string message,
        string? linkUrl = null, CancellationToken ct = default);

    Task<NotificationListVm> GetForCustomerAsync(int customerId, CancellationToken ct = default);
    Task<int> UnreadCountAsync(int customerId, CancellationToken ct = default);
    Task MarkReadAsync(int customerId, int notificationId, CancellationToken ct = default);
    Task MarkAllReadAsync(int customerId, CancellationToken ct = default);
}

using NKSilk.Domain.Common;
using NKSilk.Domain.Enums;

namespace NKSilk.Domain.Entities;

/// <summary>
/// An in-app notification addressed to a customer (order/payment/return events).
/// Persisted alongside any email/SMS dispatched through INotificationSender.
/// </summary>
public class Notification : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    /// <summary>Optional deep link (e.g. /Orders/Details/NK2026...).</summary>
    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; }
}

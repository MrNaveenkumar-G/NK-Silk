using NKSilk.Domain.Enums;

namespace NKSilk.Application.ViewModels;

public class NotificationVm
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class NotificationListVm
{
    public IReadOnlyList<NotificationVm> Items { get; set; } = new List<NotificationVm>();
    public int UnreadCount => Items.Count(i => !i.IsRead);
    public bool IsEmpty => Items.Count == 0;
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Web.Infrastructure;

namespace NKSilk.Web.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notifications;
    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _notifications.GetForCustomerAsync(User.GetCustomerId(), ct));

    /// <summary>Marks a notification read and forwards to its deep link (or the list).</summary>
    public async Task<IActionResult> Open(int id, CancellationToken ct)
    {
        await _notifications.MarkReadAsync(User.GetCustomerId(), id, ct);

        var list = await _notifications.GetForCustomerAsync(User.GetCustomerId(), ct);
        var target = list.Items.FirstOrDefault(n => n.Id == id)?.LinkUrl;
        if (!string.IsNullOrWhiteSpace(target) && Url.IsLocalUrl(target))
            return Redirect(target);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await _notifications.MarkAllReadAsync(User.GetCustomerId(), ct);
        return RedirectToAction(nameof(Index));
    }

    // Polled by the navbar bell badge.
    public async Task<IActionResult> Count(CancellationToken ct)
        => Json(new { count = await _notifications.UnreadCountAsync(User.GetCustomerId(), ct) });
}

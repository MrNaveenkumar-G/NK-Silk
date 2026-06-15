using Microsoft.Extensions.Logging;
using NKSilk.Application.Common.Interfaces;

namespace NKSilk.Infrastructure.Notifications;

/// <summary>
/// Development notification transport: writes email/SMS payloads to the application log
/// instead of dispatching them. Swap this registration for a real SMTP/SendGrid/Twilio
/// implementation in production — the <see cref="INotificationSender"/> contract is unchanged.
/// </summary>
public class LoggingNotificationSender : INotificationSender
{
    private readonly ILogger<LoggingNotificationSender> _log;

    public LoggingNotificationSender(ILogger<LoggingNotificationSender> log) => _log = log;

    public Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        _log.LogInformation("[EMAIL → {To}] {Subject} — {Body}", toEmail, subject, body);
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string? toPhone, string message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toPhone)) return Task.CompletedTask;
        _log.LogInformation("[SMS → {To}] {Message}", toPhone, message);
        return Task.CompletedTask;
    }
}

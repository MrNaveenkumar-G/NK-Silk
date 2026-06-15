namespace NKSilk.Application.Common.Interfaces;

/// <summary>
/// Outbound transport for customer notifications (email / SMS). The Infrastructure layer
/// supplies the implementation; a logging simulator runs in dev until a real provider
/// (SMTP, SendGrid, Twilio, etc.) is configured.
/// </summary>
public interface INotificationSender
{
    Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken ct = default);
    Task SendSmsAsync(string? toPhone, string message, CancellationToken ct = default);
}

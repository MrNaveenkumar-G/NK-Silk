using NKSilk.Application.ViewModels;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services.Interfaces;

public interface ISupportService
{
    // Customer
    Task<SupportResult> CreateAsync(int customerId, SupportTicketFormVm form, CancellationToken ct = default);
    Task<IReadOnlyList<SupportTicketListItemVm>> GetForCustomerAsync(int customerId, CancellationToken ct = default);
    Task<SupportTicketDetailVm?> GetForCustomerAsync(int customerId, string ticketNumber, CancellationToken ct = default);
    Task<SupportResult> CustomerReplyAsync(int customerId, string ticketNumber, string body, CancellationToken ct = default);

    // Admin
    Task<IReadOnlyList<SupportTicketListItemVm>> GetAllAsync(TicketStatus? status, CancellationToken ct = default);
    Task<SupportTicketDetailVm?> GetAsync(string ticketNumber, CancellationToken ct = default);
    Task<SupportResult> StaffReplyAsync(string ticketNumber, string authorName, string body, CancellationToken ct = default);
    Task<SupportResult> SetStatusAsync(string ticketNumber, TicketStatus status, CancellationToken ct = default);
    Task<int> CountOpenAsync(CancellationToken ct = default);
}

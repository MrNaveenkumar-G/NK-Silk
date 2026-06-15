using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services;

/// <summary>Customer support tickets with a message thread and admin-driven status lifecycle.</summary>
public class SupportService : ISupportService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notifications;

    public SupportService(IUnitOfWork uow, INotificationService notifications)
    {
        _uow = uow;
        _notifications = notifications;
    }

    public async Task<SupportResult> CreateAsync(int customerId, SupportTicketFormVm form, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>().GetByIdAsync(customerId, ct);
        if (customer is null) return SupportResult.Fail("Account not found.");

        int? orderId = null;
        if (!string.IsNullOrWhiteSpace(form.OrderNumber))
        {
            orderId = await _uow.Repository<Order>().Query()
                .Where(o => o.OrderNumber == form.OrderNumber && o.CustomerId == customerId)
                .Select(o => (int?)o.Id)
                .FirstOrDefaultAsync(ct);
            if (orderId is null) return SupportResult.Fail("That order number isn't on your account.");
        }

        var now = DateTime.UtcNow;
        var ticket = new SupportTicket
        {
            TicketNumber = $"TK{now:yyyyMMddHHmmssfff}",
            Subject = form.Subject.Trim(),
            Category = form.Category,
            Status = TicketStatus.Open,
            CustomerId = customerId,
            OrderId = orderId,
            CreatedAtUtc = now
        };
        ticket.Messages.Add(new SupportMessage
        {
            Body = form.Message.Trim(),
            IsStaff = false,
            AuthorName = customer.FullName,
            CreatedAtUtc = now
        });

        await _uow.Repository<SupportTicket>().AddAsync(ticket, ct);
        await _uow.SaveChangesAsync(ct);
        return SupportResult.Success(ticket.TicketNumber);
    }

    public async Task<IReadOnlyList<SupportTicketListItemVm>> GetForCustomerAsync(int customerId, CancellationToken ct = default)
        => await ListQuery(_uow.Repository<SupportTicket>().Query().Where(t => t.CustomerId == customerId)).ToListAsync(ct);

    public Task<SupportTicketDetailVm?> GetForCustomerAsync(int customerId, string ticketNumber, CancellationToken ct = default)
        => DetailQuery(t => t.CustomerId == customerId && t.TicketNumber == ticketNumber).FirstOrDefaultAsync(ct);

    public async Task<SupportResult> CustomerReplyAsync(int customerId, string ticketNumber, string body, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(body)) return SupportResult.Fail("Message cannot be empty.");
        var ticket = await LoadTrackedAsync(t => t.TicketNumber == ticketNumber && t.CustomerId == customerId, ct);
        if (ticket is null) return SupportResult.Fail("Ticket not found.");
        if (ticket.Status == TicketStatus.Closed) return SupportResult.Fail("This ticket is closed.");

        var customer = await _uow.Repository<Customer>().GetByIdAsync(customerId, ct);
        ticket.Messages.Add(new SupportMessage
        {
            SupportTicketId = ticket.Id,
            Body = body.Trim(),
            IsStaff = false,
            AuthorName = customer?.FullName ?? "Customer",
            CreatedAtUtc = DateTime.UtcNow
        });
        ticket.Status = TicketStatus.Open; // customer reply re-opens for staff
        _uow.Repository<SupportTicket>().Update(ticket);
        await _uow.SaveChangesAsync(ct);
        return SupportResult.Success(ticket.TicketNumber);
    }

    public async Task<IReadOnlyList<SupportTicketListItemVm>> GetAllAsync(TicketStatus? status, CancellationToken ct = default)
    {
        var q = _uow.Repository<SupportTicket>().Query();
        if (status is not null) q = q.Where(t => t.Status == status);
        return await ListQuery(q).ToListAsync(ct);
    }

    public Task<SupportTicketDetailVm?> GetAsync(string ticketNumber, CancellationToken ct = default)
        => DetailQuery(t => t.TicketNumber == ticketNumber).FirstOrDefaultAsync(ct);

    public async Task<SupportResult> StaffReplyAsync(string ticketNumber, string authorName, string body, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(body)) return SupportResult.Fail("Message cannot be empty.");
        var ticket = await LoadTrackedAsync(t => t.TicketNumber == ticketNumber, ct);
        if (ticket is null) return SupportResult.Fail("Ticket not found.");

        var now = DateTime.UtcNow;
        ticket.Messages.Add(new SupportMessage
        {
            SupportTicketId = ticket.Id,
            Body = body.Trim(),
            IsStaff = true,
            AuthorName = string.IsNullOrWhiteSpace(authorName) ? "Support" : authorName,
            CreatedAtUtc = now
        });
        if (ticket.Status is TicketStatus.Open) ticket.Status = TicketStatus.AwaitingCustomer;
        _uow.Repository<SupportTicket>().Update(ticket);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(ticket.CustomerId, NotificationType.SupportReply,
            "Support replied",
            $"Support replied to ticket {ticket.TicketNumber}: {ticket.Subject}.",
            $"/Support/Details/{ticket.TicketNumber}", ct);

        return SupportResult.Success(ticket.TicketNumber);
    }

    public async Task<SupportResult> SetStatusAsync(string ticketNumber, TicketStatus status, CancellationToken ct = default)
    {
        var ticket = await LoadTrackedAsync(t => t.TicketNumber == ticketNumber, ct);
        if (ticket is null) return SupportResult.Fail("Ticket not found.");
        ticket.Status = status;
        _uow.Repository<SupportTicket>().Update(ticket);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(ticket.CustomerId, NotificationType.SupportReply,
            $"Ticket {status}",
            $"Your support ticket {ticket.TicketNumber} is now {status}.",
            $"/Support/Details/{ticket.TicketNumber}", ct);

        return SupportResult.Success(ticket.TicketNumber);
    }

    public Task<int> CountOpenAsync(CancellationToken ct = default)
        => _uow.Repository<SupportTicket>().CountAsync(t => t.Status == TicketStatus.Open, ct);

    // ---------------- helpers ----------------

    private Task<SupportTicket?> LoadTrackedAsync(System.Linq.Expressions.Expression<Func<SupportTicket, bool>> predicate, CancellationToken ct)
        => _uow.Repository<SupportTicket>().Query(asNoTracking: false)
            .Include(t => t.Messages)
            .FirstOrDefaultAsync(predicate, ct);

    private static IQueryable<SupportTicketListItemVm> ListQuery(IQueryable<SupportTicket> q)
        => q.OrderByDescending(t => t.UpdatedAtUtc ?? t.CreatedAtUtc)
            .Select(t => new SupportTicketListItemVm
            {
                TicketNumber = t.TicketNumber,
                Subject = t.Subject,
                Category = t.Category,
                Status = t.Status,
                OrderNumber = t.Order != null ? t.Order.OrderNumber : null,
                CustomerName = t.Customer.FullName,
                LastActivityUtc = t.UpdatedAtUtc ?? t.CreatedAtUtc
            });

    private IQueryable<SupportTicketDetailVm> DetailQuery(System.Linq.Expressions.Expression<Func<SupportTicket, bool>> predicate)
        => _uow.Repository<SupportTicket>().Query()
            .Where(predicate)
            .Select(t => new SupportTicketDetailVm
            {
                TicketNumber = t.TicketNumber,
                Subject = t.Subject,
                Category = t.Category,
                Status = t.Status,
                OrderNumber = t.Order != null ? t.Order.OrderNumber : null,
                CustomerName = t.Customer.FullName,
                CustomerEmail = t.Customer.Email,
                Messages = t.Messages.OrderBy(m => m.CreatedAtUtc)
                    .Select(m => new SupportMessageVm
                    {
                        Body = m.Body,
                        IsStaff = m.IsStaff,
                        AuthorName = m.AuthorName,
                        CreatedAtUtc = m.CreatedAtUtc
                    }).ToList()
            });
}

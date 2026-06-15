using NKSilk.Domain.Common;
using NKSilk.Domain.Enums;

namespace NKSilk.Domain.Entities;

/// <summary>A customer support enquiry, optionally tied to an order, with a message thread.</summary>
public class SupportTicket : BaseEntity
{
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public TicketCategory Category { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    /// <summary>Optional order this enquiry relates to.</summary>
    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    public ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
}

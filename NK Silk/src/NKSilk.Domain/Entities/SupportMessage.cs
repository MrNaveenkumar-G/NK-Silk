using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>A single message in a support ticket thread (from the customer or staff).</summary>
public class SupportMessage : BaseEntity
{
    public int SupportTicketId { get; set; }
    public SupportTicket SupportTicket { get; set; } = null!;

    public string Body { get; set; } = string.Empty;
    /// <summary>True when written by support staff/admin, false when written by the customer.</summary>
    public bool IsStaff { get; set; }
    public string AuthorName { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.ViewModels;

public class SupportTicketFormVm
{
    [Required, StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    public TicketCategory Category { get; set; } = TicketCategory.Order;

    [Display(Name = "Related order (optional)")]
    public string? OrderNumber { get; set; }

    [Required(ErrorMessage = "Please describe your issue."), StringLength(4000)]
    public string Message { get; set; } = string.Empty;
}

public class SupportResult
{
    public bool Succeeded { get; private set; }
    public string? Error { get; private set; }
    public string TicketNumber { get; private set; } = string.Empty;

    public static SupportResult Success(string number) => new() { Succeeded = true, TicketNumber = number };
    public static SupportResult Fail(string error) => new() { Succeeded = false, Error = error };
}

public class SupportTicketListItemVm
{
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public TicketCategory Category { get; set; }
    public TicketStatus Status { get; set; }
    public string? OrderNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime LastActivityUtc { get; set; }
}

public class SupportMessageVm
{
    public string Body { get; set; } = string.Empty;
    public bool IsStaff { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class SupportTicketDetailVm
{
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public TicketCategory Category { get; set; }
    public TicketStatus Status { get; set; }
    public string? OrderNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public IReadOnlyList<SupportMessageVm> Messages { get; set; } = new List<SupportMessageVm>();
}

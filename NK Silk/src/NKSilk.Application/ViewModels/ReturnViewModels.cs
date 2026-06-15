using System.ComponentModel.DataAnnotations;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.ViewModels;

/// <summary>A returnable order line shown on the return-request form.</summary>
public class ReturnableLineVm
{
    public int OrderItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ColorName { get; set; }
    public string? SizeName { get; set; }
    public int QuantityOrdered { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>Model backing the customer return-request page for one delivered order.</summary>
public class ReturnRequestVm
{
    public string OrderNumber { get; set; } = string.Empty;
    public ReturnReason Reason { get; set; } = ReturnReason.DefectiveOrDamaged;

    [StringLength(1000)]
    public string? Comments { get; set; }

    public IReadOnlyList<ReturnableLineVm> Lines { get; set; } = new List<ReturnableLineVm>();

    /// <summary>Per order-item quantity the customer wants to return (posted back).</summary>
    public Dictionary<int, int> Quantities { get; set; } = new();
}

public class ReturnResult
{
    public bool Succeeded { get; private set; }
    public string? Error { get; private set; }
    public string ReturnNumber { get; private set; } = string.Empty;

    public static ReturnResult Success(string number) => new() { Succeeded = true, ReturnNumber = number };
    public static ReturnResult Fail(string error) => new() { Succeeded = false, Error = error };
}

public class ReturnLineVm
{
    public string ProductName { get; set; } = string.Empty;
    public string? ColorName { get; set; }
    public string? SizeName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class ReturnListItemVm
{
    public int Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public ReturnStatus Status { get; set; }
    public ReturnReason Reason { get; set; }
    public decimal RefundAmount { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public string? CustomerName { get; set; }
}

public class ReturnDetailVm
{
    public int Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public ReturnStatus Status { get; set; }
    public ReturnReason Reason { get; set; }
    public string? Comments { get; set; }
    public string? ResolutionNote { get; set; }
    public decimal RefundAmount { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    public IReadOnlyList<ReturnLineVm> Lines { get; set; } = new List<ReturnLineVm>();
}

using NKSilk.Application.ViewModels;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services.Interfaces;

public interface IReturnService
{
    /// <summary>Builds the return-request form for a delivered order, or null if it isn't eligible.</summary>
    Task<ReturnRequestVm?> GetRequestFormAsync(int customerId, string orderNumber, CancellationToken ct = default);

    /// <summary>Creates a return request from the posted form (validates eligibility &amp; quantities).</summary>
    Task<ReturnResult> CreateAsync(int customerId, ReturnRequestVm form, CancellationToken ct = default);

    // Customer views
    Task<IReadOnlyList<ReturnListItemVm>> GetForCustomerAsync(int customerId, CancellationToken ct = default);
    Task<ReturnDetailVm?> GetForCustomerAsync(int customerId, string returnNumber, CancellationToken ct = default);

    // Admin views & actions
    Task<IReadOnlyList<ReturnListItemVm>> GetAllAsync(ReturnStatus? status, CancellationToken ct = default);
    Task<ReturnDetailVm?> GetAsync(string returnNumber, CancellationToken ct = default);
    Task<int> CountPendingAsync(CancellationToken ct = default);

    /// <summary>Approve or reject a requested return (note is optional, used for rejections).</summary>
    Task<ReturnResult> SetApprovalAsync(string returnNumber, bool approved, string? note, CancellationToken ct = default);

    /// <summary>Mark an approved return as picked up from the customer.</summary>
    Task<ReturnResult> MarkPickedUpAsync(string returnNumber, CancellationToken ct = default);

    /// <summary>
    /// Settle a return: restock the returned units, refund the payment (gateway when applicable),
    /// and move the return → Refunded and the order → Returned.
    /// </summary>
    Task<ReturnResult> RefundAsync(string returnNumber, CancellationToken ct = default);
}

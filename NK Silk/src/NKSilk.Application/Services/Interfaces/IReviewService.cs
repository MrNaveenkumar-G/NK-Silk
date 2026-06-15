using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewVm>> GetApprovedAsync(int productId, CancellationToken ct = default);

    /// <summary>Submits a review (held for moderation). Fails if the product doesn't exist.</summary>
    Task<bool> AddAsync(int customerId, ReviewFormVm form, CancellationToken ct = default);
}

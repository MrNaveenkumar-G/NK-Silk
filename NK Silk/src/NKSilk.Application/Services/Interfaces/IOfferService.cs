using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface IOfferService
{
    // Storefront
    Task<IReadOnlyList<OfferCardVm>> GetActiveBannersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<OfferCardVm>> GetActiveOffersAsync(CancellationToken ct = default);

    // Admin
    Task<IReadOnlyList<AdminOfferVm>> GetAllAsync(CancellationToken ct = default);
    Task<AdminOfferVm?> GetForEditAsync(int? id, CancellationToken ct = default);
    Task<AdminResult> SaveAsync(AdminOfferVm vm, CancellationToken ct = default);
    Task<AdminResult> ToggleActiveAsync(int id, CancellationToken ct = default);
    Task<AdminResult> DeleteAsync(int id, CancellationToken ct = default);
}

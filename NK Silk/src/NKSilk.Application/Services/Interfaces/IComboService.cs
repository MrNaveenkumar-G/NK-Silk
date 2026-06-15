using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface IComboService
{
    // Storefront
    Task<IReadOnlyList<ComboCardVm>> GetActiveAsync(CancellationToken ct = default);
    Task<ComboDetailVm?> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Default buyable (variantId, qty) pairs for a combo, for adding the bundle to the cart.</summary>
    Task<IReadOnlyList<(int variantId, int qty)>> GetCartVariantsAsync(int comboId, CancellationToken ct = default);

    // Admin
    Task<IReadOnlyList<AdminComboVm>> GetAllAsync(CancellationToken ct = default);
    Task<AdminComboVm?> GetForEditAsync(int? id, CancellationToken ct = default);
    Task<AdminResult> SaveAsync(AdminComboVm vm, CancellationToken ct = default);
    Task<AdminResult> ToggleActiveAsync(int id, CancellationToken ct = default);
}

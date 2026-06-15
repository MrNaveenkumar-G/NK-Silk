using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface IWishlistService
{
    Task<WishlistVm> GetAsync(int customerId, CancellationToken ct = default);

    /// <summary>Adds the product if absent, removes it if present. Returns true when now saved.</summary>
    Task<bool> ToggleAsync(int customerId, int productId, CancellationToken ct = default);

    Task RemoveAsync(int customerId, int productId, CancellationToken ct = default);
    Task<int> CountAsync(int customerId, CancellationToken ct = default);
    Task<bool> ContainsAsync(int customerId, int productId, CancellationToken ct = default);
}

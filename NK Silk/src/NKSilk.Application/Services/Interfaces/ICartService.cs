using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface ICartService
{
    Task<CartVm> GetCartAsync(string cartKey, CancellationToken ct = default);
    Task<CartVm> AddItemAsync(string cartKey, int productVariantId, int quantity, CancellationToken ct = default);
    Task<CartVm> UpdateQuantityAsync(string cartKey, int cartItemId, int quantity, CancellationToken ct = default);
    Task<CartVm> RemoveItemAsync(string cartKey, int cartItemId, CancellationToken ct = default);
    Task<int> GetItemCountAsync(string cartKey, CancellationToken ct = default);
}

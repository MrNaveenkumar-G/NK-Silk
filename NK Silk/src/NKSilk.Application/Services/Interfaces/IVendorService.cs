using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface IVendorService
{
    Task<VendorDashboardVm?> GetDashboardAsync(int vendorId, CancellationToken ct = default);
    Task<IReadOnlyList<VendorProductVm>> GetProductsAsync(int vendorId, CancellationToken ct = default);
    Task<AdminResult> ToggleProductActiveAsync(int vendorId, int productId, CancellationToken ct = default);
    Task<IReadOnlyList<AdminInventoryItemVm>> GetInventoryAsync(int vendorId, CancellationToken ct = default);
    Task<AdminResult> UpdateStockAsync(int vendorId, int variantId, int quantityOnHand, int reorderLevel, CancellationToken ct = default);
    Task<IReadOnlyList<VendorOrderItemVm>> GetOrderItemsAsync(int vendorId, CancellationToken ct = default);
}

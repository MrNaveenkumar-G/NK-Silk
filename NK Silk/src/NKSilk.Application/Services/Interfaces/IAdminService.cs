using NKSilk.Application.ViewModels;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardVm> GetDashboardAsync(CancellationToken ct = default);

    // Products
    Task<IReadOnlyList<AdminProductListItemVm>> GetProductsAsync(string? search, CancellationToken ct = default);
    Task<AdminProductEditVm?> GetProductForEditAsync(int? id, CancellationToken ct = default);
    Task<AdminResult> SaveProductAsync(AdminProductEditVm vm, CancellationToken ct = default);
    Task<AdminResult> ToggleProductActiveAsync(int id, CancellationToken ct = default);

    // Categories
    Task<IReadOnlyList<AdminCategoryVm>> GetCategoriesAsync(CancellationToken ct = default);
    Task<AdminResult> SaveCategoryAsync(AdminCategoryVm vm, CancellationToken ct = default);

    // Inventory
    Task<IReadOnlyList<AdminInventoryItemVm>> GetInventoryAsync(string? search, CancellationToken ct = default);
    Task<AdminResult> UpdateStockAsync(int variantId, int quantityOnHand, int reorderLevel, CancellationToken ct = default);

    // Orders
    Task<IReadOnlyList<AdminOrderListItemVm>> GetOrdersAsync(OrderStatus? status, CancellationToken ct = default);
    Task<AdminOrderDetailVm?> GetOrderAsync(string orderNumber, CancellationToken ct = default);
    Task<AdminResult> UpdateOrderStatusAsync(string orderNumber, OrderStatus status, CancellationToken ct = default);

    // Customers
    Task<IReadOnlyList<AdminCustomerVm>> GetCustomersAsync(string? search, CancellationToken ct = default);

    // Coupons
    Task<IReadOnlyList<AdminCouponVm>> GetCouponsAsync(CancellationToken ct = default);
    Task<AdminCouponVm?> GetCouponForEditAsync(int? id, CancellationToken ct = default);
    Task<AdminResult> SaveCouponAsync(AdminCouponVm vm, CancellationToken ct = default);

    // Reviews (moderation)
    Task<IReadOnlyList<AdminReviewVm>> GetReviewsAsync(bool? approved, CancellationToken ct = default);
    Task<AdminResult> SetReviewApprovalAsync(int reviewId, bool approved, CancellationToken ct = default);
    Task<AdminResult> DeleteReviewAsync(int reviewId, CancellationToken ct = default);
}

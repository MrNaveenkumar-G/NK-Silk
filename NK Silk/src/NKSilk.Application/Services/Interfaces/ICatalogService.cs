using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface ICatalogService
{
    Task<IReadOnlyList<CategoryVm>> GetCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProductCardVm>> GetFeaturedAsync(int take = 8, CancellationToken ct = default);

    Task<ProductListVm> GetProductsAsync(
        string? categorySlug = null,
        string? search = null,
        int page = 1,
        int pageSize = 12,
        CancellationToken ct = default);

    Task<ProductDetailVm?> GetProductBySlugAsync(string slug, CancellationToken ct = default);
}

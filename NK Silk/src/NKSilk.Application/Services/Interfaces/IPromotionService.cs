using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

/// <summary>
/// Central promotions engine. Resolves the best active offer per product and any matched
/// combo-pack savings for a set of cart lines, so the cart, checkout and order all price
/// promotions identically.
/// </summary>
public interface IPromotionService
{
    Task<PromotionResultVm> EvaluateAsync(IReadOnlyList<PromotionLineVm> lines, CancellationToken ct = default);

    /// <summary>Best active offer price per product (for storefront cards/detail). Keyed by productId.</summary>
    Task<IReadOnlyDictionary<int, (decimal price, string title)>> ResolveProductOffersAsync(
        IReadOnlyList<(int productId, int categoryId, decimal price)> products, CancellationToken ct = default);
}

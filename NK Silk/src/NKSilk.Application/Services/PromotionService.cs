using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services;

/// <summary>
/// Resolves automatic offers (per product/category/store) and combo-pack savings.
/// Shared by the storefront (display prices), the cart and checkout so promotional
/// pricing is computed exactly once and consistently everywhere.
/// </summary>
public class PromotionService : IPromotionService
{
    private readonly IUnitOfWork _uow;
    public PromotionService(IUnitOfWork uow) => _uow = uow;

    private sealed record ActiveOffer(OfferType Type, decimal Value, OfferScope Scope, int? CategoryId, int? ProductId, int Priority, string Title);

    private async Task<List<ActiveOffer>> LoadActiveOffersAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return await _uow.Repository<Offer>().Query()
            .Where(o => o.IsActive && o.StartsAtUtc <= now && o.EndsAtUtc >= now)
            .Select(o => new ActiveOffer(o.OfferType, o.Value, o.Scope, o.CategoryId, o.ProductId, o.Priority, o.Title))
            .ToListAsync(ct);
    }

    /// <summary>Best (largest) discounted unit price for a product, or null when no offer applies.</summary>
    private static (decimal price, string title)? BestOffer(List<ActiveOffer> offers, int productId, int categoryId, decimal price)
    {
        (decimal price, string title)? best = null;
        var bestPriority = int.MinValue;
        foreach (var o in offers)
        {
            var matches = o.Scope switch
            {
                OfferScope.EntireStore => true,
                OfferScope.Category => o.CategoryId == categoryId,
                OfferScope.Product => o.ProductId == productId,
                _ => false
            };
            if (!matches) continue;

            var discount = o.Type == OfferType.PercentageOff ? price * o.Value / 100m : Math.Min(o.Value, price);
            var candidate = Math.Max(0m, Math.Round(price - discount, 2));
            if (best is null || candidate < best.Value.price || (candidate == best.Value.price && o.Priority > bestPriority))
            {
                best = (candidate, o.Title);
                bestPriority = o.Priority;
            }
        }
        return best;
    }

    public async Task<IReadOnlyDictionary<int, (decimal price, string title)>> ResolveProductOffersAsync(
        IReadOnlyList<(int productId, int categoryId, decimal price)> products, CancellationToken ct = default)
    {
        var result = new Dictionary<int, (decimal, string)>();
        if (products.Count == 0) return result;
        var offers = await LoadActiveOffersAsync(ct);
        if (offers.Count == 0) return result;
        foreach (var p in products)
        {
            var best = BestOffer(offers, p.productId, p.categoryId, p.price);
            if (best is not null && best.Value.price < p.price)
                result[p.productId] = best.Value;
        }
        return result;
    }

    public async Task<PromotionResultVm> EvaluateAsync(IReadOnlyList<PromotionLineVm> lines, CancellationToken ct = default)
    {
        var effective = new List<decimal>(lines.Count);
        var titles = new List<string?>(lines.Count);
        decimal offerSavings = 0m;

        var offers = await LoadActiveOffersAsync(ct);
        foreach (var l in lines)
        {
            var best = offers.Count == 0 ? null : BestOffer(offers, l.ProductId, l.CategoryId, l.UnitPrice);
            if (best is not null && best.Value.price < l.UnitPrice)
            {
                effective.Add(best.Value.price);
                titles.Add(best.Value.title);
                offerSavings += (l.UnitPrice - best.Value.price) * l.Quantity;
            }
            else
            {
                effective.Add(l.UnitPrice);
                titles.Add(null);
            }
        }

        // Per-product effective price + available quantity for combo matching.
        var effPrice = new Dictionary<int, decimal>();
        var qtyByProduct = new Dictionary<int, int>();
        for (var i = 0; i < lines.Count; i++)
        {
            effPrice[lines[i].ProductId] = effective[i];
            qtyByProduct[lines[i].ProductId] = qtyByProduct.GetValueOrDefault(lines[i].ProductId) + lines[i].Quantity;
        }

        decimal comboSavings = 0m;
        var applied = new List<string>();
        if (qtyByProduct.Count > 0)
        {
            var combos = await _uow.Repository<ComboPack>().Query()
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.Name,
                    c.ComboPrice,
                    Items = c.Items.Select(i => new { i.ProductId, i.Quantity }).ToList()
                })
                .ToListAsync(ct);

            foreach (var combo in combos)
            {
                if (combo.Items.Count == 0) continue;
                var satisfied = combo.Items.All(i => qtyByProduct.GetValueOrDefault(i.ProductId) >= i.Quantity);
                if (!satisfied) continue;

                var regular = combo.Items.Sum(i => effPrice.GetValueOrDefault(i.ProductId) * i.Quantity);
                var saving = Math.Round(regular - combo.ComboPrice, 2);
                if (saving > 0)
                {
                    comboSavings += saving;
                    applied.Add(combo.Name);
                }
            }
        }

        return new PromotionResultVm
        {
            EffectiveUnitPrices = effective,
            LineOfferTitles = titles,
            OfferSavings = offerSavings,
            ComboSavings = comboSavings,
            AppliedCombos = applied
        };
    }
}

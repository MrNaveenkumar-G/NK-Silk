using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;

namespace NKSilk.Application.Services;

/// <summary>DB-backed cart keyed by the visitor's cart cookie token.</summary>
public class CartService : ICartService
{
    private readonly IUnitOfWork _uow;
    private readonly IPromotionService _promotions;

    public CartService(IUnitOfWork uow, IPromotionService promotions)
    {
        _uow = uow;
        _promotions = promotions;
    }

    public async Task<CartVm> GetCartAsync(string cartKey, CancellationToken ct = default)
    {
        var cart = await LoadCartAsync(cartKey, ct);
        return await MapAsync(cart, ct);
    }

    public async Task<int> GetItemCountAsync(string cartKey, CancellationToken ct = default)
    {
        return await _uow.Repository<CartItem>().Query()
            .Where(i => i.Cart.CartKey == cartKey)
            .SumAsync(i => (int?)i.Quantity, ct) ?? 0;
    }

    public async Task<CartVm> AddItemAsync(string cartKey, int productVariantId, int quantity, CancellationToken ct = default)
    {
        if (quantity < 1) quantity = 1;

        var variant = await _uow.Repository<ProductVariant>().Query()
            .FirstOrDefaultAsync(v => v.Id == productVariantId && v.IsActive, ct)
            ?? throw new InvalidOperationException("Variant not found or inactive.");

        var cart = await GetOrCreateCartAsync(cartKey, ct);

        var line = cart.Items.FirstOrDefault(i => i.ProductVariantId == productVariantId);
        if (line is null)
        {
            cart.Items.Add(new CartItem
            {
                ProductVariantId = productVariantId,
                Quantity = quantity,
                UnitPrice = variant.Price,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            line.Quantity += quantity;
            line.UnitPrice = variant.Price; // refresh to current price
            line.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _uow.SaveChangesAsync(ct);
        return await GetCartAsync(cartKey, ct);
    }

    public async Task<CartVm> UpdateQuantityAsync(string cartKey, int cartItemId, int quantity, CancellationToken ct = default)
    {
        var cart = await LoadCartAsync(cartKey, ct);
        var line = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (line is not null)
        {
            if (quantity < 1)
                cart.Items.Remove(line);
            else
            {
                line.Quantity = quantity;
                line.UpdatedAtUtc = DateTime.UtcNow;
            }
            await _uow.SaveChangesAsync(ct);
        }
        return await MapAsync(cart, ct);
    }

    public async Task<CartVm> RemoveItemAsync(string cartKey, int cartItemId, CancellationToken ct = default)
    {
        var cart = await LoadCartAsync(cartKey, ct);
        var line = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (line is not null)
        {
            cart.Items.Remove(line);
            await _uow.SaveChangesAsync(ct);
        }
        return await MapAsync(cart, ct);
    }

    // ---- helpers ----

    private async Task<Cart> GetOrCreateCartAsync(string cartKey, CancellationToken ct)
    {
        var repo = _uow.Repository<Cart>();
        var cart = await repo.Query(asNoTracking: false)
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CartKey == cartKey, ct);

        if (cart is null)
        {
            cart = new Cart { CartKey = cartKey, CreatedAtUtc = DateTime.UtcNow };
            await repo.AddAsync(cart, ct);
        }
        return cart;
    }

    private async Task<Cart> LoadCartAsync(string cartKey, CancellationToken ct)
    {
        var cart = await _uow.Repository<Cart>().Query(asNoTracking: false)
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product).ThenInclude(p => p.Images)
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Color)
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Size)
            .FirstOrDefaultAsync(c => c.CartKey == cartKey, ct);

        return cart ?? new Cart { CartKey = cartKey };
    }

    private async Task<CartVm> MapAsync(Cart cart, CancellationToken ct)
    {
        var lines = cart.Items.Select(i => new CartLineVm
        {
            CartItemId = i.Id,
            ProductVariantId = i.ProductVariantId,
            ProductId = i.ProductVariant?.ProductId ?? 0,
            ProductName = i.ProductVariant?.Product?.Name ?? "Item",
            ProductSlug = i.ProductVariant?.Product?.Slug ?? "",
            ImageUrl = i.ProductVariant?.Product?.Images
                .OrderByDescending(im => im.IsPrimary).ThenBy(im => im.DisplayOrder)
                .Select(im => im.Url).FirstOrDefault(),
            ColorName = i.ProductVariant?.Color?.Name,
            SizeName = i.ProductVariant?.Size?.Name,
            UnitPrice = i.UnitPrice,
            OriginalUnitPrice = i.UnitPrice,
            Quantity = i.Quantity
        }).ToList();

        var vm = new CartVm { Lines = lines };
        if (lines.Count == 0) return vm;

        // Apply offers + combo savings via the shared promotions engine.
        var promoLines = cart.Items.Select(i => new PromotionLineVm
        {
            ProductId = i.ProductVariant?.ProductId ?? 0,
            CategoryId = i.ProductVariant?.Product?.CategoryId ?? 0,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity
        }).ToList();

        var promo = await _promotions.EvaluateAsync(promoLines, ct);
        for (var idx = 0; idx < lines.Count && idx < promo.EffectiveUnitPrices.Count; idx++)
        {
            lines[idx].UnitPrice = promo.EffectiveUnitPrices[idx];
            lines[idx].OfferTitle = promo.LineOfferTitles.Count > idx ? promo.LineOfferTitles[idx] : null;
        }
        vm.OfferSavings = promo.OfferSavings;
        vm.ComboSavings = promo.ComboSavings;
        vm.AppliedCombos = promo.AppliedCombos;
        return vm;
    }
}

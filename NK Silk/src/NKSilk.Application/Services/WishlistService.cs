using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;

namespace NKSilk.Application.Services;

public class WishlistService : IWishlistService
{
    private readonly IUnitOfWork _uow;
    public WishlistService(IUnitOfWork uow) => _uow = uow;

    public async Task<WishlistVm> GetAsync(int customerId, CancellationToken ct = default)
    {
        var items = await _uow.Repository<WishlistItem>().Query()
            .Where(w => w.CustomerId == customerId)
            .OrderByDescending(w => w.CreatedAtUtc)
            .Select(w => new WishlistItemVm
            {
                ProductId = w.ProductId,
                Name = w.Product.Name,
                Slug = w.Product.Slug,
                Price = w.Product.BasePrice,
                FabricType = w.Product.FabricType,
                ImageUrl = w.Product.Images
                    .OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder)
                    .Select(i => i.Url).FirstOrDefault()
            })
            .ToListAsync(ct);

        return new WishlistVm { Items = items };
    }

    public async Task<bool> ToggleAsync(int customerId, int productId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<WishlistItem>();
        var existing = await repo.Query(asNoTracking: false)
            .FirstOrDefaultAsync(w => w.CustomerId == customerId && w.ProductId == productId, ct);

        if (existing is not null)
        {
            // Soft-delete; the filtered unique index lets the pair be re-added later.
            repo.Remove(existing);
            await _uow.SaveChangesAsync(ct);
            return false;
        }

        await repo.AddAsync(new WishlistItem
        {
            CustomerId = customerId,
            ProductId = productId,
            CreatedAtUtc = DateTime.UtcNow
        }, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task RemoveAsync(int customerId, int productId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<WishlistItem>();
        var existing = await repo.Query(asNoTracking: false)
            .FirstOrDefaultAsync(w => w.CustomerId == customerId && w.ProductId == productId, ct);
        if (existing is not null)
        {
            repo.Remove(existing);
            await _uow.SaveChangesAsync(ct);
        }
    }

    public async Task<int> CountAsync(int customerId, CancellationToken ct = default)
        => await _uow.Repository<WishlistItem>().CountAsync(w => w.CustomerId == customerId, ct);

    public async Task<bool> ContainsAsync(int customerId, int productId, CancellationToken ct = default)
        => await _uow.Repository<WishlistItem>().Query()
            .AnyAsync(w => w.CustomerId == customerId && w.ProductId == productId, ct);
}

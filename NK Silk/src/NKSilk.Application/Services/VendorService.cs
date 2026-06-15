using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services;

/// <summary>
/// Vendor-scoped marketplace operations. Every query is filtered to the seller's own
/// VendorId so a vendor can only see and mutate their own products, stock and order lines.
/// </summary>
public class VendorService : IVendorService
{
    private readonly IUnitOfWork _uow;
    public VendorService(IUnitOfWork uow) => _uow = uow;

    public async Task<VendorDashboardVm?> GetDashboardAsync(int vendorId, CancellationToken ct = default)
    {
        var vendor = await _uow.Repository<Vendor>().GetByIdAsync(vendorId, ct);
        if (vendor is null) return null;

        var products = _uow.Repository<Product>().Query().Where(p => p.VendorId == vendorId);
        var soldLines = _uow.Repository<OrderItem>().Query()
            .Where(i => i.ProductVariant.Product.VendorId == vendorId && i.Order.Status != OrderStatus.Cancelled);

        return new VendorDashboardVm
        {
            VendorName = vendor.Name,
            CommissionRate = vendor.CommissionRate,
            ProductCount = await products.CountAsync(ct),
            ActiveProductCount = await products.CountAsync(p => p.IsActive, ct),
            LowStockCount = await _uow.Repository<Inventory>().Query()
                .CountAsync(i => i.ProductVariant.Product.VendorId == vendorId
                                 && i.QuantityOnHand - i.QuantityReserved <= i.ReorderLevel, ct),
            UnitsSold = await soldLines.SumAsync(i => (int?)i.Quantity, ct) ?? 0,
            GrossSales = await soldLines.SumAsync(i => (decimal?)i.LineTotal, ct) ?? 0m
        };
    }

    public async Task<IReadOnlyList<VendorProductVm>> GetProductsAsync(int vendorId, CancellationToken ct = default)
        => await _uow.Repository<Product>().Query()
            .Where(p => p.VendorId == vendorId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new VendorProductVm
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category.Name,
                BasePrice = p.BasePrice,
                IsActive = p.IsActive,
                VariantCount = p.Variants.Count,
                TotalStock = p.Variants.Sum(v => v.Inventory != null ? v.Inventory.QuantityOnHand : 0)
            }).ToListAsync(ct);

    public async Task<AdminResult> ToggleProductActiveAsync(int vendorId, int productId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Product>();
        var p = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == productId && x.VendorId == vendorId, ct);
        if (p is null) return AdminResult.Fail("Product not found.");
        p.IsActive = !p.IsActive;
        repo.Update(p);
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(productId);
    }

    public async Task<IReadOnlyList<AdminInventoryItemVm>> GetInventoryAsync(int vendorId, CancellationToken ct = default)
        => await _uow.Repository<ProductVariant>().Query()
            .Where(v => v.Product.VendorId == vendorId)
            .OrderBy(v => v.Product.Name)
            .Select(v => new AdminInventoryItemVm
            {
                VariantId = v.Id,
                ProductName = v.Product.Name,
                Sku = v.Sku,
                ColorName = v.Color != null ? v.Color.Name : null,
                SizeName = v.Size != null ? v.Size.Name : null,
                QuantityOnHand = v.Inventory != null ? v.Inventory.QuantityOnHand : 0,
                QuantityReserved = v.Inventory != null ? v.Inventory.QuantityReserved : 0,
                ReorderLevel = v.Inventory != null ? v.Inventory.ReorderLevel : 0
            }).ToListAsync(ct);

    public async Task<AdminResult> UpdateStockAsync(int vendorId, int variantId, int quantityOnHand, int reorderLevel, CancellationToken ct = default)
    {
        if (quantityOnHand < 0 || reorderLevel < 0) return AdminResult.Fail("Quantities cannot be negative.");

        // Ownership check: the variant's product must belong to this vendor.
        var owns = await _uow.Repository<ProductVariant>().Query()
            .AnyAsync(v => v.Id == variantId && v.Product.VendorId == vendorId, ct);
        if (!owns) return AdminResult.Fail("Variant not found.");

        var invRepo = _uow.Repository<Inventory>();
        var inv = await invRepo.Query(asNoTracking: false).FirstOrDefaultAsync(i => i.ProductVariantId == variantId, ct);
        if (inv is null)
        {
            inv = new Inventory { ProductVariantId = variantId, CreatedAtUtc = DateTime.UtcNow };
            await invRepo.AddAsync(inv, ct);
        }
        inv.QuantityOnHand = quantityOnHand;
        inv.ReorderLevel = reorderLevel;
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(variantId);
    }

    public async Task<IReadOnlyList<VendorOrderItemVm>> GetOrderItemsAsync(int vendorId, CancellationToken ct = default)
        => await _uow.Repository<OrderItem>().Query()
            .Where(i => i.ProductVariant.Product.VendorId == vendorId)
            .OrderByDescending(i => i.Order.CreatedAtUtc)
            .Select(i => new VendorOrderItemVm
            {
                OrderNumber = i.Order.OrderNumber,
                PlacedAtUtc = i.Order.CreatedAtUtc,
                Status = i.Order.Status,
                ProductName = i.ProductName,
                ColorName = i.ColorName,
                SizeName = i.SizeName,
                Quantity = i.Quantity,
                LineTotal = i.LineTotal
            }).ToListAsync(ct);
}

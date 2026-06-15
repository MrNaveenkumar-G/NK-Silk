using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notifications;

    public AdminService(IUnitOfWork uow, INotificationService notifications)
    {
        _uow = uow;
        _notifications = notifications;
    }

    // ---------------- Dashboard ----------------
    public async Task<AdminDashboardVm> GetDashboardAsync(CancellationToken ct = default)
    {
        var orders = _uow.Repository<Order>().Query();
        var recent = await orders
            .OrderByDescending(o => o.CreatedAtUtc)
            .Take(8)
            .Select(o => new AdminOrderListItemVm
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.Customer.FullName,
                PlacedAtUtc = o.CreatedAtUtc,
                Status = o.Status,
                PaymentStatus = o.Payment != null ? o.Payment.Status : PaymentStatus.Pending,
                PaymentMethod = o.Payment != null ? o.Payment.Method : PaymentMethod.CashOnDelivery,
                GrandTotal = o.GrandTotal
            })
            .ToListAsync(ct);

        return new AdminDashboardVm
        {
            TotalProducts = await _uow.Repository<Product>().CountAsync(ct: ct),
            TotalOrders = await orders.CountAsync(ct),
            TotalCustomers = await _uow.Repository<Customer>().CountAsync(c => !c.IsAdmin, ct),
            TotalRevenue = await orders.Where(o => o.Status != OrderStatus.Cancelled)
                                       .SumAsync(o => (decimal?)o.GrandTotal, ct) ?? 0m,
            PendingOrders = await orders.CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed, ct),
            LowStockVariants = await _uow.Repository<Inventory>().Query()
                .CountAsync(i => i.QuantityOnHand - i.QuantityReserved <= i.ReorderLevel, ct),
            PendingReturns = await _uow.Repository<Return>().CountAsync(r => r.Status == ReturnStatus.Requested, ct),
            RecentOrders = recent
        };
    }

    // ---------------- Products ----------------
    public async Task<IReadOnlyList<AdminProductListItemVm>> GetProductsAsync(string? search, CancellationToken ct = default)
    {
        var q = _uow.Repository<Product>().Query();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(p => p.Name.Contains(term) || p.Sku.Contains(term));
        }

        return await q.OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new AdminProductListItemVm
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category.Name,
                BasePrice = p.BasePrice,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                VariantCount = p.Variants.Count,
                TotalStock = p.Variants.Sum(v => v.Inventory != null ? v.Inventory.QuantityOnHand : 0)
            })
            .ToListAsync(ct);
    }

    public async Task<AdminProductEditVm?> GetProductForEditAsync(int? id, CancellationToken ct = default)
    {
        var categories = await CategoryDropdownAsync(ct);

        if (id is null or 0)
            return new AdminProductEditVm { Categories = categories, IsActive = true };

        var vm = await _uow.Repository<Product>().Query()
            .Where(p => p.Id == id)
            .Select(p => new AdminProductEditVm
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Sku = p.Sku,
                CategoryId = p.CategoryId,
                BasePrice = p.BasePrice,
                MrpPrice = p.MrpPrice,
                FabricType = p.FabricType,
                MaterialComposition = p.MaterialComposition,
                Gsm = p.Gsm,
                WashCare = p.WashCare,
                Occasion = p.Occasion,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured
            })
            .FirstOrDefaultAsync(ct);

        if (vm is null) return null;
        vm.Categories = categories;
        return vm;
    }

    public async Task<AdminResult> SaveProductAsync(AdminProductEditVm vm, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Product>();
        var slug = string.IsNullOrWhiteSpace(vm.Slug) ? Slugify(vm.Name) : Slugify(vm.Slug);

        // Uniqueness checks (exclude self).
        if (await repo.Query().AnyAsync(p => p.Slug == slug && p.Id != vm.Id, ct))
            return AdminResult.Fail($"Slug '{slug}' is already in use.");
        if (await repo.Query().AnyAsync(p => p.Sku == vm.Sku && p.Id != vm.Id, ct))
            return AdminResult.Fail($"SKU '{vm.Sku}' is already in use.");

        if (vm.Id == 0)
        {
            var product = new Product
            {
                Name = vm.Name.Trim(),
                Slug = slug,
                Sku = vm.Sku.Trim(),
                CategoryId = vm.CategoryId,
                BasePrice = vm.BasePrice,
                MrpPrice = vm.MrpPrice,
                FabricType = vm.FabricType,
                MaterialComposition = vm.MaterialComposition,
                Gsm = vm.Gsm,
                WashCare = vm.WashCare,
                Occasion = vm.Occasion,
                ShortDescription = vm.ShortDescription,
                Description = vm.Description,
                IsActive = vm.IsActive,
                IsFeatured = vm.IsFeatured,
                CreatedAtUtc = DateTime.UtcNow
            };
            product.Images.Add(new ProductImage { Url = "/img/product-placeholder.svg", IsPrimary = true, CreatedAtUtc = DateTime.UtcNow });
            // Seed a default buyable variant so the product works on the storefront immediately.
            product.Variants.Add(new ProductVariant
            {
                Sku = vm.Sku.Trim() + "-1",
                Price = vm.BasePrice,
                MrpPrice = vm.MrpPrice,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                Inventory = new Inventory { QuantityOnHand = 0, ReorderLevel = 5, CreatedAtUtc = DateTime.UtcNow }
            });

            await repo.AddAsync(product, ct);
            await _uow.SaveChangesAsync(ct);
            return AdminResult.Ok(product.Id);
        }

        var existing = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(p => p.Id == vm.Id, ct);
        if (existing is null) return AdminResult.Fail("Product not found.");

        existing.Name = vm.Name.Trim();
        existing.Slug = slug;
        existing.Sku = vm.Sku.Trim();
        existing.CategoryId = vm.CategoryId;
        existing.BasePrice = vm.BasePrice;
        existing.MrpPrice = vm.MrpPrice;
        existing.FabricType = vm.FabricType;
        existing.MaterialComposition = vm.MaterialComposition;
        existing.Gsm = vm.Gsm;
        existing.WashCare = vm.WashCare;
        existing.Occasion = vm.Occasion;
        existing.ShortDescription = vm.ShortDescription;
        existing.Description = vm.Description;
        existing.IsActive = vm.IsActive;
        existing.IsFeatured = vm.IsFeatured;

        repo.Update(existing);
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(existing.Id);
    }

    public async Task<AdminResult> ToggleProductActiveAsync(int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Product>();
        var p = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return AdminResult.Fail("Product not found.");
        p.IsActive = !p.IsActive;
        repo.Update(p);
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(id);
    }

    // ---------------- Categories ----------------
    public async Task<IReadOnlyList<AdminCategoryVm>> GetCategoriesAsync(CancellationToken ct = default)
        => await _uow.Repository<Category>().Query()
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new AdminCategoryVm
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count
            }).ToListAsync(ct);

    public async Task<AdminResult> SaveCategoryAsync(AdminCategoryVm vm, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Category>();
        var slug = string.IsNullOrWhiteSpace(vm.Slug) ? Slugify(vm.Name) : Slugify(vm.Slug);
        if (await repo.Query().AnyAsync(c => c.Slug == slug && c.Id != vm.Id, ct))
            return AdminResult.Fail($"Slug '{slug}' is already in use.");

        if (vm.Id == 0)
        {
            var c = new Category
            {
                Name = vm.Name.Trim(), Slug = slug, DisplayOrder = vm.DisplayOrder,
                IsActive = vm.IsActive, CreatedAtUtc = DateTime.UtcNow
            };
            await repo.AddAsync(c, ct);
            await _uow.SaveChangesAsync(ct);
            return AdminResult.Ok(c.Id);
        }

        var existing = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(c => c.Id == vm.Id, ct);
        if (existing is null) return AdminResult.Fail("Category not found.");
        existing.Name = vm.Name.Trim();
        existing.Slug = slug;
        existing.DisplayOrder = vm.DisplayOrder;
        existing.IsActive = vm.IsActive;
        repo.Update(existing);
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(existing.Id);
    }

    // ---------------- Inventory ----------------
    public async Task<IReadOnlyList<AdminInventoryItemVm>> GetInventoryAsync(string? search, CancellationToken ct = default)
    {
        var q = _uow.Repository<ProductVariant>().Query();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(v => v.Product.Name.Contains(term) || v.Sku.Contains(term));
        }

        return await q.OrderBy(v => v.Product.Name)
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
    }

    public async Task<AdminResult> UpdateStockAsync(int variantId, int quantityOnHand, int reorderLevel, CancellationToken ct = default)
    {
        if (quantityOnHand < 0 || reorderLevel < 0) return AdminResult.Fail("Quantities cannot be negative.");

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

    // ---------------- Orders ----------------
    public async Task<IReadOnlyList<AdminOrderListItemVm>> GetOrdersAsync(OrderStatus? status, CancellationToken ct = default)
    {
        var q = _uow.Repository<Order>().Query();
        if (status is not null) q = q.Where(o => o.Status == status);

        return await q.OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new AdminOrderListItemVm
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.Customer.FullName,
                PlacedAtUtc = o.CreatedAtUtc,
                Status = o.Status,
                PaymentStatus = o.Payment != null ? o.Payment.Status : PaymentStatus.Pending,
                PaymentMethod = o.Payment != null ? o.Payment.Method : PaymentMethod.CashOnDelivery,
                GrandTotal = o.GrandTotal
            }).ToListAsync(ct);
    }

    public async Task<AdminOrderDetailVm?> GetOrderAsync(string orderNumber, CancellationToken ct = default)
    {
        return await _uow.Repository<Order>().Query()
            .Where(o => o.OrderNumber == orderNumber)
            .Select(o => new AdminOrderDetailVm
            {
                CustomerName = o.Customer.FullName,
                CustomerEmail = o.Customer.Email,
                Order = new OrderDetailVm
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    PlacedAtUtc = o.CreatedAtUtc,
                    Status = o.Status,
                    SubTotal = o.SubTotal,
                    ShippingFee = o.ShippingFee,
                    DiscountAmount = o.DiscountAmount,
                    GrandTotal = o.GrandTotal,
                    PaymentMethod = o.Payment != null ? o.Payment.Method : PaymentMethod.CashOnDelivery,
                    PaymentStatus = o.Payment != null ? o.Payment.Status : PaymentStatus.Pending,
                    ShipToName = o.ShippingAddress.ContactName,
                    ShipToPhone = o.ShippingAddress.PhoneNumber,
                    ShipToAddress = o.ShippingAddress.Line1
                        + (o.ShippingAddress.Line2 != null ? ", " + o.ShippingAddress.Line2 : "")
                        + ", " + o.ShippingAddress.City + ", " + o.ShippingAddress.State + " - " + o.ShippingAddress.PostalCode,
                    Lines = o.Items.Select(i => new OrderLineVm
                    {
                        ProductName = i.ProductName,
                        ColorName = i.ColorName,
                        SizeName = i.SizeName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        LineTotal = i.LineTotal
                    }).ToList()
                }
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AdminResult> UpdateOrderStatusAsync(string orderNumber, OrderStatus status, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Order>();
        var order = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
        if (order is null) return AdminResult.Fail("Order not found.");
        if (order.Status == status) return AdminResult.Ok(order.Id);

        order.Status = status;
        repo.Update(order);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(order.CustomerId, NotificationType.OrderStatusChanged,
            $"Order {status}",
            $"Your order {order.OrderNumber} is now {status}.",
            $"/Orders/Details/{order.OrderNumber}", ct);

        return AdminResult.Ok(order.Id);
    }

    // ---------------- Customers ----------------
    public async Task<IReadOnlyList<AdminCustomerVm>> GetCustomersAsync(string? search, CancellationToken ct = default)
    {
        var q = _uow.Repository<Customer>().Query();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(c => c.FullName.Contains(term) || c.Email.Contains(term));
        }

        return await q.OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => new AdminCustomerVm
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                IsActive = c.IsActive,
                IsAdmin = c.IsAdmin,
                JoinedAtUtc = c.CreatedAtUtc,
                OrderCount = c.Orders.Count
            }).ToListAsync(ct);
    }

    // ---------------- Coupons ----------------
    public async Task<IReadOnlyList<AdminCouponVm>> GetCouponsAsync(CancellationToken ct = default)
        => await _uow.Repository<Coupon>().Query()
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => new AdminCouponVm
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MinOrderAmount = c.MinOrderAmount,
                MaxDiscountAmount = c.MaxDiscountAmount,
                StartsAtUtc = c.StartsAtUtc,
                EndsAtUtc = c.EndsAtUtc,
                UsageLimit = c.UsageLimit,
                TimesUsed = c.TimesUsed,
                IsActive = c.IsActive
            }).ToListAsync(ct);

    public async Task<AdminCouponVm?> GetCouponForEditAsync(int? id, CancellationToken ct = default)
    {
        if (id is null or 0) return new AdminCouponVm();
        return await _uow.Repository<Coupon>().Query()
            .Where(c => c.Id == id)
            .Select(c => new AdminCouponVm
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MinOrderAmount = c.MinOrderAmount,
                MaxDiscountAmount = c.MaxDiscountAmount,
                StartsAtUtc = c.StartsAtUtc,
                EndsAtUtc = c.EndsAtUtc,
                UsageLimit = c.UsageLimit,
                TimesUsed = c.TimesUsed,
                IsActive = c.IsActive
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<AdminResult> SaveCouponAsync(AdminCouponVm vm, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Coupon>();
        var code = vm.Code.Trim().ToUpperInvariant();
        if (await repo.Query().AnyAsync(c => c.Code == code && c.Id != vm.Id, ct))
            return AdminResult.Fail($"Coupon code '{code}' already exists.");
        if (vm.EndsAtUtc < vm.StartsAtUtc)
            return AdminResult.Fail("End date must be after the start date.");

        if (vm.Id == 0)
        {
            var c = new Coupon { CreatedAtUtc = DateTime.UtcNow };
            ApplyCoupon(c, vm, code);
            await repo.AddAsync(c, ct);
            await _uow.SaveChangesAsync(ct);
            return AdminResult.Ok(c.Id);
        }

        var existing = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(c => c.Id == vm.Id, ct);
        if (existing is null) return AdminResult.Fail("Coupon not found.");
        ApplyCoupon(existing, vm, code);
        repo.Update(existing);
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(existing.Id);

        static void ApplyCoupon(Coupon c, AdminCouponVm vm, string code)
        {
            c.Code = code;
            c.Description = vm.Description;
            c.DiscountType = vm.DiscountType;
            c.DiscountValue = vm.DiscountValue;
            c.MinOrderAmount = vm.MinOrderAmount;
            c.MaxDiscountAmount = vm.MaxDiscountAmount;
            c.StartsAtUtc = DateTime.SpecifyKind(vm.StartsAtUtc, DateTimeKind.Utc);
            c.EndsAtUtc = DateTime.SpecifyKind(vm.EndsAtUtc.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);
            c.UsageLimit = vm.UsageLimit;
            c.IsActive = vm.IsActive;
        }
    }

    // ---------------- Reviews ----------------
    public async Task<IReadOnlyList<AdminReviewVm>> GetReviewsAsync(bool? approved, CancellationToken ct = default)
    {
        var q = _uow.Repository<Review>().Query();
        if (approved is not null) q = q.Where(r => r.IsApproved == approved);

        return await q.OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new AdminReviewVm
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                CustomerName = r.Customer.FullName,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                IsApproved = r.IsApproved,
                CreatedAtUtc = r.CreatedAtUtc
            }).ToListAsync(ct);
    }

    public async Task<AdminResult> SetReviewApprovalAsync(int reviewId, bool approved, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Review>();
        var r = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == reviewId, ct);
        if (r is null) return AdminResult.Fail("Review not found.");
        r.IsApproved = approved;
        repo.Update(r);
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(reviewId);
    }

    public async Task<AdminResult> DeleteReviewAsync(int reviewId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Review>();
        var r = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == reviewId, ct);
        if (r is null) return AdminResult.Fail("Review not found.");
        repo.Remove(r);
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(reviewId);
    }

    // ---------------- helpers ----------------
    private async Task<IReadOnlyList<CategoryVm>> CategoryDropdownAsync(CancellationToken ct)
        => await _uow.Repository<Category>().Query()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryVm { Id = c.Id, Name = c.Name, Slug = c.Slug })
            .ToListAsync(ct);

    private static string Slugify(string input)
    {
        var s = input.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"[\s-]+", "-").Trim('-');
        return string.IsNullOrEmpty(s) ? Guid.NewGuid().ToString("N")[..8] : s;
    }
}

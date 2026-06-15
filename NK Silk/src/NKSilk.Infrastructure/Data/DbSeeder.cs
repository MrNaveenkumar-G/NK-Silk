using Microsoft.EntityFrameworkCore;
using NKSilk.Domain.Entities;

namespace NKSilk.Infrastructure.Data;

/// <summary>Seeds demo catalogue data on first run (idempotent — skips if products exist).</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var now = DateTime.UtcNow;
        await SeedCatalogAsync(db, now);
        await SeedCouponsAsync(db, now);
    }

    private static async Task SeedCatalogAsync(ApplicationDbContext db, DateTime now)
    {
        if (await db.Products.AnyAsync()) return;

        var sarees = new Category { Name = "Sarees", Slug = "sarees", DisplayOrder = 1, CreatedAtUtc = now, ImageUrl = "/img/cat-sarees.svg" };
        var mens   = new Category { Name = "Men's Wear", Slug = "mens-wear", DisplayOrder = 2, CreatedAtUtc = now, ImageUrl = "/img/cat-mens.svg" };
        var kids   = new Category { Name = "Kids", Slug = "kids", DisplayOrder = 3, CreatedAtUtc = now, ImageUrl = "/img/cat-kids.svg" };
        db.Categories.AddRange(sarees, mens, kids);

        var red    = new Color { Name = "Maroon", HexCode = "#800000", CreatedAtUtc = now };
        var gold   = new Color { Name = "Gold", HexCode = "#D4AF37", CreatedAtUtc = now };
        var blue   = new Color { Name = "Royal Blue", HexCode = "#1E3A8A", CreatedAtUtc = now };
        var white  = new Color { Name = "White", HexCode = "#FFFFFF", CreatedAtUtc = now };
        db.Colors.AddRange(red, gold, blue, white);

        var free = new Size { Name = "Free Size", DisplayOrder = 1, CreatedAtUtc = now };
        var m    = new Size { Name = "M", DisplayOrder = 2, CreatedAtUtc = now };
        var l    = new Size { Name = "L", DisplayOrder = 3, CreatedAtUtc = now };
        db.Sizes.AddRange(free, m, l);

        // Helper to build a product with one image and a couple of variants.
        Product Build(string name, string slug, Category cat, decimal price, decimal mrp,
            string fabric, string composition, int gsm, string wash, string occasion,
            bool featured, (Color color, Size size, int qty)[] variants)
        {
            var p = new Product
            {
                Name = name, Slug = slug, Category = cat,
                Sku = slug.ToUpperInvariant().Replace("-", ""),
                BasePrice = price, MrpPrice = mrp,
                FabricType = fabric, MaterialComposition = composition, Gsm = gsm,
                WashCare = wash, Occasion = occasion,
                ShortDescription = $"{fabric} • {occasion}",
                Description = $"Exquisite {name.ToLowerInvariant()} crafted from {composition.ToLowerInvariant()}. "
                            + $"Ideal for {occasion.ToLowerInvariant()} occasions. Wash care: {wash}.",
                IsFeatured = featured, IsActive = true, CreatedAtUtc = now
            };
            p.Images.Add(new ProductImage { Url = "/img/product-placeholder.svg", AltText = name, IsPrimary = true, DisplayOrder = 0, CreatedAtUtc = now });

            int n = 1;
            foreach (var (color, size, qty) in variants)
            {
                var v = new ProductVariant
                {
                    Sku = $"{p.Sku}-{n++}",
                    Price = price, MrpPrice = mrp, Color = color, Size = size,
                    IsActive = true, CreatedAtUtc = now
                };
                v.Inventory = new Inventory { QuantityOnHand = qty, ReorderLevel = 5, CreatedAtUtc = now };
                p.Variants.Add(v);
            }
            return p;
        }

        db.Products.AddRange(
            Build("Kanchipuram Pure Silk Saree", "kanchipuram-pure-silk-saree", sarees, 8499, 11999,
                "Silk", "100% Pure Mulberry Silk", 420, "Dry clean only", "Wedding", true,
                new[] { (red, free, 12), (gold, free, 8), (blue, free, 5) }),

            Build("Soft Cotton Handloom Saree", "soft-cotton-handloom-saree", sarees, 1899, 2799,
                "Cotton", "100% Handloom Cotton", 180, "Machine wash cold", "Casual", true,
                new[] { (blue, free, 20), (white, free, 15) }),

            Build("Banarasi Georgette Saree", "banarasi-georgette-saree", sarees, 4299, 6499,
                "Georgette", "Pure Georgette with Zari", 90, "Dry clean only", "Festive", true,
                new[] { (red, free, 10), (gold, free, 6) }),

            Build("Men's Cotton Dhoti with Angavastram", "mens-cotton-dhoti-angavastram", mens, 1299, 1799,
                "Cotton", "100% Combed Cotton", 160, "Machine wash", "Traditional", true,
                new[] { (white, free, 30) }),

            Build("Men's Silk Kurta", "mens-silk-kurta", mens, 2499, 3499,
                "Silk", "Art Silk Blend", 150, "Dry clean", "Festive", false,
                new[] { (gold, m, 14), (gold, l, 11), (blue, m, 9) }),

            Build("Kids Pattu Pavadai Set", "kids-pattu-pavadai-set", kids, 1599, 2299,
                "Silk", "Art Silk", 140, "Hand wash", "Festive", true,
                new[] { (red, m, 10), (gold, l, 7) })
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedCouponsAsync(ApplicationDbContext db, DateTime now)
    {
        if (await db.Coupons.AnyAsync()) return;

        db.Coupons.AddRange(
            new Coupon
            {
                Code = "FESTIVE10", Description = "10% off festive collection",
                DiscountType = Domain.Enums.DiscountType.Percentage, DiscountValue = 10,
                MaxDiscountAmount = 1000, MinOrderAmount = 1000,
                StartsAtUtc = now.AddDays(-1), EndsAtUtc = now.AddMonths(3),
                IsActive = true, CreatedAtUtc = now
            },
            new Coupon
            {
                Code = "FLAT200", Description = "Flat ₹200 off orders above ₹2000",
                DiscountType = Domain.Enums.DiscountType.FlatAmount, DiscountValue = 200,
                MinOrderAmount = 2000,
                StartsAtUtc = now.AddDays(-1), EndsAtUtc = now.AddMonths(3),
                IsActive = true, CreatedAtUtc = now
            }
        );

        await db.SaveChangesAsync();
    }
}

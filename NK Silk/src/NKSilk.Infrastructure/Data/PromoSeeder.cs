using Microsoft.EntityFrameworkCore;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Infrastructure.Data;

/// <summary>Seeds a demo offer and combo pack so the promotions UI has data in development.</summary>
public static class PromoSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var now = DateTime.UtcNow;

        // ---- Demo offer: 15% off the Sarees category ----
        if (!await db.Offers.AnyAsync(o => o.Slug == "festive-saree-sale"))
        {
            var sarees = await db.Categories.FirstOrDefaultAsync(c => c.Slug == "sarees");
            db.Offers.Add(new Offer
            {
                Title = "Festive Saree Sale",
                Slug = "festive-saree-sale",
                Description = "Celebrate the season with 15% off all sarees.",
                OfferType = OfferType.PercentageOff,
                Value = 15m,
                Scope = sarees is not null ? OfferScope.Category : OfferScope.EntireStore,
                CategoryId = sarees?.Id,
                StartsAtUtc = now.AddDays(-1),
                EndsAtUtc = now.AddDays(20),
                Priority = 10,
                IsActive = true,
                CreatedAtUtc = now
            });
            await db.SaveChangesAsync();
        }

        // ---- Demo combo: two products bundled below their combined price ----
        if (!await db.ComboPacks.AnyAsync(c => c.Slug == "festive-family-combo"))
        {
            var products = await db.Products
                .OrderBy(p => p.Id)
                .Take(2)
                .Select(p => new { p.Id, p.BasePrice })
                .ToListAsync();

            if (products.Count == 2)
            {
                var regular = products.Sum(p => p.BasePrice);
                var combo = new ComboPack
                {
                    Name = "Festive Family Combo",
                    Slug = "festive-family-combo",
                    Description = "Two festive favourites bundled together at a special price.",
                    ComboPrice = Math.Round(regular * 0.85m, 0),  // ~15% bundle saving
                    IsActive = true,
                    CreatedAtUtc = now
                };
                foreach (var p in products)
                    combo.Items.Add(new ComboPackItem { ProductId = p.Id, Quantity = 1, CreatedAtUtc = now });
                db.ComboPacks.Add(combo);
                await db.SaveChangesAsync();
            }
        }
    }
}

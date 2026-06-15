using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Services;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;
using NKSilk.Infrastructure.Data;
using NKSilk.Infrastructure.Repositories;
using Xunit;

namespace NKSilk.Tests;

public class PromotionServiceTests
{
    private static ApplicationDbContext NewContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static PromotionService NewService(ApplicationDbContext db) => new(new UnitOfWork(db));

    [Fact]
    public async Task Category_offer_discounts_matching_product()
    {
        await using var db = NewContext();
        db.Categories.Add(new Category { Id = 1, Name = "Sarees", Slug = "sarees" });
        db.Offers.Add(new Offer
        {
            Title = "15% off sarees", Slug = "o1", OfferType = OfferType.PercentageOff, Value = 15m,
            Scope = OfferScope.Category, CategoryId = 1, IsActive = true,
            StartsAtUtc = DateTime.UtcNow.AddDays(-1), EndsAtUtc = DateTime.UtcNow.AddDays(1)
        });
        await db.SaveChangesAsync();

        var result = await NewService(db).EvaluateAsync(new[]
        {
            new PromotionLineVm { ProductId = 10, CategoryId = 1, UnitPrice = 1000m, Quantity = 2 }
        });

        Assert.Equal(850m, result.EffectiveUnitPrices[0]);
        Assert.Equal(300m, result.OfferSavings); // 150 per unit × 2
        Assert.Equal("15% off sarees", result.LineOfferTitles[0]);
    }

    [Fact]
    public async Task No_offer_leaves_price_unchanged()
    {
        await using var db = NewContext();
        var result = await NewService(db).EvaluateAsync(new[]
        {
            new PromotionLineVm { ProductId = 5, CategoryId = 2, UnitPrice = 999m, Quantity = 1 }
        });

        Assert.Equal(999m, result.EffectiveUnitPrices[0]);
        Assert.Equal(0m, result.OfferSavings);
        Assert.Null(result.LineOfferTitles[0]);
    }

    [Fact]
    public async Task Combo_saving_applies_when_all_components_present()
    {
        await using var db = NewContext();
        var combo = new ComboPack { Name = "Pair", Slug = "pair", ComboPrice = 1500m, IsActive = true };
        combo.Items.Add(new ComboPackItem { ProductId = 10, Quantity = 1 });
        combo.Items.Add(new ComboPackItem { ProductId = 11, Quantity = 1 });
        db.ComboPacks.Add(combo);
        await db.SaveChangesAsync();

        var lines = new[]
        {
            new PromotionLineVm { ProductId = 10, CategoryId = 1, UnitPrice = 1000m, Quantity = 1 },
            new PromotionLineVm { ProductId = 11, CategoryId = 1, UnitPrice = 1000m, Quantity = 1 }
        };

        var result = await NewService(db).EvaluateAsync(lines);

        Assert.Equal(500m, result.ComboSavings); // 2000 regular − 1500 combo
        Assert.Contains("Pair", result.AppliedCombos);
    }

    [Fact]
    public async Task Combo_saving_skipped_when_a_component_is_missing()
    {
        await using var db = NewContext();
        var combo = new ComboPack { Name = "Pair", Slug = "pair", ComboPrice = 1500m, IsActive = true };
        combo.Items.Add(new ComboPackItem { ProductId = 10, Quantity = 1 });
        combo.Items.Add(new ComboPackItem { ProductId = 11, Quantity = 1 });
        db.ComboPacks.Add(combo);
        await db.SaveChangesAsync();

        var result = await NewService(db).EvaluateAsync(new[]
        {
            new PromotionLineVm { ProductId = 10, CategoryId = 1, UnitPrice = 1000m, Quantity = 1 }
        });

        Assert.Equal(0m, result.ComboSavings);
        Assert.Empty(result.AppliedCombos);
    }
}

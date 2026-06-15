/* ============================================================================
   NK Silk - Seed Data : Offers
   Source: src/NKSilk.Infrastructure/Data/PromoSeeder.cs
   Demo offer: 15% off the Sarees category.
     OfferType 0 = PercentageOff ; Scope 1 = Category (falls back to 0 = EntireStore
     if the Sarees category is missing). Window: starts yesterday, ends +20 days.
   Idempotent - keyed on Slug. Run AFTER 01_Categories.sql.
   ============================================================================ */
SET NOCOUNT ON;
DECLARE @now DATETIME2 = SYSUTCDATETIME();
DECLARE @sarees INT = (SELECT Id FROM dbo.Categories WHERE Slug = N'sarees');

INSERT dbo.Offers (Title, Slug, Description, BannerImageUrl, OfferType, Value, Scope,
                   CategoryId, ProductId, Priority, StartsAtUtc, EndsAtUtc, IsActive, IsDeleted, CreatedAtUtc)
SELECT N'Festive Saree Sale', N'festive-saree-sale',
       N'Celebrate the season with 15% off all sarees.', NULL,
       0,                                  -- PercentageOff
       CAST(15 AS DECIMAL(18,2)),
       CASE WHEN @sarees IS NOT NULL THEN 1 ELSE 0 END,  -- Category else EntireStore
       @sarees, NULL, 10,
       DATEADD(DAY, -1, @now), DATEADD(DAY, 20, @now), 1, 0, @now
WHERE NOT EXISTS (SELECT 1 FROM dbo.Offers o WHERE o.Slug = N'festive-saree-sale');

PRINT 'Seed: Offers done.';
GO

/* ============================================================================
   NK Silk - Seed Data : ComboPacks (+ ComboPackItems)
   Source: src/NKSilk.Infrastructure/Data/PromoSeeder.cs
   Bundles the two lowest-Id products at ~15% off their combined BasePrice:
       ComboPrice = ROUND(SUM(BasePrice) * 0.85, 0)
   Idempotent - keyed on Slug. Run AFTER 09_Products.sql.
   ============================================================================ */
SET NOCOUNT ON;
DECLARE @now DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM dbo.ComboPacks WHERE Slug = N'festive-family-combo')
BEGIN
    DECLARE @p1 INT, @p2 INT, @regular DECIMAL(18,2);

    SELECT TOP (2) Id, BasePrice
    INTO #pick
    FROM dbo.Products
    WHERE IsDeleted = 0
    ORDER BY Id ASC;

    IF (SELECT COUNT(*) FROM #pick) = 2
    BEGIN
        SELECT @regular = SUM(BasePrice) FROM #pick;

        DECLARE @comboId INT;
        INSERT dbo.ComboPacks (Name, Slug, Description, ImageUrl, ComboPrice, IsActive, IsDeleted, CreatedAtUtc)
        VALUES (N'Festive Family Combo', N'festive-family-combo',
                N'Two festive favourites bundled together at a special price.', NULL,
                ROUND(@regular * 0.85, 0), 1, 0, @now);
        SET @comboId = SCOPE_IDENTITY();

        INSERT dbo.ComboPackItems (ComboPackId, ProductId, Quantity, IsDeleted, CreatedAtUtc)
        SELECT @comboId, Id, 1, 0, @now FROM #pick;

        PRINT 'Seed: ComboPacks done.';
    END
    ELSE
        PRINT 'Seed: ComboPacks skipped (need at least 2 products).';

    DROP TABLE #pick;
END
ELSE
    PRINT 'Seed: ComboPacks skipped (already present).';
GO

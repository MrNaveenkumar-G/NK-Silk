/* ============================================================================
   NK Silk - Seed Data : Coupons
   Source: src/NKSilk.Infrastructure/Data/DbSeeder.cs (SeedCouponsAsync)
   DiscountType: 0 = Percentage, 1 = FlatAmount.
   Validity window mirrors the seeder: starts yesterday, ends +3 months.
   Idempotent - keyed on Code.
   ============================================================================ */
SET NOCOUNT ON;
DECLARE @now DATETIME2 = SYSUTCDATETIME();

INSERT dbo.Coupons (Code, Description, DiscountType, DiscountValue, MinOrderAmount, MaxDiscountAmount,
                    StartsAtUtc, EndsAtUtc, UsageLimit, TimesUsed, IsActive, IsDeleted, CreatedAtUtc)
SELECT v.Code, v.Description, v.DiscountType, v.DiscountValue, v.MinOrderAmount, v.MaxDiscountAmount,
       DATEADD(DAY, -1, @now), DATEADD(MONTH, 3, @now), NULL, 0, 1, 0, @now
FROM (VALUES
        (N'FESTIVE10', N'10% off festive collection',          0, CAST(10  AS DECIMAL(18,2)), CAST(1000 AS DECIMAL(18,2)), CAST(1000 AS DECIMAL(18,2))),
        (N'FLAT200',   N'Flat ₹200 off orders above ₹2000',    1, CAST(200 AS DECIMAL(18,2)), CAST(2000 AS DECIMAL(18,2)), CAST(NULL AS DECIMAL(18,2)))
     ) AS v(Code, Description, DiscountType, DiscountValue, MinOrderAmount, MaxDiscountAmount)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Coupons c WHERE c.Code = v.Code);

PRINT 'Seed: Coupons done.';
GO

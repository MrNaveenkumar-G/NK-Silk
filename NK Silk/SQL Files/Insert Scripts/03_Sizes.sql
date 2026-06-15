/* ============================================================================
   NK Silk - Seed Data : Sizes
   Source: src/NKSilk.Infrastructure/Data/DbSeeder.cs (SeedCatalogAsync)
   Idempotent - keyed on Name.
   ============================================================================ */
SET NOCOUNT ON;

INSERT dbo.Sizes (Name, DisplayOrder, IsDeleted, CreatedAtUtc)
SELECT v.Name, v.DisplayOrder, 0, SYSUTCDATETIME()
FROM (VALUES
        (N'Free Size', 1),
        (N'M',         2),
        (N'L',         3)
     ) AS v(Name, DisplayOrder)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Sizes s WHERE s.Name = v.Name);

PRINT 'Seed: Sizes done.';
GO

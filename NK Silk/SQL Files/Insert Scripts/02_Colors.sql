/* ============================================================================
   NK Silk - Seed Data : Colors
   Source: src/NKSilk.Infrastructure/Data/DbSeeder.cs (SeedCatalogAsync)
   Idempotent - keyed on Name.
   ============================================================================ */
SET NOCOUNT ON;

INSERT dbo.Colors (Name, HexCode, IsDeleted, CreatedAtUtc)
SELECT v.Name, v.HexCode, 0, SYSUTCDATETIME()
FROM (VALUES
        (N'Maroon',     N'#800000'),
        (N'Gold',       N'#D4AF37'),
        (N'Royal Blue', N'#1E3A8A'),
        (N'White',      N'#FFFFFF')
     ) AS v(Name, HexCode)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Colors c WHERE c.Name = v.Name);

PRINT 'Seed: Colors done.';
GO

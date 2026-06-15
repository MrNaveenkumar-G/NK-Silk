/* ============================================================================
   NK Silk - Seed Data : Categories
   Source: src/NKSilk.Infrastructure/Data/DbSeeder.cs (SeedCatalogAsync)
   Idempotent - inserts each category only if its Slug is not already present.
   ============================================================================ */
SET NOCOUNT ON;

INSERT dbo.Categories (Name, Slug, DisplayOrder, ImageUrl, IsActive, IsDeleted, CreatedAtUtc)
SELECT v.Name, v.Slug, v.DisplayOrder, v.ImageUrl, 1, 0, SYSUTCDATETIME()
FROM (VALUES
        (N'Sarees',     N'sarees',    1, N'/img/cat-sarees.svg'),
        (N'Men''s Wear', N'mens-wear', 2, N'/img/cat-mens.svg'),
        (N'Kids',       N'kids',      3, N'/img/cat-kids.svg')
     ) AS v(Name, Slug, DisplayOrder, ImageUrl)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Categories c WHERE c.Slug = v.Slug);

PRINT 'Seed: Categories done.';
GO

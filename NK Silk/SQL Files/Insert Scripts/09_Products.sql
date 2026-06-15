/* ============================================================================
   NK Silk - Seed Data : Products (+ ProductImages, ProductVariants, Inventories)
   Source: src/NKSilk.Infrastructure/Data/DbSeeder.cs (SeedCatalogAsync / Build)
   ----------------------------------------------------------------------------
   Reproduces the 6 demo products with their placeholder image, variants
   (color/size) and per-variant inventory. Derived values match the C# helper:
     - Sku           = UPPER(slug) with dashes removed
     - VariantSku    = <ProductSku>-<n>
     - ShortDesc     = "<Fabric> • <Occasion>"
     - Description   = "Exquisite <name lower> crafted from <composition lower>. "
                       "Ideal for <occasion lower> occasions. Wash care: <wash>."
     - Inventory.ReorderLevel = 5
   Guarded like the seeder: the whole block runs only if Products is empty.
   Requires 01_Categories, 02_Colors, 03_Sizes to have run first.
   ============================================================================ */
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    PRINT 'Seed: Products skipped (table already populated).';
    RETURN;
END

DECLARE @now DATETIME2 = SYSUTCDATETIME();

/* --- resolve lookup ids ---------------------------------------------------- */
DECLARE @catSarees INT = (SELECT Id FROM dbo.Categories WHERE Slug = N'sarees');
DECLARE @catMens   INT = (SELECT Id FROM dbo.Categories WHERE Slug = N'mens-wear');
DECLARE @catKids   INT = (SELECT Id FROM dbo.Categories WHERE Slug = N'kids');

DECLARE @cMaroon INT = (SELECT Id FROM dbo.Colors WHERE Name = N'Maroon');
DECLARE @cGold   INT = (SELECT Id FROM dbo.Colors WHERE Name = N'Gold');
DECLARE @cBlue   INT = (SELECT Id FROM dbo.Colors WHERE Name = N'Royal Blue');
DECLARE @cWhite  INT = (SELECT Id FROM dbo.Colors WHERE Name = N'White');

DECLARE @sFree INT = (SELECT Id FROM dbo.Sizes WHERE Name = N'Free Size');
DECLARE @sM    INT = (SELECT Id FROM dbo.Sizes WHERE Name = N'M');
DECLARE @sL    INT = (SELECT Id FROM dbo.Sizes WHERE Name = N'L');

DECLARE @pid INT, @vid INT;

/* ============================ Product 1 =================================== */
INSERT dbo.Products (Name, Slug, Sku, ShortDescription, Description, BasePrice, MrpPrice,
                     CategoryId, FabricType, Occasion, MaterialComposition, WashCare, Gsm,
                     IsActive, IsFeatured, IsDeleted, CreatedAtUtc)
VALUES (N'Kanchipuram Pure Silk Saree', N'kanchipuram-pure-silk-saree', N'KANCHIPURAMPURESILKSAREE',
        N'Silk • Wedding',
        N'Exquisite kanchipuram pure silk saree crafted from 100% pure mulberry silk. Ideal for wedding occasions. Wash care: Dry clean only.',
        8499, 11999, @catSarees, N'Silk', N'Wedding', N'100% Pure Mulberry Silk', N'Dry clean only', 420,
        1, 1, 0, @now);
SET @pid = SCOPE_IDENTITY();
INSERT dbo.ProductImages (ProductId, Url, AltText, DisplayOrder, IsPrimary, IsDeleted, CreatedAtUtc)
VALUES (@pid, N'/img/product-placeholder.svg', N'Kanchipuram Pure Silk Saree', 0, 1, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cMaroon, @sFree, N'KANCHIPURAMPURESILKSAREE-1', 8499, 11999, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 12, 0, 5, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cGold, @sFree, N'KANCHIPURAMPURESILKSAREE-2', 8499, 11999, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 8, 0, 5, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cBlue, @sFree, N'KANCHIPURAMPURESILKSAREE-3', 8499, 11999, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 5, 0, 5, 0, @now);

/* ============================ Product 2 =================================== */
INSERT dbo.Products (Name, Slug, Sku, ShortDescription, Description, BasePrice, MrpPrice,
                     CategoryId, FabricType, Occasion, MaterialComposition, WashCare, Gsm,
                     IsActive, IsFeatured, IsDeleted, CreatedAtUtc)
VALUES (N'Soft Cotton Handloom Saree', N'soft-cotton-handloom-saree', N'SOFTCOTTONHANDLOOMSAREE',
        N'Cotton • Casual',
        N'Exquisite soft cotton handloom saree crafted from 100% handloom cotton. Ideal for casual occasions. Wash care: Machine wash cold.',
        1899, 2799, @catSarees, N'Cotton', N'Casual', N'100% Handloom Cotton', N'Machine wash cold', 180,
        1, 1, 0, @now);
SET @pid = SCOPE_IDENTITY();
INSERT dbo.ProductImages (ProductId, Url, AltText, DisplayOrder, IsPrimary, IsDeleted, CreatedAtUtc)
VALUES (@pid, N'/img/product-placeholder.svg', N'Soft Cotton Handloom Saree', 0, 1, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cBlue, @sFree, N'SOFTCOTTONHANDLOOMSAREE-1', 1899, 2799, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 20, 0, 5, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cWhite, @sFree, N'SOFTCOTTONHANDLOOMSAREE-2', 1899, 2799, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 15, 0, 5, 0, @now);

/* ============================ Product 3 =================================== */
INSERT dbo.Products (Name, Slug, Sku, ShortDescription, Description, BasePrice, MrpPrice,
                     CategoryId, FabricType, Occasion, MaterialComposition, WashCare, Gsm,
                     IsActive, IsFeatured, IsDeleted, CreatedAtUtc)
VALUES (N'Banarasi Georgette Saree', N'banarasi-georgette-saree', N'BANARASIGEORGETTESAREE',
        N'Georgette • Festive',
        N'Exquisite banarasi georgette saree crafted from pure georgette with zari. Ideal for festive occasions. Wash care: Dry clean only.',
        4299, 6499, @catSarees, N'Georgette', N'Festive', N'Pure Georgette with Zari', N'Dry clean only', 90,
        1, 1, 0, @now);
SET @pid = SCOPE_IDENTITY();
INSERT dbo.ProductImages (ProductId, Url, AltText, DisplayOrder, IsPrimary, IsDeleted, CreatedAtUtc)
VALUES (@pid, N'/img/product-placeholder.svg', N'Banarasi Georgette Saree', 0, 1, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cMaroon, @sFree, N'BANARASIGEORGETTESAREE-1', 4299, 6499, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 10, 0, 5, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cGold, @sFree, N'BANARASIGEORGETTESAREE-2', 4299, 6499, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 6, 0, 5, 0, @now);

/* ============================ Product 4 =================================== */
INSERT dbo.Products (Name, Slug, Sku, ShortDescription, Description, BasePrice, MrpPrice,
                     CategoryId, FabricType, Occasion, MaterialComposition, WashCare, Gsm,
                     IsActive, IsFeatured, IsDeleted, CreatedAtUtc)
VALUES (N'Men''s Cotton Dhoti with Angavastram', N'mens-cotton-dhoti-angavastram', N'MENSCOTTONDHOTIANGAVASTRAM',
        N'Cotton • Traditional',
        N'Exquisite men''s cotton dhoti with angavastram crafted from 100% combed cotton. Ideal for traditional occasions. Wash care: Machine wash.',
        1299, 1799, @catMens, N'Cotton', N'Traditional', N'100% Combed Cotton', N'Machine wash', 160,
        1, 1, 0, @now);
SET @pid = SCOPE_IDENTITY();
INSERT dbo.ProductImages (ProductId, Url, AltText, DisplayOrder, IsPrimary, IsDeleted, CreatedAtUtc)
VALUES (@pid, N'/img/product-placeholder.svg', N'Men''s Cotton Dhoti with Angavastram', 0, 1, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cWhite, @sFree, N'MENSCOTTONDHOTIANGAVASTRAM-1', 1299, 1799, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 30, 0, 5, 0, @now);

/* ============================ Product 5 =================================== */
INSERT dbo.Products (Name, Slug, Sku, ShortDescription, Description, BasePrice, MrpPrice,
                     CategoryId, FabricType, Occasion, MaterialComposition, WashCare, Gsm,
                     IsActive, IsFeatured, IsDeleted, CreatedAtUtc)
VALUES (N'Men''s Silk Kurta', N'mens-silk-kurta', N'MENSSILKKURTA',
        N'Silk • Festive',
        N'Exquisite men''s silk kurta crafted from art silk blend. Ideal for festive occasions. Wash care: Dry clean.',
        2499, 3499, @catMens, N'Silk', N'Festive', N'Art Silk Blend', N'Dry clean', 150,
        1, 0, 0, @now);
SET @pid = SCOPE_IDENTITY();
INSERT dbo.ProductImages (ProductId, Url, AltText, DisplayOrder, IsPrimary, IsDeleted, CreatedAtUtc)
VALUES (@pid, N'/img/product-placeholder.svg', N'Men''s Silk Kurta', 0, 1, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cGold, @sM, N'MENSSILKKURTA-1', 2499, 3499, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 14, 0, 5, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cGold, @sL, N'MENSSILKKURTA-2', 2499, 3499, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 11, 0, 5, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cBlue, @sM, N'MENSSILKKURTA-3', 2499, 3499, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 9, 0, 5, 0, @now);

/* ============================ Product 6 =================================== */
INSERT dbo.Products (Name, Slug, Sku, ShortDescription, Description, BasePrice, MrpPrice,
                     CategoryId, FabricType, Occasion, MaterialComposition, WashCare, Gsm,
                     IsActive, IsFeatured, IsDeleted, CreatedAtUtc)
VALUES (N'Kids Pattu Pavadai Set', N'kids-pattu-pavadai-set', N'KIDSPATTUPAVADAISET',
        N'Silk • Festive',
        N'Exquisite kids pattu pavadai set crafted from art silk. Ideal for festive occasions. Wash care: Hand wash.',
        1599, 2299, @catKids, N'Silk', N'Festive', N'Art Silk', N'Hand wash', 140,
        1, 1, 0, @now);
SET @pid = SCOPE_IDENTITY();
INSERT dbo.ProductImages (ProductId, Url, AltText, DisplayOrder, IsPrimary, IsDeleted, CreatedAtUtc)
VALUES (@pid, N'/img/product-placeholder.svg', N'Kids Pattu Pavadai Set', 0, 1, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cMaroon, @sM, N'KIDSPATTUPAVADAISET-1', 1599, 2299, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 10, 0, 5, 0, @now);

INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
VALUES (@pid, @cGold, @sL, N'KIDSPATTUPAVADAISET-2', 1599, 2299, 1, 0, @now);
SET @vid = SCOPE_IDENTITY();
INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
VALUES (@vid, 7, 0, 5, 0, @now);

PRINT 'Seed: Products / Images / Variants / Inventories done (6 products).';
GO

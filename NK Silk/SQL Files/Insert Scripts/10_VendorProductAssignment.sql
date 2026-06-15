/* ============================================================================
   NK Silk - Seed Data : assign demo products to the demo vendor
   Source: src/NKSilk.Infrastructure/Data/VendorSeeder.cs
   Tags the two newest house products (VendorId IS NULL), ordered by Id DESC,
   to the Heritage Weaves vendor - exactly as the seeder does.
   Run AFTER 06_Vendors.sql and 09_Products.sql.
   ============================================================================ */
SET NOCOUNT ON;

DECLARE @vendorId INT = (SELECT Id FROM dbo.Vendors WHERE Slug = N'heritage-weaves');

IF @vendorId IS NOT NULL
BEGIN
    UPDATE p
       SET p.VendorId = @vendorId,
           p.UpdatedAtUtc = SYSUTCDATETIME()
    FROM dbo.Products p
    JOIN (
        SELECT TOP (2) Id
        FROM dbo.Products
        WHERE VendorId IS NULL AND IsDeleted = 0
        ORDER BY Id DESC
    ) AS pick ON pick.Id = p.Id;

    PRINT 'Seed: Vendor product assignment done.';
END
ELSE
    PRINT 'Seed: Vendor not found - run 06_Vendors.sql first.';
GO

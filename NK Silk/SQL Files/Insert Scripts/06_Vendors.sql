/* ============================================================================
   NK Silk - Seed Data : Vendors
   Source: src/NKSilk.Infrastructure/Data/VendorSeeder.cs
   The demo marketplace vendor. Idempotent - keyed on Slug.
   ============================================================================ */
SET NOCOUNT ON;

INSERT dbo.Vendors (Name, Slug, ContactEmail, PhoneNumber, CommissionRate, IsActive, IsDeleted, CreatedAtUtc)
SELECT N'Heritage Weaves', N'heritage-weaves', N'seller@nksilk.com', N'+91 90000 00000',
       CAST(12.5 AS DECIMAL(5,2)), 1, 0, SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM dbo.Vendors v WHERE v.Slug = N'heritage-weaves');

PRINT 'Seed: Vendors done.';
GO

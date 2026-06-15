/* ============================================================================
   NK Silk - Seed Data : Customers (default Admin + demo Vendor seller)
   Source: src/NKSilk.Infrastructure/Data/AdminSeeder.cs + VendorSeeder.cs
   Idempotent - keyed on Email.

   !! PASSWORD HASH WARNING !!
   The app stores PasswordHash using ASP.NET Core Identity's PasswordHasher
   (PBKDF2 with a random per-hash salt). That value is NOT reproducible as a
   static SQL literal, so these rows are seeded with PasswordHash = NULL.
   To enable login for these seed accounts either:
     (a) let the application's AdminSeeder / VendorSeeder create them (preferred), or
     (b) use the app's "forgot password" / set-password flow after inserting.
   Intended dev credentials (set by the app, shown here for reference only):
     admin@nksilk.com  / Admin@123
     seller@nksilk.com / Seller@123
   ============================================================================ */
SET NOCOUNT ON;

-- Default administrator -------------------------------------------------------
INSERT dbo.Customers (FullName, Email, PhoneNumber, PasswordHash, IsActive, IsAdmin, IsVendor,
                      VendorId, IsEmailVerified, IsDeleted, CreatedAtUtc)
SELECT N'NK Silk Admin', N'admin@nksilk.com', NULL, NULL /* set via app */, 1, 1, 0,
       NULL, 1, 0, SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM dbo.Customers c WHERE c.Email = N'admin@nksilk.com');

-- Demo vendor seller (linked to the Heritage Weaves vendor) -------------------
INSERT dbo.Customers (FullName, Email, PhoneNumber, PasswordHash, IsActive, IsAdmin, IsVendor,
                      VendorId, IsEmailVerified, IsDeleted, CreatedAtUtc)
SELECT N'Heritage Weaves Seller', N'seller@nksilk.com', NULL, NULL /* set via app */, 1, 0, 1,
       (SELECT Id FROM dbo.Vendors WHERE Slug = N'heritage-weaves'), 1, 0, SYSUTCDATETIME()
WHERE NOT EXISTS (SELECT 1 FROM dbo.Customers c WHERE c.Email = N'seller@nksilk.com');

PRINT 'Seed: Customers (admin + seller) done.';
GO

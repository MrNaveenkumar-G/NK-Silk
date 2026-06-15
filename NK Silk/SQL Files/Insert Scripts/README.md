# NK Silk – Seed / Default Data Insert Scripts

These scripts insert the application's **default (demo) data** — the same data
the app's seeders create on first run. They are reconstructed from:

- `DbSeeder.cs`   → Categories, Colors, Sizes, Products (+ images, variants, inventory), Coupons
- `RoleSeeder.cs` → Roles, CustomerRole assignments
- `AdminSeeder.cs`→ default admin Customer
- `VendorSeeder.cs`→ demo Vendor, seller Customer, product→vendor tagging
- `PromoSeeder.cs`→ demo Offer and ComboPack

## Prerequisites

Create the schema first (from the parent `SQL Files` folder):

```
..\01_Tables.sql      -- required
..\02_Views.sql       -- optional
```

## Run order

Run `00_SeedAll.sql` in **SSMS with SQLCMD Mode enabled**
(`Query` → `SQLCMD Mode`), or run the numbered files individually in order:

| # | File | Inserts |
|---|------|---------|
| 01 | Categories | Sarees, Men's Wear, Kids |
| 02 | Colors | Maroon, Gold, Royal Blue, White |
| 03 | Sizes | Free Size, M, L |
| 04 | Roles | Admin, Vendor, Customer |
| 05 | Coupons | FESTIVE10 (10%), FLAT200 (₹200 flat) |
| 06 | Vendors | Heritage Weaves |
| 07 | Customers | admin@nksilk.com, seller@nksilk.com |
| 08 | CustomerRoles | admin→Admin, seller→Vendor |
| 09 | Products | 6 products + images + 13 variants + inventory |
| 10 | VendorProductAssignment | tags 2 newest products to the vendor |
| 11 | Offers | Festive Saree Sale (15% off Sarees) |
| 12 | ComboPacks | Festive Family Combo |

All scripts are **idempotent** (guarded by `NOT EXISTS` / empty-table checks),
so re-running them will not create duplicates.

## Important: passwords are NOT seeded

`Customers.PasswordHash` is produced by ASP.NET Core Identity (PBKDF2 with a
random salt) and cannot be expressed as a static SQL literal. `07_Customers.sql`
inserts the admin and seller with `PasswordHash = NULL`. To enable login:

- **Preferred:** let the application's `AdminSeeder` / `VendorSeeder` create the
  accounts (they hash the passwords correctly), or
- use the app's password-reset / set-password flow after seeding.

Reference dev credentials the app would set:
`admin@nksilk.com / Admin@123` and `seller@nksilk.com / Seller@123`.

## Notes

- Dates use `SYSUTCDATETIME()` with `DATEADD` offsets to mirror the seeders'
  relative windows (e.g. coupons valid from yesterday to +3 months; the offer
  ends +20 days).
- Product `Sku` follows the seeder rule `UPPER(slug)` with dashes removed;
  variant SKUs are `<ProductSku>-<n>`.
- `ComboPacks.ComboPrice` = `ROUND(SUM(BasePrice of 2 lowest-Id products) * 0.85, 0)`.
- Tables with no application default data (Orders, Payments, Shipments, Returns,
  SupportTickets, Notifications, Carts, Addresses, Reviews, Wishlists, AuditLogs,
  Brands) are intentionally **not** seeded — create those rows via the insert
  stored procedures in `..\03_StoredProcedures_Insert.sql`.
```

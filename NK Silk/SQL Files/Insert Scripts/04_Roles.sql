/* ============================================================================
   NK Silk - Seed Data : Roles
   Source: src/NKSilk.Infrastructure/Data/RoleSeeder.cs
   The three canonical RBAC roles. Idempotent - keyed on Name.
   ============================================================================ */
SET NOCOUNT ON;

INSERT dbo.Roles (Name, Description, IsDeleted, CreatedAtUtc)
SELECT v.Name, v.Description, 0, SYSUTCDATETIME()
FROM (VALUES
        (N'Admin',    N'Full back-office access'),
        (N'Vendor',   N'Marketplace seller portal'),
        (N'Customer', N'Registered shopper')
     ) AS v(Name, Description)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles r WHERE r.Name = v.Name);

PRINT 'Seed: Roles done.';
GO

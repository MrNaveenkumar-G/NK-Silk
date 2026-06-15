/* ============================================================================
   NK Silk - Seed Data : CustomerRoles
   Source: src/NKSilk.Infrastructure/Data/RoleSeeder.cs (back-fill logic)
   Assigns RBAC roles to the seeded accounts based on their IsAdmin/IsVendor
   flags. Idempotent - keyed on (CustomerId, RoleId).
   Run AFTER 04_Roles.sql and 07_Customers.sql.
   ============================================================================ */
SET NOCOUNT ON;

;WITH Want AS (
    SELECT c.Id AS CustomerId, r.Id AS RoleId
    FROM dbo.Customers c
    JOIN dbo.Roles r
      ON r.Name = CASE
                     WHEN c.IsAdmin  = 1 THEN N'Admin'
                     WHEN c.IsVendor = 1 THEN N'Vendor'
                     ELSE N'Customer'
                  END
)
INSERT dbo.CustomerRoles (CustomerId, RoleId, IsDeleted, CreatedAtUtc)
SELECT w.CustomerId, w.RoleId, 0, SYSUTCDATETIME()
FROM Want w
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.CustomerRoles cr
    WHERE cr.CustomerId = w.CustomerId AND cr.RoleId = w.RoleId AND cr.IsDeleted = 0
);

PRINT 'Seed: CustomerRoles done.';
GO

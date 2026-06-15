using Microsoft.EntityFrameworkCore;
using NKSilk.Domain.Entities;

namespace NKSilk.Infrastructure.Data;

/// <summary>
/// Seeds the canonical roles and back-fills CustomerRole assignments from the existing
/// IsAdmin/IsVendor flags so the RBAC tables and the convenience flags agree.
/// </summary>
public static class RoleSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var defaults = new (string Name, string Desc)[]
        {
            ("Admin", "Full back-office access"),
            ("Vendor", "Marketplace seller portal"),
            ("Customer", "Registered shopper")
        };

        foreach (var (name, desc) in defaults)
            if (!await db.Roles.AnyAsync(r => r.Name == name))
                db.Roles.Add(new Role { Name = name, Description = desc, CreatedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var roleByName = await db.Roles.ToDictionaryAsync(r => r.Name, r => r.Id);

        // Back-fill assignments for customers that don't yet have a role row.
        var customers = await db.Customers
            .Select(c => new { c.Id, c.IsAdmin, c.IsVendor })
            .ToListAsync();
        var existing = await db.CustomerRoles
            .Select(cr => new { cr.CustomerId, cr.RoleId })
            .ToListAsync();
        var have = existing.Select(e => (e.CustomerId, e.RoleId)).ToHashSet();

        void Ensure(int customerId, string roleName)
        {
            var rid = roleByName[roleName];
            if (have.Add((customerId, rid)))
                db.CustomerRoles.Add(new CustomerRole { CustomerId = customerId, RoleId = rid, CreatedAtUtc = DateTime.UtcNow });
        }

        foreach (var c in customers)
        {
            if (c.IsAdmin) Ensure(c.Id, "Admin");
            if (c.IsVendor) Ensure(c.Id, "Vendor");
            if (!c.IsAdmin && !c.IsVendor) Ensure(c.Id, "Customer");
        }
        await db.SaveChangesAsync();
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NKSilk.Domain.Entities;

namespace NKSilk.Infrastructure.Data;

/// <summary>Ensures a default admin account exists (dev convenience).</summary>
public static class AdminSeeder
{
    public const string AdminEmail = "admin@nksilk.com";
    public const string AdminPassword = "Admin@123";

    public static async Task SeedAsync(ApplicationDbContext db, IPasswordHasher<Customer> hasher)
    {
        if (await db.Customers.AnyAsync(c => c.Email == AdminEmail)) return;

        var admin = new Customer
        {
            FullName = "NK Silk Admin",
            Email = AdminEmail,
            IsAdmin = true,
            IsActive = true,
            IsEmailVerified = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, AdminPassword);

        db.Customers.Add(admin);
        await db.SaveChangesAsync();
    }
}

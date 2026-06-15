using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NKSilk.Domain.Entities;

namespace NKSilk.Infrastructure.Data;

/// <summary>
/// Seeds a demo marketplace vendor, a seller login, and assigns a couple of existing
/// products to the vendor so the /Vendor portal has data to show in development.
/// </summary>
public static class VendorSeeder
{
    public const string VendorEmail = "seller@nksilk.com";
    public const string VendorPassword = "Seller@123";

    public static async Task SeedAsync(ApplicationDbContext db, IPasswordHasher<Customer> hasher)
    {
        var vendor = await db.Vendors.FirstOrDefaultAsync(v => v.Slug == "heritage-weaves");
        if (vendor is null)
        {
            vendor = new Vendor
            {
                Name = "Heritage Weaves",
                Slug = "heritage-weaves",
                ContactEmail = VendorEmail,
                PhoneNumber = "+91 90000 00000",
                CommissionRate = 12.5m,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            db.Vendors.Add(vendor);
            await db.SaveChangesAsync();

            // Give the demo vendor some inventory to manage: tag the two newest house products.
            var houseProducts = await db.Products
                .Where(p => p.VendorId == null)
                .OrderByDescending(p => p.Id)
                .Take(2)
                .ToListAsync();
            foreach (var p in houseProducts) p.VendorId = vendor.Id;
            await db.SaveChangesAsync();
        }

        if (!await db.Customers.AnyAsync(c => c.Email == VendorEmail))
        {
            var seller = new Customer
            {
                FullName = "Heritage Weaves Seller",
                Email = VendorEmail,
                IsVendor = true,
                VendorId = vendor.Id,
                IsActive = true,
                IsEmailVerified = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            seller.PasswordHash = hasher.HashPassword(seller, VendorPassword);
            db.Customers.Add(seller);
            await db.SaveChangesAsync();
        }
    }
}

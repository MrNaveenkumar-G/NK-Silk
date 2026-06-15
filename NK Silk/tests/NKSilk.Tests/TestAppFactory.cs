using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NKSilk.Infrastructure.Data;

namespace NKSilk.Tests;

/// <summary>
/// Boots the real application for integration tests but swaps SQL Server for an in-memory
/// EF store, so the full pipeline (DI, auth, routing, seeders, services) runs without a database.
/// </summary>
public class TestAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Drop the SQL Server registration and any related provider services.
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(ApplicationDbContext) ||
                (d.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true &&
                 d.ServiceType.Name.Contains("DbContextOptions"))).ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase("nksilk-integration-tests"));
        });
    }
}

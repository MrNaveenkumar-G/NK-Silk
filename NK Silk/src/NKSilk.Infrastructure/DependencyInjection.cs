using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Common.Settings;
using NKSilk.Infrastructure.Data;
using NKSilk.Infrastructure.Notifications;
using NKSilk.Infrastructure.Payments;
using NKSilk.Infrastructure.Repositories;

namespace NKSilk.Infrastructure;

/// <summary>Registers EF Core, the DbContext and persistence services.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Payment gateway (Razorpay; simulation mode when no credentials are set).
        services.Configure<RazorpayOptions>(config.GetSection(RazorpayOptions.SectionName));
        services.AddHttpClient();
        services.AddScoped<RazorpayGateway>();
        services.AddScoped<IPaymentGateway>(sp => sp.GetRequiredService<RazorpayGateway>());
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

        // Notification transport (email/SMS). Logging simulator for dev; swap for a real
        // SMTP/SMS provider in production without touching the Application layer.
        services.AddScoped<INotificationSender, ConfigurableNotificationSender>();

        // CDN-aware media URL resolution (no-op until Cdn:BaseUrl is set).
        services.AddSingleton<IMediaUrlResolver, NKSilk.Infrastructure.Media.CdnMediaUrlResolver>();

        return services;
    }
}

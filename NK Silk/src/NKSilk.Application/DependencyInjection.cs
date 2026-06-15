using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NKSilk.Application.Services;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Domain.Entities;

namespace NKSilk.Application;

/// <summary>Registers Application-layer services (use cases) into the DI container.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IReturnService, ReturnService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ILogisticsService, LogisticsService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<ISupportService, SupportService>();
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<IOfferService, OfferService>();
        services.AddScoped<IComboService, ComboService>();
        services.AddScoped<IAccessService, AccessService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<ISearchService, SqlSearchService>();

        // Password hashing for customer credentials.
        services.AddScoped<IPasswordHasher<Customer>, PasswordHasher<Customer>>();
        return services;
    }
}

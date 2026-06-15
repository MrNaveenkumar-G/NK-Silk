using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface ICouponService
{
    /// <summary>Validates a coupon code against the order subtotal and returns the computed discount.</summary>
    Task<CouponValidation> ValidateAsync(string code, decimal subtotal, CancellationToken ct = default);
}

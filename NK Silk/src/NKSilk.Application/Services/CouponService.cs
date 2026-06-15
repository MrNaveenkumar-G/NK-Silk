using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services;

public class CouponService : ICouponService
{
    private readonly IUnitOfWork _uow;
    public CouponService(IUnitOfWork uow) => _uow = uow;

    public async Task<CouponValidation> ValidateAsync(string code, decimal subtotal, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return CouponValidation.Fail("Enter a coupon code.");

        var normalized = code.Trim().ToUpperInvariant();
        var coupon = await _uow.Repository<Coupon>()
            .FirstOrDefaultAsync(c => c.Code == normalized, ct);

        var now = DateTime.UtcNow;
        if (coupon is null || !coupon.IsActive)
            return CouponValidation.Fail("This coupon code is not valid.");
        if (now < coupon.StartsAtUtc || now > coupon.EndsAtUtc)
            return CouponValidation.Fail("This coupon has expired or is not yet active.");
        if (coupon.UsageLimit is int limit && coupon.TimesUsed >= limit)
            return CouponValidation.Fail("This coupon has reached its usage limit.");
        if (coupon.MinOrderAmount is decimal min && subtotal < min)
            return CouponValidation.Fail($"Add ₹{min - subtotal:N0} more to use this coupon (min order ₹{min:N0}).");

        var discount = coupon.DiscountType == DiscountType.Percentage
            ? subtotal * coupon.DiscountValue / 100m
            : coupon.DiscountValue;

        if (coupon.MaxDiscountAmount is decimal cap && discount > cap)
            discount = cap;
        if (discount > subtotal) discount = subtotal; // never below zero

        discount = Math.Round(discount, 2);
        return CouponValidation.Ok(coupon.Id, coupon.Code, discount);
    }
}

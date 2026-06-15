using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly ICartService _cart;
    private readonly ICouponService _coupons;
    private readonly INotificationService _notifications;
    private readonly IPromotionService _promotions;

    public OrderService(IUnitOfWork uow, ICartService cart, ICouponService coupons,
        INotificationService notifications, IPromotionService promotions)
    {
        _uow = uow;
        _cart = cart;
        _coupons = coupons;
        _notifications = notifications;
        _promotions = promotions;
    }

    public async Task<CheckoutVm> GetCheckoutAsync(string cartKey, CancellationToken ct = default)
        => new() { Cart = await _cart.GetCartAsync(cartKey, ct) };

    public async Task<OrderResult> PlaceOrderAsync(string cartKey, int customerId, PlaceOrderVm form, string? couponCode = null, CancellationToken ct = default)
    {
        // Load the cart tracked, with everything needed to snapshot lines and adjust stock.
        var cart = await _uow.Repository<Cart>().Query(asNoTracking: false)
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product)
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Color)
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Size)
            .Include(c => c.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Inventory)
            .FirstOrDefaultAsync(c => c.CartKey == cartKey, ct);

        if (cart is null || cart.Items.Count == 0)
            return OrderResult.Fail("Your cart is empty.");

        // Stock check up-front so we never partially fulfil.
        foreach (var item in cart.Items)
        {
            var available = item.ProductVariant.Inventory?.QuantityAvailable ?? 0;
            if (item.Quantity > available)
                return OrderResult.Fail($"'{item.ProductVariant.Product.Name}' has only {available} left in stock.");
        }

        var now = DateTime.UtcNow;
        var items = cart.Items.ToList();

        // Apply automatic offers + combo savings via the shared promotions engine.
        var promo = await _promotions.EvaluateAsync(items.Select(i => new PromotionLineVm
        {
            ProductId = i.ProductVariant.ProductId,
            CategoryId = i.ProductVariant.Product.CategoryId,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity
        }).ToList(), ct);

        var effective = new decimal[items.Count];
        for (var i = 0; i < items.Count; i++)
            effective[i] = i < promo.EffectiveUnitPrices.Count ? promo.EffectiveUnitPrices[i] : items[i].UnitPrice;

        var subTotal = 0m;
        for (var i = 0; i < items.Count; i++) subTotal += effective[i] * items[i].Quantity;
        var comboSavings = promo.ComboSavings;
        var shipping = (subTotal - comboSavings) >= 999m ? 0m : 49m;

        // Re-validate any applied coupon server-side (never trust a client-sent discount).
        decimal discount = comboSavings;
        Coupon? appliedCoupon = null;
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var validation = await _coupons.ValidateAsync(couponCode, subTotal - comboSavings, ct);
            if (validation.IsValid)
            {
                discount += validation.DiscountAmount;
                appliedCoupon = await _uow.Repository<Coupon>().Query(asNoTracking: false)
                    .FirstOrDefaultAsync(c => c.Id == validation.CouponId, ct);
            }
        }

        var address = new Address
        {
            CustomerId = customerId,
            ContactName = form.ContactName.Trim(),
            PhoneNumber = form.PhoneNumber.Trim(),
            Line1 = form.Line1.Trim(),
            Line2 = string.IsNullOrWhiteSpace(form.Line2) ? null : form.Line2.Trim(),
            City = form.City.Trim(),
            State = form.State.Trim(),
            PostalCode = form.PostalCode.Trim(),
            Country = "India",
            CreatedAtUtc = now
        };

        var order = new Order
        {
            OrderNumber = $"NK{now:yyyyMMddHHmmssfff}",
            Status = form.PaymentMethod == PaymentMethod.CashOnDelivery ? OrderStatus.Confirmed : OrderStatus.Pending,
            CustomerId = customerId,
            ShippingAddress = address,
            CouponId = appliedCoupon?.Id,
            SubTotal = subTotal,
            DiscountAmount = discount,
            ShippingFee = shipping,
            TaxAmount = 0m,
            GrandTotal = subTotal - discount + shipping,
            CreatedAtUtc = now
        };

        for (var idx = 0; idx < items.Count; idx++)
        {
            var item = items[idx];
            var v = item.ProductVariant;
            var unit = effective[idx];
            order.Items.Add(new OrderItem
            {
                ProductVariantId = v.Id,
                ProductName = v.Product.Name,
                VariantSku = v.Sku,
                ColorName = v.Color?.Name,
                SizeName = v.Size?.Name,
                Quantity = item.Quantity,
                UnitPrice = unit,
                LineTotal = unit * item.Quantity,
                CreatedAtUtc = now
            });

            // Deduct stock.
            if (v.Inventory is not null)
            {
                v.Inventory.QuantityOnHand -= item.Quantity;
                v.Inventory.UpdatedAtUtc = now;
            }
        }

        // Payment intent (COD is immediately "pending collection"; gateways are wired in Phase 2).
        order.Payment = new Payment
        {
            Method = form.PaymentMethod,
            Status = PaymentStatus.Pending,
            Amount = order.GrandTotal,
            Currency = "INR",
            CreatedAtUtc = now
        };

        if (appliedCoupon is not null)
        {
            appliedCoupon.TimesUsed += 1;
            _uow.Repository<Coupon>().Update(appliedCoupon);
        }

        await _uow.Repository<Order>().AddAsync(order, ct);

        // Empty the cart.
        var cartItemRepo = _uow.Repository<CartItem>();
        foreach (var item in cart.Items.ToList())
            cartItemRepo.Remove(item);

        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(customerId, NotificationType.OrderPlaced,
            "Order placed",
            $"Thanks! Your order {order.OrderNumber} for ₹{order.GrandTotal:N0} has been placed.",
            $"/Orders/Details/{order.OrderNumber}", ct);

        return OrderResult.Success(order.Id, order.OrderNumber);
    }

    public async Task<IReadOnlyList<OrderListItemVm>> GetOrdersForCustomerAsync(int customerId, CancellationToken ct = default)
    {
        return await _uow.Repository<Order>().Query()
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new OrderListItemVm
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                PlacedAtUtc = o.CreatedAtUtc,
                Status = o.Status,
                GrandTotal = o.GrandTotal,
                ItemCount = o.Items.Sum(i => i.Quantity)
            })
            .ToListAsync(ct);
    }

    public async Task<OrderDetailVm?> GetOrderForCustomerAsync(int customerId, string orderNumber, CancellationToken ct = default)
    {
        return await _uow.Repository<Order>().Query()
            .Where(o => o.CustomerId == customerId && o.OrderNumber == orderNumber)
            .Select(o => new OrderDetailVm
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                PlacedAtUtc = o.CreatedAtUtc,
                Status = o.Status,
                SubTotal = o.SubTotal,
                ShippingFee = o.ShippingFee,
                DiscountAmount = o.DiscountAmount,
                GrandTotal = o.GrandTotal,
                PaymentMethod = o.Payment != null ? o.Payment.Method : PaymentMethod.CashOnDelivery,
                PaymentStatus = o.Payment != null ? o.Payment.Status : PaymentStatus.Pending,
                ShipToName = o.ShippingAddress.ContactName,
                ShipToPhone = o.ShippingAddress.PhoneNumber,
                ShipToAddress = o.ShippingAddress.Line1
                    + (o.ShippingAddress.Line2 != null ? ", " + o.ShippingAddress.Line2 : "")
                    + ", " + o.ShippingAddress.City + ", " + o.ShippingAddress.State + " - " + o.ShippingAddress.PostalCode,
                CanRequestReturn = o.Status == OrderStatus.Delivered,
                ActiveReturnNumber = o.Returns
                    .Where(r => r.Status != ReturnStatus.Rejected)
                    .OrderByDescending(r => r.CreatedAtUtc)
                    .Select(r => r.ReturnNumber)
                    .FirstOrDefault(),
                Lines = o.Items.Select(i => new OrderLineVm
                {
                    ProductName = i.ProductName,
                    ColorName = i.ColorName,
                    SizeName = i.SizeName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.LineTotal
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);
    }
}

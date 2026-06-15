using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface IOrderService
{
    /// <summary>Builds the checkout model (cart + totals) for the given cart key.</summary>
    Task<CheckoutVm> GetCheckoutAsync(string cartKey, CancellationToken ct = default);

    /// <summary>
    /// Places an order from the customer's cart: snapshots lines, deducts inventory,
    /// records payment intent, then empties the cart. Atomic within one SaveChanges.
    /// </summary>
    Task<OrderResult> PlaceOrderAsync(string cartKey, int customerId, PlaceOrderVm form, string? couponCode = null, CancellationToken ct = default);

    Task<IReadOnlyList<OrderListItemVm>> GetOrdersForCustomerAsync(int customerId, CancellationToken ct = default);

    /// <summary>Order detail for a customer (scoped so customers only see their own orders).</summary>
    Task<OrderDetailVm?> GetOrderForCustomerAsync(int customerId, string orderNumber, CancellationToken ct = default);
}

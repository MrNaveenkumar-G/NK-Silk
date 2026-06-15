using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Common.Settings;

namespace NKSilk.Infrastructure.Payments;

/// <summary>
/// Razorpay implementation. When credentials are configured it calls the Razorpay REST
/// API; otherwise it runs in a self-contained simulation mode for local development.
/// </summary>
public class RazorpayGateway : IPaymentGateway
{
    private readonly RazorpayOptions _opt;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<RazorpayGateway> _log;

    private const string BaseUrl = "https://api.razorpay.com/v1/";

    public RazorpayGateway(IOptions<RazorpayOptions> opt, IHttpClientFactory httpFactory, ILogger<RazorpayGateway> log)
    {
        _opt = opt.Value;
        _httpFactory = httpFactory;
        _log = log;
    }

    public bool IsLive => _opt.IsLive;
    public string PublicKeyId => _opt.IsLive ? _opt.KeyId : "rzp_test_simulated";

    public async Task<GatewayOrder> CreateOrderAsync(long amountPaise, string currency, string receipt, CancellationToken ct = default)
    {
        if (!_opt.IsLive)
            return new GatewayOrder($"order_sim_{Guid.NewGuid():N}", amountPaise, currency);

        using var client = CreateClient();
        var resp = await client.PostAsJsonAsync("orders", new
        {
            amount = amountPaise,
            currency,
            receipt,
            payment_capture = 1
        }, ct);

        resp.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var id = doc.RootElement.GetProperty("id").GetString()!;
        return new GatewayOrder(id, amountPaise, currency);
    }

    public bool VerifySignature(string gatewayOrderId, string gatewayPaymentId, string signature)
    {
        if (!_opt.IsLive) return true; // simulation trusts the dev callback

        var expected = Hmac($"{gatewayOrderId}|{gatewayPaymentId}", _opt.KeySecret);
        // Constant-time comparison.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature ?? string.Empty));
    }

    public async Task<bool> RefundAsync(string gatewayPaymentId, long amountPaise, CancellationToken ct = default)
    {
        if (!_opt.IsLive) return true;
        try
        {
            using var client = CreateClient();
            var resp = await client.PostAsJsonAsync($"payments/{gatewayPaymentId}/refund", new { amount = amountPaise }, ct);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Razorpay refund failed for {PaymentId}", gatewayPaymentId);
            return false;
        }
    }

    private HttpClient CreateClient()
    {
        var client = _httpFactory.CreateClient("razorpay");
        client.BaseAddress = new Uri(BaseUrl);
        var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_opt.KeyId}:{_opt.KeySecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
        return client;
    }

    private static string Hmac(string payload, string secret)
    {
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = h.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

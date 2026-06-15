namespace NKSilk.Web.Infrastructure;

/// <summary>Reads/creates the opaque guest-cart token stored in a long-lived cookie.</summary>
public static class CartCookie
{
    private const string CookieName = "nk_cart";

    public static string GetOrCreateKey(HttpContext ctx)
    {
        if (ctx.Request.Cookies.TryGetValue(CookieName, out var existing) && !string.IsNullOrWhiteSpace(existing))
            return existing;

        var key = Guid.NewGuid().ToString("N");
        ctx.Response.Cookies.Append(CookieName, key, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
        return key;
    }
}

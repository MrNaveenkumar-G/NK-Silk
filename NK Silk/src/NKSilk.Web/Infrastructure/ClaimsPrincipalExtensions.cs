using System.Security.Claims;

namespace NKSilk.Web.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Current customer's id from the auth cookie, or 0 if not signed in.</summary>
    public static int GetCustomerId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : 0;
    }

    /// <summary>Current vendor's id from the auth cookie, or 0 if the user isn't a vendor.</summary>
    public static int GetVendorId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue("VendorId");
        return int.TryParse(raw, out var id) ? id : 0;
    }
}

using System.Security.Claims;
using NKSilk.Application.Common.Interfaces;

namespace NKSilk.Web.Infrastructure;

/// <summary>Resolves the acting user from the current HTTP request for audit attribution.</summary>
public class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;
    public HttpCurrentUser(IHttpContextAccessor http) => _http = http;

    public int? CustomerId
    {
        get
        {
            var raw = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }
    }

    public string Name => _http.HttpContext?.User.Identity?.Name ?? "system";
}

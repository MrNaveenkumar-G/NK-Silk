using Microsoft.Extensions.Configuration;
using NKSilk.Application.Common.Interfaces;

namespace NKSilk.Infrastructure.Media;

/// <summary>Rewrites local asset paths to a configured CDN base (<c>Cdn:BaseUrl</c>).</summary>
public class CdnMediaUrlResolver : IMediaUrlResolver
{
    private readonly string? _cdnBase;
    public CdnMediaUrlResolver(IConfiguration config)
        => _cdnBase = config["Cdn:BaseUrl"]?.TrimEnd('/');

    public string Resolve(string? path)
    {
        var p = string.IsNullOrWhiteSpace(path) ? "/img/product-placeholder.svg" : path;
        // Only rewrite local relative paths; leave absolute URLs untouched.
        if (string.IsNullOrEmpty(_cdnBase) || p.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return p;
        return _cdnBase + (p.StartsWith('/') ? p : "/" + p);
    }
}

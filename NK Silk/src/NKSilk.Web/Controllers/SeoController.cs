using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;

namespace NKSilk.Web.Controllers;

/// <summary>Serves sitemap.xml and robots.txt for search-engine discovery.</summary>
[ApiExplorerSettings(IgnoreApi = true)]
public class SeoController : Controller
{
    private readonly ICatalogService _catalog;
    public SeoController(ICatalogService catalog) => _catalog = catalog;

    [Route("sitemap.xml")]
    public async Task<IActionResult> Sitemap(CancellationToken ct)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urls = new List<XElement>
        {
            UrlNode(ns, baseUrl, "1.0"),
            UrlNode(ns, $"{baseUrl}/Catalog", "0.9"),
            UrlNode(ns, $"{baseUrl}/Offers", "0.7"),
            UrlNode(ns, $"{baseUrl}/Combos", "0.7")
        };

        foreach (var c in await _catalog.GetCategoriesAsync(ct))
            urls.Add(UrlNode(ns, $"{baseUrl}/Catalog?category={c.Slug}", "0.8"));

        var products = await _catalog.GetProductsAsync(page: 1, pageSize: 60, ct: ct);
        foreach (var p in products.Products)
            urls.Add(UrlNode(ns, $"{baseUrl}/Catalog/Details/{p.Slug}", "0.6"));

        var doc = new XDocument(new XElement(ns + "urlset", urls));
        return Content(doc.Declaration + "\n" + doc, "application/xml", Encoding.UTF8);
    }

    [Route("robots.txt")]
    public IActionResult Robots()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine("Disallow: /Admin");
        sb.AppendLine("Disallow: /Vendor");
        sb.AppendLine("Disallow: /Cart");
        sb.AppendLine("Disallow: /Checkout");
        sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    private static XElement UrlNode(XNamespace ns, string loc, string priority)
        => new(ns + "url", new XElement(ns + "loc", loc), new XElement(ns + "priority", priority));
}

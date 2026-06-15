using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

/// <summary>
/// Catalogue search with category facets. The default implementation is SQL-backed; the
/// seam lets a dedicated engine (Elasticsearch / Azure Cognitive Search) be swapped in for
/// 100k+ SKU scale without touching callers.
/// </summary>
public interface ISearchService
{
    Task<SearchResultVm> SearchAsync(string? query, string? categorySlug, int page = 1, int pageSize = 12, CancellationToken ct = default);
}

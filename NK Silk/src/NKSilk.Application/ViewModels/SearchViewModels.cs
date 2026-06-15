namespace NKSilk.Application.ViewModels;

public class CategoryFacetVm
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class SearchResultVm
{
    public ProductListVm Results { get; set; } = new();
    public IReadOnlyList<CategoryFacetVm> CategoryFacets { get; set; } = new List<CategoryFacetVm>();
    public string? Query { get; set; }
}

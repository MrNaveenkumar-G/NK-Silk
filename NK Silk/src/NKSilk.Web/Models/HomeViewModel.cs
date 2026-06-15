using NKSilk.Application.ViewModels;

namespace NKSilk.Web.Models;

public class HomeViewModel
{
    public IReadOnlyList<CategoryVm> Categories { get; set; } = new List<CategoryVm>();
    public IReadOnlyList<ProductCardVm> Featured { get; set; } = new List<ProductCardVm>();
    public IReadOnlyList<OfferCardVm> Offers { get; set; } = new List<OfferCardVm>();
}

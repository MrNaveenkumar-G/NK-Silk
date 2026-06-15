using System.ComponentModel.DataAnnotations;

namespace NKSilk.Application.ViewModels;

public class ComboItemVm
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class ComboCardVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal ComboPrice { get; set; }
    public decimal RegularPrice { get; set; }
    public decimal Savings => Math.Max(0, RegularPrice - ComboPrice);
    public int ItemCount { get; set; }
}

public class ComboDetailVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal ComboPrice { get; set; }
    public decimal RegularPrice => Items.Sum(i => i.UnitPrice * i.Quantity);
    public decimal Savings => Math.Max(0, RegularPrice - ComboPrice);
    public IReadOnlyList<ComboItemVm> Items { get; set; } = new List<ComboItemVm>();
}

public class AdminComboVm
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(160)] public string? Slug { get; set; }
    [StringLength(1000)] public string? Description { get; set; }
    [StringLength(500)][Display(Name = "Image URL")] public string? ImageUrl { get; set; }

    [Range(1, 10000000)][Display(Name = "Combo price")] public decimal ComboPrice { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Comma-separated "productId:qty" pairs (e.g. "12:1, 30:2").</summary>
    [Display(Name = "Items (productId:qty, comma-separated)")]
    public string? ItemsCsv { get; set; }

    // list-only
    public int ItemCount { get; set; }
    public decimal RegularPrice { get; set; }
}

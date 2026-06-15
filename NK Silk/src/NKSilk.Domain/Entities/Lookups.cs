using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>Reusable colour swatch shared across product variants.</summary>
public class Color : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Hex code such as #C41E3A for swatch rendering.</summary>
    public string HexCode { get; set; } = "#000000";

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}

/// <summary>Reusable size value (e.g. S, M, L, 38, Free Size).</summary>
public class Size : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}

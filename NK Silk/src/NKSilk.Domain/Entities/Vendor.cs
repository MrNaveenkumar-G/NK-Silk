using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>A third-party seller whose products are listed on the marketplace.</summary>
public class Vendor : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    /// <summary>Marketplace commission taken on this vendor's sales, as a percentage (e.g. 12.5).</summary>
    public decimal CommissionRate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

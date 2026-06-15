using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>A component product (and quantity) within a combo pack.</summary>
public class ComboPackItem : BaseEntity
{
    public int ComboPackId { get; set; }
    public ComboPack ComboPack { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; } = 1;
}

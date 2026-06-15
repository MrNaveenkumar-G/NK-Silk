using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>Join row assigning a <see cref="Role"/> to a <see cref="Customer"/>.</summary>
public class CustomerRole : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

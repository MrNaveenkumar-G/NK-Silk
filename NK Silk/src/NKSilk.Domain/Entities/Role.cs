using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>A named access role (Admin, Vendor, Customer …) assignable to customers.</summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<CustomerRole> CustomerRoles { get; set; } = new List<CustomerRole>();
}

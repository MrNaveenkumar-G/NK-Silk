using NKSilk.Domain.Common;
using NKSilk.Domain.Enums;

namespace NKSilk.Domain.Entities;

/// <summary>A shipping/billing address belonging to a customer.</summary>
public class Address : BaseEntity
{
    public string ContactName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "India";
    public AddressType Type { get; set; } = AddressType.Home;
    public bool IsDefault { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}

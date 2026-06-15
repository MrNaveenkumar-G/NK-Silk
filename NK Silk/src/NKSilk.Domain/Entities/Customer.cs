using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>Registered shopper. Auth (password hashing/Identity) is layered on later.</summary>
public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiresUtc { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>Grants access to the /Admin back-office area.</summary>
    public bool IsAdmin { get; set; }
    /// <summary>Grants access to the /Vendor seller area, scoped to <see cref="VendorId"/>.</summary>
    public bool IsVendor { get; set; }
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Return> Returns { get; set; } = new List<Return>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
    public ICollection<CustomerRole> CustomerRoles { get; set; } = new List<CustomerRole>();
}

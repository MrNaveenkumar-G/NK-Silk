using System.ComponentModel.DataAnnotations;

namespace NKSilk.Application.ViewModels;

public class RegisterVm
{
    [Required, StringLength(150)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(20)]
    [Display(Name = "Mobile number")]
    public string? PhoneNumber { get; set; }

    [Required, StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; } = true;
}

public class ForgotPasswordVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordVm
{
    [Required] public string Token { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class AddressVm
{
    public int Id { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public string OneLine => $"{Line1}{(string.IsNullOrEmpty(Line2) ? "" : ", " + Line2)}, {City}, {State} - {PostalCode}";
}

public class AddressFormVm
{
    public int Id { get; set; }

    [Required, StringLength(150)][Display(Name = "Full name")]
    public string ContactName { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)][Display(Name = "Mobile number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(250)][Display(Name = "Address line 1")]
    public string Line1 { get; set; } = string.Empty;

    [StringLength(250)][Display(Name = "Address line 2")]
    public string? Line2 { get; set; }

    [Required, StringLength(100)] public string City { get; set; } = string.Empty;
    [Required, StringLength(100)] public string State { get; set; } = string.Empty;
    [Required, StringLength(12)][Display(Name = "PIN code")] public string PostalCode { get; set; } = string.Empty;
    [Display(Name = "Set as default")] public bool IsDefault { get; set; }
}

/// <summary>Outcome of an account operation (register / login).</summary>
public class AuthResult
{
    public bool Succeeded { get; private set; }
    public string? Error { get; private set; }
    public int CustomerId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsAdmin { get; private set; }
    public bool IsVendor { get; private set; }
    public int? VendorId { get; private set; }
    public IReadOnlyList<string> Roles { get; private set; } = new List<string>();

    public static AuthResult Success(int id, string fullName, string email, bool isAdmin = false,
        bool isVendor = false, int? vendorId = null, IReadOnlyList<string>? roles = null)
        => new()
        {
            Succeeded = true, CustomerId = id, FullName = fullName, Email = email,
            IsAdmin = isAdmin, IsVendor = isVendor, VendorId = vendorId,
            Roles = roles ?? new List<string>()
        };

    public static AuthResult Fail(string error) => new() { Succeeded = false, Error = error };
}

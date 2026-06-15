using System.ComponentModel.DataAnnotations;

namespace NKSilk.Application.ViewModels;

// ---------------- Coupons ----------------
public class CouponValidation
{
    public bool IsValid { get; private set; }
    public string? Error { get; private set; }
    public int CouponId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public decimal DiscountAmount { get; private set; }

    public static CouponValidation Ok(int id, string code, decimal discount)
        => new() { IsValid = true, CouponId = id, Code = code, DiscountAmount = discount };
    public static CouponValidation Fail(string error) => new() { IsValid = false, Error = error };
}

// ---------------- Reviews ----------------
public class ReviewVm
{
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class ReviewFormVm
{
    public int ProductId { get; set; }

    [Range(1, 5, ErrorMessage = "Please choose a rating from 1 to 5.")]
    public int Rating { get; set; } = 5;

    [StringLength(150)]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Please write a short review."), StringLength(2000)]
    public string? Comment { get; set; }
}

// ---------------- Wishlist ----------------
public class WishlistItemVm
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public string? FabricType { get; set; }
}

public class WishlistVm
{
    public IReadOnlyList<WishlistItemVm> Items { get; set; } = new List<WishlistItemVm>();
    public bool IsEmpty => Items.Count == 0;
}

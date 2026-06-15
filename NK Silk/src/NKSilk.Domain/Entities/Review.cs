using NKSilk.Domain.Common;

namespace NKSilk.Domain.Entities;

/// <summary>Customer product review with a 1–5 star rating.</summary>
public class Review : BaseEntity
{
    public int Rating { get; set; }          // 1..5
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}

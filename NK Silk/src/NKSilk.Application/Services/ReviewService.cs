using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;

namespace NKSilk.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _uow;
    public ReviewService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<ReviewVm>> GetApprovedAsync(int productId, CancellationToken ct = default)
        => await _uow.Repository<Review>().Query()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new ReviewVm
            {
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                CustomerName = r.Customer.FullName,
                CreatedAtUtc = r.CreatedAtUtc
            })
            .ToListAsync(ct);

    public async Task<bool> AddAsync(int customerId, ReviewFormVm form, CancellationToken ct = default)
    {
        var productExists = await _uow.Repository<Product>().Query().AnyAsync(p => p.Id == form.ProductId, ct);
        if (!productExists) return false;

        await _uow.Repository<Review>().AddAsync(new Review
        {
            ProductId = form.ProductId,
            CustomerId = customerId,
            Rating = Math.Clamp(form.Rating, 1, 5),
            Title = form.Title?.Trim(),
            Comment = form.Comment?.Trim(),
            IsApproved = false, // held for admin moderation
            CreatedAtUtc = DateTime.UtcNow
        }, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}

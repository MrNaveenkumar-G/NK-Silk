using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Application.Services;

public class OfferService : IOfferService
{
    private readonly IUnitOfWork _uow;
    public OfferService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<OfferCardVm>> GetActiveBannersAsync(CancellationToken ct = default)
        => (await GetActiveOffersAsync(ct)).Where(o => !string.IsNullOrEmpty(o.BannerImageUrl)).ToList();

    public async Task<IReadOnlyList<OfferCardVm>> GetActiveOffersAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _uow.Repository<Offer>().Query()
            .Where(o => o.IsActive && o.StartsAtUtc <= now && o.EndsAtUtc >= now)
            .OrderByDescending(o => o.Priority).ThenByDescending(o => o.CreatedAtUtc)
            .Select(o => new OfferCardVm
            {
                Title = o.Title,
                Slug = o.Slug,
                Description = o.Description,
                BannerImageUrl = o.BannerImageUrl,
                OfferType = o.OfferType,
                Value = o.Value,
                Scope = o.Scope,
                ScopeName = o.Scope == OfferScope.Category && o.Category != null ? o.Category.Name
                          : o.Scope == OfferScope.Product && o.Product != null ? o.Product.Name
                          : null,
                TargetSlug = o.Scope == OfferScope.Category && o.Category != null ? o.Category.Slug
                           : o.Scope == OfferScope.Product && o.Product != null ? o.Product.Slug
                           : null,
                EndsAtUtc = o.EndsAtUtc
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AdminOfferVm>> GetAllAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _uow.Repository<Offer>().Query()
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new AdminOfferVm
            {
                Id = o.Id,
                Title = o.Title,
                Slug = o.Slug,
                OfferType = o.OfferType,
                Value = o.Value,
                Scope = o.Scope,
                StartsAtUtc = o.StartsAtUtc,
                EndsAtUtc = o.EndsAtUtc,
                Priority = o.Priority,
                IsActive = o.IsActive,
                ScopeName = o.Scope == OfferScope.Category && o.Category != null ? o.Category.Name
                          : o.Scope == OfferScope.Product && o.Product != null ? o.Product.Name
                          : "Entire store",
                IsLive = o.IsActive && o.StartsAtUtc <= now && o.EndsAtUtc >= now
            })
            .ToListAsync(ct);
    }

    public async Task<AdminOfferVm?> GetForEditAsync(int? id, CancellationToken ct = default)
    {
        var categories = await CategoriesAsync(ct);
        if (id is null or 0)
            return new AdminOfferVm { Categories = categories };

        var vm = await _uow.Repository<Offer>().Query()
            .Where(o => o.Id == id)
            .Select(o => new AdminOfferVm
            {
                Id = o.Id,
                Title = o.Title,
                Slug = o.Slug,
                Description = o.Description,
                BannerImageUrl = o.BannerImageUrl,
                OfferType = o.OfferType,
                Value = o.Value,
                Scope = o.Scope,
                CategoryId = o.CategoryId,
                ProductId = o.ProductId,
                StartsAtUtc = o.StartsAtUtc,
                EndsAtUtc = o.EndsAtUtc,
                Priority = o.Priority,
                IsActive = o.IsActive
            }).FirstOrDefaultAsync(ct);
        if (vm is null) return null;
        vm.Categories = categories;
        return vm;
    }

    public async Task<AdminResult> SaveAsync(AdminOfferVm vm, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Offer>();
        var slug = string.IsNullOrWhiteSpace(vm.Slug) ? Slugify(vm.Title) : Slugify(vm.Slug);
        if (await repo.Query().AnyAsync(o => o.Slug == slug && o.Id != vm.Id, ct))
            return AdminResult.Fail($"Slug '{slug}' is already in use.");
        if (vm.EndsAtUtc < vm.StartsAtUtc)
            return AdminResult.Fail("End date must be after the start date.");
        if (vm.OfferType == OfferType.PercentageOff && vm.Value > 100)
            return AdminResult.Fail("A percentage discount cannot exceed 100.");

        Offer entity;
        if (vm.Id == 0)
        {
            entity = new Offer { CreatedAtUtc = DateTime.UtcNow };
            Apply(entity, vm, slug);
            await repo.AddAsync(entity, ct);
        }
        else
        {
            entity = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(o => o.Id == vm.Id, ct)
                ?? throw new InvalidOperationException("Offer not found.");
            Apply(entity, vm, slug);
            repo.Update(entity);
        }
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(entity.Id);

        static void Apply(Offer o, AdminOfferVm vm, string slug)
        {
            o.Title = vm.Title.Trim();
            o.Slug = slug;
            o.Description = vm.Description;
            o.BannerImageUrl = vm.BannerImageUrl;
            o.OfferType = vm.OfferType;
            o.Value = vm.Value;
            o.Scope = vm.Scope;
            o.CategoryId = vm.Scope == OfferScope.Category ? vm.CategoryId : null;
            o.ProductId = vm.Scope == OfferScope.Product ? vm.ProductId : null;
            o.StartsAtUtc = DateTime.SpecifyKind(vm.StartsAtUtc, DateTimeKind.Utc);
            o.EndsAtUtc = DateTime.SpecifyKind(vm.EndsAtUtc.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);
            o.Priority = vm.Priority;
            o.IsActive = vm.IsActive;
        }
    }

    public async Task<AdminResult> ToggleActiveAsync(int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Offer>();
        var o = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return AdminResult.Fail("Offer not found.");
        o.IsActive = !o.IsActive;
        repo.Update(o);
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(id);
    }

    public async Task<AdminResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Offer>();
        var o = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return AdminResult.Fail("Offer not found.");
        repo.Remove(o);
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(id);
    }

    private async Task<IReadOnlyList<CategoryVm>> CategoriesAsync(CancellationToken ct)
        => await _uow.Repository<Category>().Query().OrderBy(c => c.Name)
            .Select(c => new CategoryVm { Id = c.Id, Name = c.Name, Slug = c.Slug }).ToListAsync(ct);

    private static string Slugify(string input)
    {
        var s = input.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"[\s-]+", "-").Trim('-');
        return string.IsNullOrEmpty(s) ? Guid.NewGuid().ToString("N")[..8] : s;
    }
}

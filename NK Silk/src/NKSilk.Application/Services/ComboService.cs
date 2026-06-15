using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;

namespace NKSilk.Application.Services;

public class ComboService : IComboService
{
    private readonly IUnitOfWork _uow;
    public ComboService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<ComboCardVm>> GetActiveAsync(CancellationToken ct = default)
        => await _uow.Repository<ComboPack>().Query()
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => new ComboCardVm
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                ComboPrice = c.ComboPrice,
                ItemCount = c.Items.Count,
                RegularPrice = c.Items.Sum(i => i.Product.BasePrice * i.Quantity)
            }).ToListAsync(ct);

    public async Task<ComboDetailVm?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await _uow.Repository<ComboPack>().Query()
            .Where(c => c.IsActive && c.Slug == slug)
            .Select(c => new ComboDetailVm
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ComboPrice = c.ComboPrice,
                Items = c.Items.Select(i => new ComboItemVm
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    ProductSlug = i.Product.Slug,
                    UnitPrice = i.Product.BasePrice,
                    Quantity = i.Quantity,
                    ImageUrl = i.Product.Images
                        .OrderByDescending(im => im.IsPrimary).ThenBy(im => im.DisplayOrder)
                        .Select(im => im.Url).FirstOrDefault()
                }).ToList()
            }).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<(int variantId, int qty)>> GetCartVariantsAsync(int comboId, CancellationToken ct = default)
    {
        var items = await _uow.Repository<ComboPackItem>().Query()
            .Where(i => i.ComboPackId == comboId)
            .Select(i => new
            {
                i.Quantity,
                VariantId = i.Product.Variants
                    .Where(v => v.IsActive)
                    .OrderBy(v => v.Id)
                    .Select(v => (int?)v.Id)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        return items.Where(x => x.VariantId is not null)
            .Select(x => (x.VariantId!.Value, x.Quantity))
            .ToList();
    }

    public async Task<IReadOnlyList<AdminComboVm>> GetAllAsync(CancellationToken ct = default)
        => await _uow.Repository<ComboPack>().Query()
            .OrderByDescending(c => c.CreatedAtUtc)
            .Select(c => new AdminComboVm
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ComboPrice = c.ComboPrice,
                IsActive = c.IsActive,
                ItemCount = c.Items.Count,
                RegularPrice = c.Items.Sum(i => i.Product.BasePrice * i.Quantity)
            }).ToListAsync(ct);

    public async Task<AdminComboVm?> GetForEditAsync(int? id, CancellationToken ct = default)
    {
        if (id is null or 0) return new AdminComboVm();
        var combo = await _uow.Repository<ComboPack>().Query()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id, c.Name, c.Slug, c.Description, c.ImageUrl, c.ComboPrice, c.IsActive,
                Items = c.Items.Select(i => new { i.ProductId, i.Quantity }).ToList()
            }).FirstOrDefaultAsync(ct);
        if (combo is null) return null;
        return new AdminComboVm
        {
            Id = combo.Id,
            Name = combo.Name,
            Slug = combo.Slug,
            Description = combo.Description,
            ImageUrl = combo.ImageUrl,
            ComboPrice = combo.ComboPrice,
            IsActive = combo.IsActive,
            ItemsCsv = string.Join(", ", combo.Items.Select(i => $"{i.ProductId}:{i.Quantity}"))
        };
    }

    public async Task<AdminResult> SaveAsync(AdminComboVm vm, CancellationToken ct = default)
    {
        var repo = _uow.Repository<ComboPack>();
        var slug = string.IsNullOrWhiteSpace(vm.Slug) ? Slugify(vm.Name) : Slugify(vm.Slug);
        if (await repo.Query().AnyAsync(c => c.Slug == slug && c.Id != vm.Id, ct))
            return AdminResult.Fail($"Slug '{slug}' is already in use.");

        var (items, parseError) = ParseItems(vm.ItemsCsv);
        if (parseError is not null) return AdminResult.Fail(parseError);
        if (items.Count == 0) return AdminResult.Fail("Add at least one item (productId:qty).");

        // Validate product ids exist.
        var ids = items.Select(i => i.ProductId).ToList();
        var existing = await _uow.Repository<Product>().Query().Where(p => ids.Contains(p.Id)).Select(p => p.Id).ToListAsync(ct);
        var missing = ids.Except(existing).ToList();
        if (missing.Count > 0) return AdminResult.Fail($"Unknown product id(s): {string.Join(", ", missing)}.");

        ComboPack combo;
        if (vm.Id == 0)
        {
            combo = new ComboPack { CreatedAtUtc = DateTime.UtcNow };
            ApplyScalars(combo, vm, slug);
            foreach (var it in items)
                combo.Items.Add(new ComboPackItem { ProductId = it.ProductId, Quantity = it.Quantity, CreatedAtUtc = DateTime.UtcNow });
            await repo.AddAsync(combo, ct);
        }
        else
        {
            combo = await repo.Query(asNoTracking: false)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == vm.Id, ct) ?? throw new InvalidOperationException("Combo not found.");
            ApplyScalars(combo, vm, slug);
            // Replace items.
            var itemRepo = _uow.Repository<ComboPackItem>();
            foreach (var old in combo.Items.ToList()) itemRepo.Remove(old);
            foreach (var it in items)
                combo.Items.Add(new ComboPackItem { ComboPackId = combo.Id, ProductId = it.ProductId, Quantity = it.Quantity, CreatedAtUtc = DateTime.UtcNow });
            repo.Update(combo);
        }
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(combo.Id);

        static void ApplyScalars(ComboPack c, AdminComboVm vm, string slug)
        {
            c.Name = vm.Name.Trim();
            c.Slug = slug;
            c.Description = vm.Description;
            c.ImageUrl = vm.ImageUrl;
            c.ComboPrice = vm.ComboPrice;
            c.IsActive = vm.IsActive;
        }
    }

    public async Task<AdminResult> ToggleActiveAsync(int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<ComboPack>();
        var c = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c is null) return AdminResult.Fail("Combo not found.");
        c.IsActive = !c.IsActive;
        repo.Update(c);
        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(id);
    }

    private static (List<(int ProductId, int Quantity)> items, string? error) ParseItems(string? csv)
    {
        var list = new List<(int, int)>();
        if (string.IsNullOrWhiteSpace(csv)) return (list, null);
        foreach (var raw in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = raw.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !int.TryParse(parts[0], out var pid) || !int.TryParse(parts[1], out var qty) || pid <= 0 || qty <= 0)
                return (list, $"Invalid item '{raw}'. Use the form productId:qty (e.g. 12:1).");
            list.Add((pid, qty));
        }
        return (list, null);
    }

    private static string Slugify(string input)
    {
        var s = input.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"[\s-]+", "-").Trim('-');
        return string.IsNullOrEmpty(s) ? Guid.NewGuid().ToString("N")[..8] : s;
    }
}

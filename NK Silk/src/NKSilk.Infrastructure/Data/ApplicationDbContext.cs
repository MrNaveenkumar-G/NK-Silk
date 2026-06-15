using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Domain.Common;
using NKSilk.Domain.Entities;
using NKSilk.Domain.Enums;

namespace NKSilk.Infrastructure.Data;

/// <summary>EF Core context for the NK Silk catalogue &amp; commerce model.</summary>
public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUser? _currentUser;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUser? currentUser = null)
        : base(options) => _currentUser = currentUser;

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Size> Sizes => Set<Size>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<ReturnItem> ReturnItems => Set<ReturnItem>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentEvent> ShipmentEvents => Set<ShipmentEvent>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<ComboPack> ComboPacks => Set<ComboPack>();
    public DbSet<ComboPackItem> ComboPackItems => Set<ComboPackItem>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<CustomerRole> CustomerRoles => Set<CustomerRole>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Apply all IEntityTypeConfiguration<> in this assembly.
        b.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global soft-delete filter on every BaseEntity-derived type.
        foreach (var et in b.Model.GetEntityTypes()
                     .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            var param = Expression.Parameter(et.ClrType, "e");
            var prop = Expression.Property(param, nameof(BaseEntity.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(prop), param);
            b.Entity(et.ClrType).HasQueryFilter(filter);
        }
    }

    /// <summary>Entity types whose create/update/delete is recorded to the audit trail.</summary>
    private static readonly HashSet<string> AuditedTypes = new()
    {
        nameof(Product), nameof(ProductVariant), nameof(Category), nameof(Order),
        nameof(Coupon), nameof(Offer), nameof(ComboPack), nameof(Return),
        nameof(Shipment), nameof(Customer), nameof(Vendor), nameof(Inventory)
    };

    /// <summary>Stamp audit fields automatically, then capture an audit trail of tracked changes.</summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAtUtc == default)
                        entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                    break;
            }
        }

        // Snapshot what to audit before saving (so we can read assigned ids afterwards).
        var pending = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => AuditedTypes.Contains(e.Metadata.ClrType.Name))
            .Select(e => new
            {
                e.Entity,
                Type = e.Metadata.ClrType.Name,
                Action = e.State switch
                {
                    EntityState.Added => AuditAction.Created,
                    EntityState.Deleted => AuditAction.Deleted,
                    _ => AuditAction.Updated
                },
                // For soft-deletes (Modified IsDeleted=true) treat as Deleted.
                IsSoftDelete = e.State == EntityState.Modified
                    && e.Property(nameof(BaseEntity.IsDeleted)).CurrentValue is true
                    && e.Property(nameof(BaseEntity.IsDeleted)).OriginalValue is false,
                Changes = e.State == EntityState.Modified
                    ? string.Join(", ", e.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name))
                    : null
            })
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (pending.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var p in pending)
            {
                AuditLogs.Add(new AuditLog
                {
                    Action = p.IsSoftDelete ? AuditAction.Deleted : p.Action,
                    EntityName = p.Type,
                    EntityId = p.Entity.Id,
                    UserId = _currentUser?.CustomerId,
                    UserName = _currentUser?.Name ?? "system",
                    Details = p.Changes,
                    CreatedAtUtc = now
                });
            }
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}

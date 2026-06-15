using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NKSilk.Domain.Entities;

namespace NKSilk.Infrastructure.Data.Configurations;

public class CategoryConfig : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
    }
}

public class SubCategoryConfig : IEntityTypeConfiguration<SubCategory>
{
    public void Configure(EntityTypeBuilder<SubCategory> b)
    {
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
        b.HasOne(x => x.Category).WithMany(c => c.SubCategories)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BrandConfig : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> b)
    {
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
    }
}

public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.Property(x => x.Name).HasMaxLength(250).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(260).IsRequired();
        b.Property(x => x.Sku).HasMaxLength(64).IsRequired();
        b.Property(x => x.BasePrice).HasColumnType("decimal(18,2)");
        b.Property(x => x.MrpPrice).HasColumnType("decimal(18,2)");
        b.Property(x => x.FabricType).HasMaxLength(100);
        b.Property(x => x.Occasion).HasMaxLength(100);
        b.Property(x => x.Collection).HasMaxLength(120);

        b.HasIndex(x => x.Slug).IsUnique();
        b.HasIndex(x => x.Sku).IsUnique();
        b.HasIndex(x => new { x.CategoryId, x.IsActive });

        b.HasOne(x => x.Category).WithMany(c => c.Products)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SubCategory).WithMany(s => s.Products)
            .HasForeignKey(x => x.SubCategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Brand).WithMany(br => br.Products)
            .HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Vendor).WithMany(v => v.Products)
            .HasForeignKey(x => x.VendorId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => x.VendorId);
    }
}

public class ProductImageConfig : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> b)
    {
        b.Property(x => x.Url).HasMaxLength(500).IsRequired();
        b.HasOne(x => x.Product).WithMany(p => p.Images)
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductVariantConfig : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> b)
    {
        b.Property(x => x.Sku).HasMaxLength(80).IsRequired();
        b.Property(x => x.Price).HasColumnType("decimal(18,2)");
        b.Property(x => x.MrpPrice).HasColumnType("decimal(18,2)");
        b.HasIndex(x => x.Sku).IsUnique();

        b.HasOne(x => x.Product).WithMany(p => p.Variants)
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Color).WithMany(c => c.Variants)
            .HasForeignKey(x => x.ColorId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Size).WithMany(s => s.Variants)
            .HasForeignKey(x => x.SizeId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ColorConfig : IEntityTypeConfiguration<Color>
{
    public void Configure(EntityTypeBuilder<Color> b)
    {
        b.Property(x => x.Name).HasMaxLength(60).IsRequired();
        b.Property(x => x.HexCode).HasMaxLength(9).IsRequired();
    }
}

public class SizeConfig : IEntityTypeConfiguration<Size>
{
    public void Configure(EntityTypeBuilder<Size> b)
        => b.Property(x => x.Name).HasMaxLength(40).IsRequired();
}

public class InventoryConfig : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> b)
    {
        b.Ignore(x => x.QuantityAvailable);
        b.HasOne(x => x.ProductVariant).WithOne(v => v.Inventory)
            .HasForeignKey<Inventory>(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.ProductVariantId).IsUnique();
    }
}

public class CustomerConfig : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.Property(x => x.PhoneNumber).HasMaxLength(20);
        b.HasIndex(x => x.Email).IsUnique();
        b.HasOne(x => x.Vendor).WithMany()
            .HasForeignKey(x => x.VendorId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class AddressConfig : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> b)
    {
        b.Property(x => x.ContactName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Line1).HasMaxLength(250).IsRequired();
        b.Property(x => x.City).HasMaxLength(100).IsRequired();
        b.Property(x => x.State).HasMaxLength(100).IsRequired();
        b.Property(x => x.PostalCode).HasMaxLength(12).IsRequired();
        b.HasOne(x => x.Customer).WithMany(c => c.Addresses)
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CartConfig : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> b)
    {
        b.Property(x => x.CartKey).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.CartKey).IsUnique();
        b.HasOne(x => x.Customer).WithMany()
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class CartItemConfig : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> b)
    {
        b.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        b.Ignore(x => x.LineTotal);
        b.HasOne(x => x.Cart).WithMany(c => c.Items)
            .HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ProductVariant).WithMany()
            .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrderConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.Property(x => x.OrderNumber).HasMaxLength(32).IsRequired();
        b.HasIndex(x => x.OrderNumber).IsUnique();
        foreach (var p in new[] { nameof(Order.SubTotal), nameof(Order.DiscountAmount),
                     nameof(Order.ShippingFee), nameof(Order.TaxAmount), nameof(Order.GrandTotal) })
            b.Property(p).HasColumnType("decimal(18,2)");

        b.HasOne(x => x.Customer).WithMany(c => c.Orders)
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ShippingAddress).WithMany()
            .HasForeignKey(x => x.ShippingAddressId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Coupon).WithMany()
            .HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        b.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.ProductName).HasMaxLength(250).IsRequired();
        b.Property(x => x.VariantSku).HasMaxLength(80).IsRequired();
        b.HasOne(x => x.Order).WithMany(o => o.Items)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ProductVariant).WithMany()
            .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Currency).HasMaxLength(3);
        b.Property(x => x.GatewayOrderId).HasMaxLength(100);
        b.Property(x => x.GatewayPaymentId).HasMaxLength(100);
        b.HasOne(x => x.Order).WithOne(o => o.Payment)
            .HasForeignKey<Payment>(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CouponConfig : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> b)
    {
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.DiscountValue).HasColumnType("decimal(18,2)");
        b.Property(x => x.MinOrderAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.MaxDiscountAmount).HasColumnType("decimal(18,2)");
        b.HasIndex(x => x.Code).IsUnique();
    }
}

public class ReviewConfig : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> b)
    {
        b.Property(x => x.Title).HasMaxLength(150);
        b.Property(x => x.Comment).HasMaxLength(2000);
        b.HasOne(x => x.Product).WithMany(p => p.Reviews)
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Customer).WithMany(c => c.Reviews)
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class WishlistItemConfig : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> b)
    {
        b.HasOne(x => x.Customer).WithMany()
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        // Unique only among live rows so a soft-deleted pair can be re-added.
        b.HasIndex(x => new { x.CustomerId, x.ProductId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}

public class ReturnConfig : IEntityTypeConfiguration<Return>
{
    public void Configure(EntityTypeBuilder<Return> b)
    {
        b.Property(x => x.ReturnNumber).HasMaxLength(32).IsRequired();
        b.Property(x => x.Comments).HasMaxLength(1000);
        b.Property(x => x.ResolutionNote).HasMaxLength(1000);
        b.Property(x => x.RefundAmount).HasColumnType("decimal(18,2)");
        b.HasIndex(x => x.ReturnNumber).IsUnique();

        b.HasOne(x => x.Order).WithMany(o => o.Returns)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Customer).WithMany(c => c.Returns)
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReturnItemConfig : IEntityTypeConfiguration<ReturnItem>
{
    public void Configure(EntityTypeBuilder<ReturnItem> b)
    {
        b.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        b.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.ProductName).HasMaxLength(250).IsRequired();
        b.Property(x => x.VariantSku).HasMaxLength(80).IsRequired();

        b.HasOne(x => x.Return).WithMany(r => r.Items)
            .HasForeignKey(x => x.ReturnId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.OrderItem).WithMany()
            .HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ProductVariant).WithMany()
            .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificationConfig : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Message).HasMaxLength(1000).IsRequired();
        b.Property(x => x.LinkUrl).HasMaxLength(300);

        b.HasOne(x => x.Customer).WithMany(c => c.Notifications)
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.CustomerId, x.IsRead });
    }
}

public class VendorConfig : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> b)
    {
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        b.Property(x => x.ContactEmail).HasMaxLength(256).IsRequired();
        b.Property(x => x.PhoneNumber).HasMaxLength(20);
        b.Property(x => x.CommissionRate).HasColumnType("decimal(5,2)");
        b.HasIndex(x => x.Slug).IsUnique();
    }
}

public class ShipmentConfig : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> b)
    {
        b.Property(x => x.Courier).HasMaxLength(100).IsRequired();
        b.Property(x => x.TrackingNumber).HasMaxLength(80).IsRequired();
        b.HasOne(x => x.Order).WithOne(o => o.Shipment)
            .HasForeignKey<Shipment>(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.OrderId).IsUnique();
        b.HasIndex(x => x.TrackingNumber);
    }
}

public class ShipmentEventConfig : IEntityTypeConfiguration<ShipmentEvent>
{
    public void Configure(EntityTypeBuilder<ShipmentEvent> b)
    {
        b.Property(x => x.Note).HasMaxLength(300);
        b.HasOne(x => x.Shipment).WithMany(s => s.Events)
            .HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SupportTicketConfig : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> b)
    {
        b.Property(x => x.TicketNumber).HasMaxLength(32).IsRequired();
        b.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.TicketNumber).IsUnique();

        b.HasOne(x => x.Customer).WithMany(c => c.SupportTickets)
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Order).WithMany()
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class SupportMessageConfig : IEntityTypeConfiguration<SupportMessage>
{
    public void Configure(EntityTypeBuilder<SupportMessage> b)
    {
        b.Property(x => x.Body).HasMaxLength(4000).IsRequired();
        b.Property(x => x.AuthorName).HasMaxLength(150).IsRequired();
        b.HasOne(x => x.SupportTicket).WithMany(t => t.Messages)
            .HasForeignKey(x => x.SupportTicketId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class OfferConfig : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> b)
    {
        b.Property(x => x.Title).HasMaxLength(150).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.BannerImageUrl).HasMaxLength(500);
        b.Property(x => x.Value).HasColumnType("decimal(18,2)");
        b.HasIndex(x => x.Slug).IsUnique();
        b.HasIndex(x => new { x.IsActive, x.StartsAtUtc, x.EndsAtUtc });

        b.HasOne(x => x.Category).WithMany()
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ComboPackConfig : IEntityTypeConfiguration<ComboPack>
{
    public void Configure(EntityTypeBuilder<ComboPack> b)
    {
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.ImageUrl).HasMaxLength(500);
        b.Property(x => x.ComboPrice).HasColumnType("decimal(18,2)");
        b.HasIndex(x => x.Slug).IsUnique();
    }
}

public class ComboPackItemConfig : IEntityTypeConfiguration<ComboPackItem>
{
    public void Configure(EntityTypeBuilder<ComboPackItem> b)
    {
        b.HasOne(x => x.ComboPack).WithMany(c => c.Items)
            .HasForeignKey(x => x.ComboPackId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.Property(x => x.Name).HasMaxLength(60).IsRequired();
        b.Property(x => x.Description).HasMaxLength(200);
        b.HasIndex(x => x.Name).IsUnique();
    }
}

public class CustomerRoleConfig : IEntityTypeConfiguration<CustomerRole>
{
    public void Configure(EntityTypeBuilder<CustomerRole> b)
    {
        b.HasOne(x => x.Customer).WithMany(c => c.CustomerRoles)
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Role).WithMany(r => r.CustomerRoles)
            .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.CustomerId, x.RoleId }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
        b.Property(x => x.UserName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Details).HasMaxLength(1000);
        b.HasIndex(x => new { x.EntityName, x.EntityId });
        b.HasIndex(x => x.CreatedAtUtc);
    }
}

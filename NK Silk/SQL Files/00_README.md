# NK Silk – SQL Scripts

These scripts were generated from the EF Core code-first model
(`src/NKSilk.Infrastructure/Data/Migrations/ApplicationDbContextModelSnapshot.cs`,
the entity `Configurations.cs`, and `Domain/Enums/Enums.cs`). The application is
**SQL Server / EF Core** — it has no native stored procedures or views, so these
are reconstructed from the model to give you runnable schema + insert helpers.

## Run order

| # | File | Contents |
|---|------|----------|
| 1 | `01_Tables.sql` | 32 tables, PKs, FKs, unique/filtered indexes, `DEFAULT` constraints |
| 2 | `02_Views.sql` | 7 reporting views (catalog, stock, orders, low-stock, offers, ratings) |
| 3 | `03_StoredProcedures_Insert.sql` | One `usp_<Table>_Insert` per table |

Run 01 → 02 → 03. Point them at the target database first (`USE [NKSilk];`).

## Reading "the values required for inserts"

In every `usp_*_Insert` procedure:

- **Parameters with NO default = REQUIRED** (the column is `NOT NULL`).
- **Parameters ending in `= NULL` (or a literal) = OPTIONAL.**
- `Id` is `IDENTITY` → never supplied; the new key comes back via `@NewId OUTPUT`
  and as a single-row result set.
- `CreatedAtUtc` auto-fills to `SYSUTCDATETIME()` if you don't pass it.
- `IsDeleted` is always `0` on insert; `UpdatedAtUtc` stays `NULL`.

### Required (NOT NULL) columns at a glance

| Table | Required values to insert |
|-------|---------------------------|
| Brands | Name, Slug |
| Categories | Name, Slug |
| SubCategories | CategoryId, Name, Slug |
| Colors | Name, HexCode |
| Sizes | Name |
| Vendors | Name, Slug, ContactEmail |
| Roles | Name |
| Coupons | Code, DiscountType, DiscountValue, StartsAtUtc, EndsAtUtc |
| Customers | FullName, Email |
| CustomerRoles | CustomerId, RoleId |
| Addresses | CustomerId, ContactName, PhoneNumber, Line1, City, State, PostalCode, Country |
| Products | Name, Slug, Sku, BasePrice, CategoryId |
| ProductImages | ProductId, Url |
| ProductVariants | ProductId, Sku, Price |
| Inventories | ProductVariantId |
| Reviews | ProductId, CustomerId, Rating |
| WishlistItems | CustomerId, ProductId |
| Offers | Title, Slug, OfferType, Value, Scope, StartsAtUtc, EndsAtUtc |
| ComboPacks | Name, Slug, ComboPrice |
| ComboPackItems | ComboPackId, ProductId |
| Carts | CartKey |
| CartItems | CartId, ProductVariantId, UnitPrice |
| Orders | OrderNumber, CustomerId, ShippingAddressId |
| OrderItems | OrderId, ProductVariantId, ProductName, VariantSku, Quantity, UnitPrice, LineTotal |
| Payments | OrderId, Amount, Method |
| Shipments | OrderId, Courier, TrackingNumber |
| ShipmentEvents | ShipmentId, Status |
| Returns | ReturnNumber, OrderId, CustomerId, Reason |
| ReturnItems | ReturnId, OrderItemId, ProductVariantId, ProductName, VariantSku, Quantity, UnitPrice, LineTotal |
| SupportTickets | TicketNumber, CustomerId, Category, Subject |
| SupportMessages | SupportTicketId, AuthorName, Body |
| Notifications | CustomerId, Title, Message, Type |
| AuditLogs | Action, EntityName, EntityId, UserName |

> Unique keys you must keep distinct: `Slug` (Brands, Categories, SubCategories,
> Products, ComboPacks, Offers, Vendors), `Sku` (Products, ProductVariants),
> `Email` (Customers), `Code` (Coupons), `Name` (Roles), `OrderNumber`,
> `ReturnNumber`, `TicketNumber`, `CartKey`, and one `Inventory`/`Payment`/`Shipment`
> per variant/order.

## Enum value maps (INT columns)

| Column(s) | Values |
|-----------|--------|
| Order.Status | 0 Pending, 1 Confirmed, 2 Packed, 3 Shipped, 4 OutForDelivery, 5 Delivered, 6 Cancelled, 7 Returned |
| Payment.Status | 0 Pending, 1 Authorized, 2 Paid, 3 Failed, 4 Refunded, 5 PartiallyRefunded |
| Payment.Method | 0 CashOnDelivery, 1 Razorpay, 2 PhonePe, 3 Upi, 4 CreditCard, 5 DebitCard, 6 NetBanking |
| Return.Status | 0 Requested, 1 Approved, 2 Rejected, 3 PickedUp, 4 Refunded |
| Return.Reason | 0 DefectiveOrDamaged, 1 WrongItemDelivered, 2 SizeOrFitIssue, 3 NotAsDescribed, 4 QualityNotSatisfactory, 5 ChangedMind, 6 Other |
| Notification.Type | 0 General, 1 OrderPlaced, 2 OrderStatusChanged, 3 PaymentReceived, 4 ReturnRequested, 5 ReturnUpdate, 6 Shipment, 7 SupportReply |
| Shipment.Status / ShipmentEvent.Status | 0 LabelCreated, 1 PickedUp, 2 InTransit, 3 OutForDelivery, 4 Delivered, 5 Failed |
| SupportTicket.Status | 0 Open, 1 AwaitingCustomer, 2 Resolved, 3 Closed |
| SupportTicket.Category | 0 Order, 1 Payment, 2 ReturnRefund, 3 Product, 4 Other |
| Offer.OfferType | 0 PercentageOff, 1 FlatOff |
| Offer.Scope | 0 EntireStore, 1 Category, 2 Product |
| Coupon.DiscountType | 0 Percentage, 1 FlatAmount |
| AuditLog.Action | 0 Created, 1 Updated, 2 Deleted |
| Address.Type | 0 Home, 1 Work, 2 Other |

## Worked example – seed a catalog and place an order

```sql
DECLARE @catId INT, @brandId INT, @colId INT, @sizeId INT,
        @prodId INT, @varId INT, @custId INT, @addrId INT,
        @orderId INT, @x INT;

EXEC dbo.usp_Category_Insert @Name=N'Sarees',  @Slug=N'sarees',  @NewId=@catId   OUTPUT;
EXEC dbo.usp_Brand_Insert    @Name=N'NK Silk',  @Slug=N'nk-silk', @NewId=@brandId OUTPUT;
EXEC dbo.usp_Color_Insert    @Name=N'Maroon',   @HexCode=N'#800000', @NewId=@colId OUTPUT;
EXEC dbo.usp_Size_Insert     @Name=N'Free Size', @NewId=@sizeId OUTPUT;

EXEC dbo.usp_Product_Insert
     @Name=N'Kanchipuram Silk Saree', @Slug=N'kanchipuram-silk-saree',
     @Sku=N'SAR-001', @BasePrice=4999.00, @CategoryId=@catId,
     @BrandId=@brandId, @MrpPrice=6999.00, @IsFeatured=1, @NewId=@prodId OUTPUT;

EXEC dbo.usp_ProductVariant_Insert
     @ProductId=@prodId, @Sku=N'SAR-001-MRN', @Price=4999.00,
     @ColorId=@colId, @SizeId=@sizeId, @NewId=@varId OUTPUT;

EXEC dbo.usp_Inventory_Insert @ProductVariantId=@varId, @QuantityOnHand=50, @ReorderLevel=5, @NewId=@x OUTPUT;

EXEC dbo.usp_Customer_Insert @FullName=N'Asha R', @Email=N'asha@example.com', @NewId=@custId OUTPUT;
EXEC dbo.usp_Address_Insert
     @CustomerId=@custId, @ContactName=N'Asha R', @PhoneNumber=N'9000000000',
     @Line1=N'12 Market St', @City=N'Coimbatore', @State=N'Tamil Nadu',
     @PostalCode=N'641001', @Country=N'India', @IsDefault=1, @NewId=@addrId OUTPUT;

EXEC dbo.usp_Order_Insert
     @OrderNumber=N'NK-2026-0001', @CustomerId=@custId, @ShippingAddressId=@addrId,
     @SubTotal=4999.00, @ShippingFee=0, @TaxAmount=249.95, @GrandTotal=5248.95,
     @Status=1 /*Confirmed*/, @NewId=@orderId OUTPUT;

EXEC dbo.usp_OrderItem_Insert
     @OrderId=@orderId, @ProductVariantId=@varId, @ProductName=N'Kanchipuram Silk Saree',
     @VariantSku=N'SAR-001-MRN', @Quantity=1, @UnitPrice=4999.00, @LineTotal=4999.00,
     @ColorName=N'Maroon', @SizeName=N'Free Size', @NewId=@x OUTPUT;

EXEC dbo.usp_Payment_Insert
     @OrderId=@orderId, @Amount=5248.95, @Method=3 /*Upi*/, @Status=2 /*Paid*/,
     @PaidAtUtc=SYSUTCDATETIME(), @NewId=@x OUTPUT;
```

## Notes / caveats

- `Inventory.QuantityAvailable` and `CartItem.LineTotal` are **computed in the
  app and not stored** (EF `Ignore`). The `vw_VariantStock` view recreates
  availability as `QuantityOnHand - QuantityReserved`.
- The EF model itself defines **no SQL `DEFAULT` constraints**; the defaults in
  `01_Tables.sql` (e.g. `IsActive=1`, `CreatedAtUtc=SYSUTCDATETIME()`) were added
  here so direct/manual inserts are convenient. The C# app sets these values
  explicitly, so they remain compatible.
- A few EF relationships use `ON DELETE` behaviors that SQL Server cannot express
  directly without "multiple cascade paths" errors (e.g. `WishlistItems→Products`,
  `Orders→Customers/Addresses`). Those FKs are created as `NO ACTION`; the cascade
  is enforced by the application. This does not affect inserts.

/* ============================================================================
   NK Silk - INSERT Stored Procedures  (Microsoft SQL Server)
   ----------------------------------------------------------------------------
   One usp_<Table>_Insert procedure per table. Run AFTER 01_Tables.sql.

   How to read the "values required for inserts":
     - Parameters WITHOUT a default value are REQUIRED (map to NOT NULL columns).
     - Parameters WITH "= NULL" are OPTIONAL (nullable columns).
     - [Id] is IDENTITY -> never passed; the new key is returned via
       @NewId OUTPUT and also as a result set (SELECT).
     - [CreatedAtUtc] auto-fills to SYSUTCDATETIME() when not supplied.
     - [IsDeleted] is forced to 0 on insert (new rows are always live).
     - [UpdatedAtUtc] stays NULL on insert (set it only on updates).
   INT enum columns: pass the numeric code (see value maps in 01_Tables.sql /
   00_README.md). Example: Order @Status = 0 (Pending).
   ============================================================================ */
GO

/* ===========================  MASTER / LOOKUP DATA  ======================== */

CREATE OR ALTER PROCEDURE dbo.usp_Brand_Insert
    @Name         NVARCHAR(150),
    @Slug         NVARCHAR(160),
    @LogoUrl      NVARCHAR(MAX) = NULL,
    @IsActive     BIT           = 1,
    @CreatedAtUtc DATETIME2     = NULL,
    @NewId        INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Brands (Name, Slug, LogoUrl, IsActive, IsDeleted, CreatedAtUtc)
    VALUES (@Name, @Slug, @LogoUrl, @IsActive, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Category_Insert
    @Name         NVARCHAR(150),
    @Slug         NVARCHAR(160),
    @Description  NVARCHAR(MAX) = NULL,
    @ImageUrl     NVARCHAR(MAX) = NULL,
    @DisplayOrder INT           = 0,
    @IsActive     BIT           = 1,
    @CreatedAtUtc DATETIME2     = NULL,
    @NewId        INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Categories (Name, Slug, Description, ImageUrl, DisplayOrder, IsActive, IsDeleted, CreatedAtUtc)
    VALUES (@Name, @Slug, @Description, @ImageUrl, @DisplayOrder, @IsActive, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SubCategory_Insert
    @CategoryId   INT,
    @Name         NVARCHAR(150),
    @Slug         NVARCHAR(160),
    @ImageUrl     NVARCHAR(MAX) = NULL,
    @DisplayOrder INT           = 0,
    @IsActive     BIT           = 1,
    @CreatedAtUtc DATETIME2     = NULL,
    @NewId        INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.SubCategories (CategoryId, Name, Slug, ImageUrl, DisplayOrder, IsActive, IsDeleted, CreatedAtUtc)
    VALUES (@CategoryId, @Name, @Slug, @ImageUrl, @DisplayOrder, @IsActive, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Color_Insert
    @Name         NVARCHAR(60),
    @HexCode      NVARCHAR(9),
    @CreatedAtUtc DATETIME2 = NULL,
    @NewId        INT       OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Colors (Name, HexCode, IsDeleted, CreatedAtUtc)
    VALUES (@Name, @HexCode, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Size_Insert
    @Name         NVARCHAR(40),
    @DisplayOrder INT       = 0,
    @CreatedAtUtc DATETIME2 = NULL,
    @NewId        INT       OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Sizes (Name, DisplayOrder, IsDeleted, CreatedAtUtc)
    VALUES (@Name, @DisplayOrder, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Vendor_Insert
    @Name           NVARCHAR(150),
    @Slug           NVARCHAR(160),
    @ContactEmail   NVARCHAR(256),
    @PhoneNumber    NVARCHAR(20)  = NULL,
    @CommissionRate DECIMAL(5,2)  = 0,
    @IsActive       BIT           = 1,
    @CreatedAtUtc   DATETIME2     = NULL,
    @NewId          INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Vendors (Name, Slug, ContactEmail, PhoneNumber, CommissionRate, IsActive, IsDeleted, CreatedAtUtc)
    VALUES (@Name, @Slug, @ContactEmail, @PhoneNumber, @CommissionRate, @IsActive, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Role_Insert
    @Name         NVARCHAR(60),
    @Description  NVARCHAR(200) = NULL,
    @CreatedAtUtc DATETIME2     = NULL,
    @NewId        INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Roles (Name, Description, IsDeleted, CreatedAtUtc)
    VALUES (@Name, @Description, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Coupon_Insert
    @Code              NVARCHAR(40),
    @DiscountType      INT,             -- 0=Percentage, 1=FlatAmount
    @DiscountValue     DECIMAL(18,2),
    @StartsAtUtc       DATETIME2,
    @EndsAtUtc         DATETIME2,
    @Description       NVARCHAR(MAX) = NULL,
    @MinOrderAmount    DECIMAL(18,2) = NULL,
    @MaxDiscountAmount DECIMAL(18,2) = NULL,
    @UsageLimit        INT           = NULL,
    @IsActive          BIT           = 1,
    @CreatedAtUtc      DATETIME2     = NULL,
    @NewId             INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Coupons (Code, Description, DiscountType, DiscountValue, MinOrderAmount, MaxDiscountAmount,
                        StartsAtUtc, EndsAtUtc, UsageLimit, TimesUsed, IsActive, IsDeleted, CreatedAtUtc)
    VALUES (@Code, @Description, @DiscountType, @DiscountValue, @MinOrderAmount, @MaxDiscountAmount,
            @StartsAtUtc, @EndsAtUtc, @UsageLimit, 0, @IsActive, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

/* ===========================  CUSTOMERS & ACCESS  ========================= */

CREATE OR ALTER PROCEDURE dbo.usp_Customer_Insert
    @FullName        NVARCHAR(150),
    @Email           NVARCHAR(256),
    @PhoneNumber     NVARCHAR(20)  = NULL,
    @PasswordHash    NVARCHAR(MAX) = NULL,
    @IsActive        BIT           = 1,
    @IsAdmin         BIT           = 0,
    @IsVendor        BIT           = 0,
    @VendorId        INT           = NULL,
    @IsEmailVerified BIT           = 0,
    @CreatedAtUtc    DATETIME2     = NULL,
    @NewId           INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Customers (FullName, Email, PhoneNumber, PasswordHash, IsActive, IsAdmin, IsVendor,
                          VendorId, IsEmailVerified, IsDeleted, CreatedAtUtc)
    VALUES (@FullName, @Email, @PhoneNumber, @PasswordHash, @IsActive, @IsAdmin, @IsVendor,
            @VendorId, @IsEmailVerified, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_CustomerRole_Insert
    @CustomerId   INT,
    @RoleId       INT,
    @CreatedAtUtc DATETIME2 = NULL,
    @NewId        INT       OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.CustomerRoles (CustomerId, RoleId, IsDeleted, CreatedAtUtc)
    VALUES (@CustomerId, @RoleId, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Address_Insert
    @CustomerId   INT,
    @ContactName  NVARCHAR(150),
    @PhoneNumber  NVARCHAR(MAX),
    @Line1        NVARCHAR(250),
    @City         NVARCHAR(100),
    @State        NVARCHAR(100),
    @PostalCode   NVARCHAR(12),
    @Country      NVARCHAR(MAX),
    @Line2        NVARCHAR(MAX) = NULL,
    @Type         INT           = 0,   -- 0=Home, 1=Work, 2=Other
    @IsDefault    BIT           = 0,
    @CreatedAtUtc DATETIME2     = NULL,
    @NewId        INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Addresses (CustomerId, ContactName, PhoneNumber, Line1, Line2, City, State,
                          PostalCode, Country, Type, IsDefault, IsDeleted, CreatedAtUtc)
    VALUES (@CustomerId, @ContactName, @PhoneNumber, @Line1, @Line2, @City, @State,
            @PostalCode, @Country, @Type, @IsDefault, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

/* ===========================  PRODUCTS & CATALOG  ========================= */

CREATE OR ALTER PROCEDURE dbo.usp_Product_Insert
    @Name                NVARCHAR(250),
    @Slug                NVARCHAR(260),
    @Sku                 NVARCHAR(64),
    @BasePrice           DECIMAL(18,2),
    @CategoryId          INT,
    @ShortDescription    NVARCHAR(MAX) = NULL,
    @Description         NVARCHAR(MAX) = NULL,
    @MrpPrice            DECIMAL(18,2) = NULL,
    @SubCategoryId       INT           = NULL,
    @BrandId             INT           = NULL,
    @VendorId            INT           = NULL,
    @FabricType          NVARCHAR(100) = NULL,
    @Occasion            NVARCHAR(100) = NULL,
    @Collection          NVARCHAR(120) = NULL,
    @MaterialComposition NVARCHAR(MAX) = NULL,
    @WashCare            NVARCHAR(MAX) = NULL,
    @Gsm                 INT           = NULL,
    @IsActive            BIT           = 1,
    @IsFeatured          BIT           = 0,
    @CreatedAtUtc        DATETIME2     = NULL,
    @NewId               INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Products (Name, Slug, Sku, ShortDescription, Description, BasePrice, MrpPrice,
                         CategoryId, SubCategoryId, BrandId, VendorId, FabricType, Occasion, Collection,
                         MaterialComposition, WashCare, Gsm, IsActive, IsFeatured, IsDeleted, CreatedAtUtc)
    VALUES (@Name, @Slug, @Sku, @ShortDescription, @Description, @BasePrice, @MrpPrice,
            @CategoryId, @SubCategoryId, @BrandId, @VendorId, @FabricType, @Occasion, @Collection,
            @MaterialComposition, @WashCare, @Gsm, @IsActive, @IsFeatured, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ProductImage_Insert
    @ProductId    INT,
    @Url          NVARCHAR(500),
    @AltText      NVARCHAR(MAX) = NULL,
    @DisplayOrder INT           = 0,
    @IsPrimary    BIT           = 0,
    @CreatedAtUtc DATETIME2     = NULL,
    @NewId        INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.ProductImages (ProductId, Url, AltText, DisplayOrder, IsPrimary, IsDeleted, CreatedAtUtc)
    VALUES (@ProductId, @Url, @AltText, @DisplayOrder, @IsPrimary, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ProductVariant_Insert
    @ProductId    INT,
    @Sku          NVARCHAR(80),
    @Price        DECIMAL(18,2),
    @ColorId      INT           = NULL,
    @SizeId       INT           = NULL,
    @MrpPrice     DECIMAL(18,2) = NULL,
    @IsActive     BIT           = 1,
    @CreatedAtUtc DATETIME2     = NULL,
    @NewId        INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.ProductVariants (ProductId, ColorId, SizeId, Sku, Price, MrpPrice, IsActive, IsDeleted, CreatedAtUtc)
    VALUES (@ProductId, @ColorId, @SizeId, @Sku, @Price, @MrpPrice, @IsActive, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Inventory_Insert
    @ProductVariantId INT,
    @QuantityOnHand   INT       = 0,
    @QuantityReserved INT       = 0,
    @ReorderLevel     INT       = 0,
    @CreatedAtUtc     DATETIME2 = NULL,
    @NewId            INT       OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Inventories (ProductVariantId, QuantityOnHand, QuantityReserved, ReorderLevel, IsDeleted, CreatedAtUtc)
    VALUES (@ProductVariantId, @QuantityOnHand, @QuantityReserved, @ReorderLevel, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Review_Insert
    @ProductId    INT,
    @CustomerId   INT,
    @Rating       INT,             -- 1..5
    @Title        NVARCHAR(150)  = NULL,
    @Comment      NVARCHAR(2000) = NULL,
    @IsApproved   BIT            = 0,
    @CreatedAtUtc DATETIME2      = NULL,
    @NewId        INT            OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Reviews (ProductId, CustomerId, Rating, Title, Comment, IsApproved, IsDeleted, CreatedAtUtc)
    VALUES (@ProductId, @CustomerId, @Rating, @Title, @Comment, @IsApproved, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_WishlistItem_Insert
    @CustomerId   INT,
    @ProductId    INT,
    @CreatedAtUtc DATETIME2 = NULL,
    @NewId        INT       OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.WishlistItems (CustomerId, ProductId, IsDeleted, CreatedAtUtc)
    VALUES (@CustomerId, @ProductId, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

/* ===========================  OFFERS & COMBOS  ============================ */

CREATE OR ALTER PROCEDURE dbo.usp_Offer_Insert
    @Title          NVARCHAR(150),
    @Slug           NVARCHAR(160),
    @OfferType      INT,            -- 0=PercentageOff, 1=FlatOff
    @Value          DECIMAL(18,2),
    @Scope          INT,            -- 0=EntireStore, 1=Category, 2=Product
    @StartsAtUtc    DATETIME2,
    @EndsAtUtc      DATETIME2,
    @Description    NVARCHAR(500) = NULL,
    @BannerImageUrl NVARCHAR(500) = NULL,
    @CategoryId     INT           = NULL,
    @ProductId      INT           = NULL,
    @Priority       INT           = 0,
    @IsActive       BIT           = 1,
    @CreatedAtUtc   DATETIME2     = NULL,
    @NewId          INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Offers (Title, Slug, Description, BannerImageUrl, OfferType, Value, Scope,
                       CategoryId, ProductId, Priority, StartsAtUtc, EndsAtUtc, IsActive, IsDeleted, CreatedAtUtc)
    VALUES (@Title, @Slug, @Description, @BannerImageUrl, @OfferType, @Value, @Scope,
            @CategoryId, @ProductId, @Priority, @StartsAtUtc, @EndsAtUtc, @IsActive, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ComboPack_Insert
    @Name         NVARCHAR(150),
    @Slug         NVARCHAR(160),
    @ComboPrice   DECIMAL(18,2),
    @Description  NVARCHAR(1000) = NULL,
    @ImageUrl     NVARCHAR(500)  = NULL,
    @IsActive     BIT            = 1,
    @CreatedAtUtc DATETIME2      = NULL,
    @NewId        INT            OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.ComboPacks (Name, Slug, Description, ImageUrl, ComboPrice, IsActive, IsDeleted, CreatedAtUtc)
    VALUES (@Name, @Slug, @Description, @ImageUrl, @ComboPrice, @IsActive, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ComboPackItem_Insert
    @ComboPackId  INT,
    @ProductId    INT,
    @Quantity     INT       = 1,
    @CreatedAtUtc DATETIME2 = NULL,
    @NewId        INT       OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.ComboPackItems (ComboPackId, ProductId, Quantity, IsDeleted, CreatedAtUtc)
    VALUES (@ComboPackId, @ProductId, @Quantity, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

/* ===========================  CART  ====================================== */

CREATE OR ALTER PROCEDURE dbo.usp_Cart_Insert
    @CartKey      NVARCHAR(64),
    @CustomerId   INT       = NULL,
    @CreatedAtUtc DATETIME2 = NULL,
    @NewId        INT       OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Carts (CartKey, CustomerId, IsDeleted, CreatedAtUtc)
    VALUES (@CartKey, @CustomerId, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_CartItem_Insert
    @CartId           INT,
    @ProductVariantId INT,
    @UnitPrice        DECIMAL(18,2),
    @Quantity         INT       = 1,
    @CreatedAtUtc     DATETIME2 = NULL,
    @NewId            INT       OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.CartItems (CartId, ProductVariantId, Quantity, UnitPrice, IsDeleted, CreatedAtUtc)
    VALUES (@CartId, @ProductVariantId, @Quantity, @UnitPrice, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

/* ===========================  ORDERS & FULFILMENT  ======================= */

CREATE OR ALTER PROCEDURE dbo.usp_Order_Insert
    @OrderNumber       NVARCHAR(32),
    @CustomerId        INT,
    @ShippingAddressId INT,
    @CouponId          INT           = NULL,
    @SubTotal          DECIMAL(18,2) = 0,
    @DiscountAmount    DECIMAL(18,2) = 0,
    @ShippingFee       DECIMAL(18,2) = 0,
    @TaxAmount         DECIMAL(18,2) = 0,
    @GrandTotal        DECIMAL(18,2) = 0,
    @Status            INT           = 0,   -- 0=Pending..7=Returned
    @CreatedAtUtc      DATETIME2     = NULL,
    @NewId             INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Orders (OrderNumber, CustomerId, ShippingAddressId, CouponId, SubTotal, DiscountAmount,
                       ShippingFee, TaxAmount, GrandTotal, Status, IsDeleted, CreatedAtUtc)
    VALUES (@OrderNumber, @CustomerId, @ShippingAddressId, @CouponId, @SubTotal, @DiscountAmount,
            @ShippingFee, @TaxAmount, @GrandTotal, @Status, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_OrderItem_Insert
    @OrderId          INT,
    @ProductVariantId INT,
    @ProductName      NVARCHAR(250),
    @VariantSku       NVARCHAR(80),
    @Quantity         INT,
    @UnitPrice        DECIMAL(18,2),
    @LineTotal        DECIMAL(18,2),
    @ColorName        NVARCHAR(MAX) = NULL,
    @SizeName         NVARCHAR(MAX) = NULL,
    @CreatedAtUtc     DATETIME2     = NULL,
    @NewId            INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.OrderItems (OrderId, ProductVariantId, ProductName, VariantSku, ColorName, SizeName,
                           Quantity, UnitPrice, LineTotal, IsDeleted, CreatedAtUtc)
    VALUES (@OrderId, @ProductVariantId, @ProductName, @VariantSku, @ColorName, @SizeName,
            @Quantity, @UnitPrice, @LineTotal, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Payment_Insert
    @OrderId          INT,
    @Amount           DECIMAL(18,2),
    @Method           INT,             -- 0=COD,1=Razorpay,2=PhonePe,3=Upi,4=CreditCard,5=DebitCard,6=NetBanking
    @Currency         NVARCHAR(3)   = 'INR',
    @Status           INT           = 0,  -- 0=Pending..5=PartiallyRefunded
    @GatewayOrderId   NVARCHAR(100) = NULL,
    @GatewayPaymentId NVARCHAR(100) = NULL,
    @GatewaySignature NVARCHAR(MAX) = NULL,
    @PaidAtUtc        DATETIME2     = NULL,
    @CreatedAtUtc     DATETIME2     = NULL,
    @NewId            INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Payments (OrderId, Amount, Currency, Method, Status, GatewayOrderId, GatewayPaymentId,
                         GatewaySignature, PaidAtUtc, IsDeleted, CreatedAtUtc)
    VALUES (@OrderId, @Amount, @Currency, @Method, @Status, @GatewayOrderId, @GatewayPaymentId,
            @GatewaySignature, @PaidAtUtc, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Shipment_Insert
    @OrderId              INT,
    @Courier              NVARCHAR(100),
    @TrackingNumber       NVARCHAR(80),
    @Status               INT       = 0,   -- 0=LabelCreated..5=Failed
    @ShippedAtUtc         DATETIME2 = NULL,
    @EstimatedDeliveryUtc DATETIME2 = NULL,
    @DeliveredAtUtc       DATETIME2 = NULL,
    @CreatedAtUtc         DATETIME2 = NULL,
    @NewId                INT       OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Shipments (OrderId, Courier, TrackingNumber, Status, ShippedAtUtc, EstimatedDeliveryUtc,
                          DeliveredAtUtc, IsDeleted, CreatedAtUtc)
    VALUES (@OrderId, @Courier, @TrackingNumber, @Status, @ShippedAtUtc, @EstimatedDeliveryUtc,
            @DeliveredAtUtc, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ShipmentEvent_Insert
    @ShipmentId    INT,
    @Status        INT,             -- mirrors ShipmentStatus
    @Note          NVARCHAR(300) = NULL,
    @OccurredAtUtc DATETIME2     = NULL,
    @CreatedAtUtc  DATETIME2     = NULL,
    @NewId         INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.ShipmentEvents (ShipmentId, Status, Note, OccurredAtUtc, IsDeleted, CreatedAtUtc)
    VALUES (@ShipmentId, @Status, @Note, ISNULL(@OccurredAtUtc, SYSUTCDATETIME()), 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

/* ===========================  RETURNS  =================================== */

CREATE OR ALTER PROCEDURE dbo.usp_Return_Insert
    @ReturnNumber   NVARCHAR(32),
    @OrderId        INT,
    @CustomerId     INT,
    @Reason         INT,             -- 0=DefectiveOrDamaged..6=Other
    @Status         INT            = 0,  -- 0=Requested..4=Refunded
    @Comments       NVARCHAR(1000) = NULL,
    @ResolutionNote NVARCHAR(1000) = NULL,
    @RefundAmount   DECIMAL(18,2)  = 0,
    @ResolvedAtUtc  DATETIME2      = NULL,
    @CreatedAtUtc   DATETIME2      = NULL,
    @NewId          INT            OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Returns (ReturnNumber, OrderId, CustomerId, Reason, Status, Comments, ResolutionNote,
                        RefundAmount, ResolvedAtUtc, IsDeleted, CreatedAtUtc)
    VALUES (@ReturnNumber, @OrderId, @CustomerId, @Reason, @Status, @Comments, @ResolutionNote,
            @RefundAmount, @ResolvedAtUtc, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ReturnItem_Insert
    @ReturnId         INT,
    @OrderItemId      INT,
    @ProductVariantId INT,
    @ProductName      NVARCHAR(250),
    @VariantSku       NVARCHAR(80),
    @Quantity         INT,
    @UnitPrice        DECIMAL(18,2),
    @LineTotal        DECIMAL(18,2),
    @ColorName        NVARCHAR(MAX) = NULL,
    @SizeName         NVARCHAR(MAX) = NULL,
    @CreatedAtUtc     DATETIME2     = NULL,
    @NewId            INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.ReturnItems (ReturnId, OrderItemId, ProductVariantId, ProductName, VariantSku, ColorName,
                            SizeName, Quantity, UnitPrice, LineTotal, IsDeleted, CreatedAtUtc)
    VALUES (@ReturnId, @OrderItemId, @ProductVariantId, @ProductName, @VariantSku, @ColorName,
            @SizeName, @Quantity, @UnitPrice, @LineTotal, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

/* ===========================  SUPPORT & NOTIFICATIONS  =================== */

CREATE OR ALTER PROCEDURE dbo.usp_SupportTicket_Insert
    @TicketNumber NVARCHAR(32),
    @CustomerId   INT,
    @Category     INT,             -- 0=Order,1=Payment,2=ReturnRefund,3=Product,4=Other
    @Subject      NVARCHAR(200),
    @OrderId      INT       = NULL,
    @Status       INT       = 0,   -- 0=Open..3=Closed
    @CreatedAtUtc DATETIME2 = NULL,
    @NewId        INT       OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.SupportTickets (TicketNumber, CustomerId, OrderId, Category, Status, Subject, IsDeleted, CreatedAtUtc)
    VALUES (@TicketNumber, @CustomerId, @OrderId, @Category, @Status, @Subject, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SupportMessage_Insert
    @SupportTicketId INT,
    @AuthorName      NVARCHAR(150),
    @Body            NVARCHAR(4000),
    @IsStaff         BIT       = 0,
    @CreatedAtUtc    DATETIME2 = NULL,
    @NewId           INT       OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.SupportMessages (SupportTicketId, AuthorName, Body, IsStaff, IsDeleted, CreatedAtUtc)
    VALUES (@SupportTicketId, @AuthorName, @Body, @IsStaff, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Notification_Insert
    @CustomerId   INT,
    @Title        NVARCHAR(200),
    @Message      NVARCHAR(1000),
    @Type         INT,             -- 0=General..7=SupportReply
    @LinkUrl      NVARCHAR(300) = NULL,
    @IsRead       BIT           = 0,
    @CreatedAtUtc DATETIME2     = NULL,
    @NewId        INT           OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.Notifications (CustomerId, Title, Message, LinkUrl, Type, IsRead, IsDeleted, CreatedAtUtc)
    VALUES (@CustomerId, @Title, @Message, @LinkUrl, @Type, @IsRead, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

/* ===========================  AUDIT  ===================================== */

CREATE OR ALTER PROCEDURE dbo.usp_AuditLog_Insert
    @Action       INT,             -- 0=Created, 1=Updated, 2=Deleted
    @EntityName   NVARCHAR(100),
    @EntityId     INT,
    @UserName     NVARCHAR(150),
    @UserId       INT            = NULL,
    @Details      NVARCHAR(1000) = NULL,
    @CreatedAtUtc DATETIME2      = NULL,
    @NewId        INT            OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditLogs (Action, EntityName, EntityId, UserId, UserName, Details, IsDeleted, CreatedAtUtc)
    VALUES (@Action, @EntityName, @EntityId, @UserId, @UserName, @Details, 0, ISNULL(@CreatedAtUtc, SYSUTCDATETIME()));
    SET @NewId = SCOPE_IDENTITY();
    SELECT @NewId AS NewId;
END
GO

PRINT 'NK Silk: 32 insert stored procedures created successfully.';
GO

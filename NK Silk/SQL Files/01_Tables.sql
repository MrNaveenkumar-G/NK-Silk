/* ============================================================================
   NK Silk - Table Creation Script  (Microsoft SQL Server)
   ----------------------------------------------------------------------------
   Generated from the EF Core model (ApplicationDbContextModelSnapshot.cs).
   Tables are created in foreign-key dependency order so inline FK constraints
   resolve correctly. Run this file FIRST, before views and stored procedures.

   Conventions captured from the EF model:
     - Every table has an IDENTITY integer surrogate key  [Id].
     - Soft-delete flag           [IsDeleted]      bit       DEFAULT 0
     - Audit timestamps           [CreatedAtUtc]   datetime2 DEFAULT SYSUTCDATETIME()
                                  [UpdatedAtUtc]   datetime2 NULL
     - Money columns              decimal(18,2)   (CommissionRate = decimal(5,2))
     - Strings                    nvarchar(n)  /  nvarchar(max)
   ENUM-backed columns are stored as INT. See 00_README / inline comments for
   the value meanings.
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ===========================  GROUP A : no dependencies  =================== */

CREATE TABLE dbo.Brands (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Brands PRIMARY KEY,
    Name          NVARCHAR(150)  NOT NULL,
    Slug          NVARCHAR(160)  NOT NULL,
    LogoUrl       NVARCHAR(MAX)  NULL,
    IsActive      BIT            NOT NULL CONSTRAINT DF_Brands_IsActive   DEFAULT (1),
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_Brands_IsDeleted  DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_Brands_CreatedAt  DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL
);
GO
CREATE UNIQUE INDEX UX_Brands_Slug ON dbo.Brands(Slug);
GO

CREATE TABLE dbo.Categories (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
    Name          NVARCHAR(150)  NOT NULL,
    Slug          NVARCHAR(160)  NOT NULL,
    Description   NVARCHAR(MAX)  NULL,
    ImageUrl      NVARCHAR(MAX)  NULL,
    DisplayOrder  INT            NOT NULL CONSTRAINT DF_Categories_DisplayOrder DEFAULT (0),
    IsActive      BIT            NOT NULL CONSTRAINT DF_Categories_IsActive  DEFAULT (1),
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_Categories_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_Categories_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL
);
GO
CREATE UNIQUE INDEX UX_Categories_Slug ON dbo.Categories(Slug);
GO

CREATE TABLE dbo.Colors (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Colors PRIMARY KEY,
    Name          NVARCHAR(60)   NOT NULL,
    HexCode       NVARCHAR(9)    NOT NULL,
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_Colors_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_Colors_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL
);
GO

CREATE TABLE dbo.Sizes (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sizes PRIMARY KEY,
    Name          NVARCHAR(40)   NOT NULL,
    DisplayOrder  INT            NOT NULL CONSTRAINT DF_Sizes_DisplayOrder DEFAULT (0),
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_Sizes_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_Sizes_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL
);
GO

CREATE TABLE dbo.Coupons (
    Id                INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Coupons PRIMARY KEY,
    Code              NVARCHAR(40)   NOT NULL,
    Description       NVARCHAR(MAX)  NULL,
    DiscountType      INT            NOT NULL,   -- 0=Percentage, 1=FlatAmount
    DiscountValue     DECIMAL(18,2)  NOT NULL,
    MinOrderAmount    DECIMAL(18,2)  NULL,
    MaxDiscountAmount DECIMAL(18,2)  NULL,
    StartsAtUtc       DATETIME2      NOT NULL,
    EndsAtUtc         DATETIME2      NOT NULL,
    UsageLimit        INT            NULL,
    TimesUsed         INT            NOT NULL CONSTRAINT DF_Coupons_TimesUsed DEFAULT (0),
    IsActive          BIT            NOT NULL CONSTRAINT DF_Coupons_IsActive  DEFAULT (1),
    IsDeleted         BIT            NOT NULL CONSTRAINT DF_Coupons_IsDeleted DEFAULT (0),
    CreatedAtUtc      DATETIME2      NOT NULL CONSTRAINT DF_Coupons_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc      DATETIME2      NULL
);
GO
CREATE UNIQUE INDEX UX_Coupons_Code ON dbo.Coupons(Code);
GO

CREATE TABLE dbo.Roles (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    Name          NVARCHAR(60)   NOT NULL,
    Description   NVARCHAR(200)  NULL,
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_Roles_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL
);
GO
CREATE UNIQUE INDEX UX_Roles_Name ON dbo.Roles(Name);
GO

CREATE TABLE dbo.Vendors (
    Id             INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vendors PRIMARY KEY,
    Name           NVARCHAR(150)  NOT NULL,
    Slug           NVARCHAR(160)  NOT NULL,
    ContactEmail   NVARCHAR(256)  NOT NULL,
    PhoneNumber    NVARCHAR(20)   NULL,
    CommissionRate DECIMAL(5,2)   NOT NULL CONSTRAINT DF_Vendors_Commission DEFAULT (0),
    IsActive       BIT            NOT NULL CONSTRAINT DF_Vendors_IsActive  DEFAULT (1),
    IsDeleted      BIT            NOT NULL CONSTRAINT DF_Vendors_IsDeleted DEFAULT (0),
    CreatedAtUtc   DATETIME2      NOT NULL CONSTRAINT DF_Vendors_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc   DATETIME2      NULL
);
GO
CREATE UNIQUE INDEX UX_Vendors_Slug ON dbo.Vendors(Slug);
GO

CREATE TABLE dbo.AuditLogs (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
    Action        INT            NOT NULL,   -- 0=Created, 1=Updated, 2=Deleted
    EntityName    NVARCHAR(100)  NOT NULL,
    EntityId      INT            NOT NULL,
    UserId        INT            NULL,
    UserName      NVARCHAR(150)  NOT NULL,
    Details       NVARCHAR(1000) NULL,
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_AuditLogs_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL
);
GO
CREATE INDEX IX_AuditLogs_CreatedAtUtc ON dbo.AuditLogs(CreatedAtUtc);
CREATE INDEX IX_AuditLogs_Entity       ON dbo.AuditLogs(EntityName, EntityId);
GO

/* ===========================  GROUP B  ===================================== */

CREATE TABLE dbo.SubCategories (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_SubCategories PRIMARY KEY,
    CategoryId    INT            NOT NULL,
    Name          NVARCHAR(150)  NOT NULL,
    Slug          NVARCHAR(160)  NOT NULL,
    ImageUrl      NVARCHAR(MAX)  NULL,
    DisplayOrder  INT            NOT NULL CONSTRAINT DF_SubCategories_DisplayOrder DEFAULT (0),
    IsActive      BIT            NOT NULL CONSTRAINT DF_SubCategories_IsActive  DEFAULT (1),
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_SubCategories_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_SubCategories_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL,
    CONSTRAINT FK_SubCategories_Categories FOREIGN KEY (CategoryId)
        REFERENCES dbo.Categories(Id)
);
GO
CREATE UNIQUE INDEX UX_SubCategories_Slug ON dbo.SubCategories(Slug);
CREATE INDEX IX_SubCategories_CategoryId  ON dbo.SubCategories(CategoryId);
GO

CREATE TABLE dbo.Customers (
    Id                       INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    FullName                 NVARCHAR(150)  NOT NULL,
    Email                    NVARCHAR(256)  NOT NULL,
    PhoneNumber              NVARCHAR(20)   NULL,
    PasswordHash             NVARCHAR(MAX)  NULL,
    IsActive                 BIT            NOT NULL CONSTRAINT DF_Customers_IsActive   DEFAULT (1),
    IsAdmin                  BIT            NOT NULL CONSTRAINT DF_Customers_IsAdmin    DEFAULT (0),
    IsVendor                 BIT            NOT NULL CONSTRAINT DF_Customers_IsVendor   DEFAULT (0),
    VendorId                 INT            NULL,
    IsEmailVerified          BIT            NOT NULL CONSTRAINT DF_Customers_IsEmailVer DEFAULT (0),
    EmailVerificationToken   NVARCHAR(MAX)  NULL,
    PasswordResetToken       NVARCHAR(MAX)  NULL,
    PasswordResetExpiresUtc  DATETIME2      NULL,
    IsDeleted                BIT            NOT NULL CONSTRAINT DF_Customers_IsDeleted  DEFAULT (0),
    CreatedAtUtc             DATETIME2      NOT NULL CONSTRAINT DF_Customers_CreatedAt  DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc             DATETIME2      NULL,
    CONSTRAINT FK_Customers_Vendors FOREIGN KEY (VendorId)
        REFERENCES dbo.Vendors(Id)
);
GO
CREATE UNIQUE INDEX UX_Customers_Email ON dbo.Customers(Email);
CREATE INDEX IX_Customers_VendorId      ON dbo.Customers(VendorId);
GO

/* ===========================  GROUP C  ===================================== */

CREATE TABLE dbo.Addresses (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Addresses PRIMARY KEY,
    CustomerId    INT            NOT NULL,
    ContactName   NVARCHAR(150)  NOT NULL,
    PhoneNumber   NVARCHAR(MAX)  NOT NULL,
    Line1         NVARCHAR(250)  NOT NULL,
    Line2         NVARCHAR(MAX)  NULL,
    City          NVARCHAR(100)  NOT NULL,
    State         NVARCHAR(100)  NOT NULL,
    PostalCode    NVARCHAR(12)   NOT NULL,
    Country       NVARCHAR(MAX)  NOT NULL,
    Type          INT            NOT NULL CONSTRAINT DF_Addresses_Type      DEFAULT (0), -- 0=Home,1=Work,2=Other
    IsDefault     BIT            NOT NULL CONSTRAINT DF_Addresses_IsDefault DEFAULT (0),
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_Addresses_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_Addresses_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL,
    CONSTRAINT FK_Addresses_Customers FOREIGN KEY (CustomerId)
        REFERENCES dbo.Customers(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_Addresses_CustomerId ON dbo.Addresses(CustomerId);
GO

CREATE TABLE dbo.Carts (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Carts PRIMARY KEY,
    CartKey       NVARCHAR(64)   NOT NULL,
    CustomerId    INT            NULL,
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_Carts_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_Carts_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL,
    CONSTRAINT FK_Carts_Customers FOREIGN KEY (CustomerId)
        REFERENCES dbo.Customers(Id)   -- SET NULL handled in app (EF: SetNull)
);
GO
CREATE UNIQUE INDEX UX_Carts_CartKey ON dbo.Carts(CartKey);
CREATE INDEX IX_Carts_CustomerId      ON dbo.Carts(CustomerId);
GO

CREATE TABLE dbo.CustomerRoles (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomerRoles PRIMARY KEY,
    CustomerId    INT            NOT NULL,
    RoleId        INT            NOT NULL,
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_CustomerRoles_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_CustomerRoles_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL,
    CONSTRAINT FK_CustomerRoles_Customers FOREIGN KEY (CustomerId)
        REFERENCES dbo.Customers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CustomerRoles_Roles FOREIGN KEY (RoleId)
        REFERENCES dbo.Roles(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_CustomerRoles_RoleId ON dbo.CustomerRoles(RoleId);
-- Unique only among live (non-deleted) rows:
CREATE UNIQUE INDEX UX_CustomerRoles_Customer_Role
    ON dbo.CustomerRoles(CustomerId, RoleId) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Products (
    Id                   INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
    Name                 NVARCHAR(250)  NOT NULL,
    Slug                 NVARCHAR(260)  NOT NULL,
    Sku                  NVARCHAR(64)   NOT NULL,
    ShortDescription     NVARCHAR(MAX)  NULL,
    Description          NVARCHAR(MAX)  NULL,
    BasePrice            DECIMAL(18,2)  NOT NULL,
    MrpPrice             DECIMAL(18,2)  NULL,
    CategoryId           INT            NOT NULL,
    SubCategoryId        INT            NULL,
    BrandId              INT            NULL,
    VendorId             INT            NULL,
    FabricType           NVARCHAR(100)  NULL,
    Occasion             NVARCHAR(100)  NULL,
    Collection           NVARCHAR(120)  NULL,
    MaterialComposition  NVARCHAR(MAX)  NULL,
    WashCare             NVARCHAR(MAX)  NULL,
    Gsm                  INT            NULL,
    IsActive             BIT            NOT NULL CONSTRAINT DF_Products_IsActive   DEFAULT (1),
    IsFeatured           BIT            NOT NULL CONSTRAINT DF_Products_IsFeatured DEFAULT (0),
    IsDeleted            BIT            NOT NULL CONSTRAINT DF_Products_IsDeleted  DEFAULT (0),
    CreatedAtUtc         DATETIME2      NOT NULL CONSTRAINT DF_Products_CreatedAt  DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc         DATETIME2      NULL,
    CONSTRAINT FK_Products_Categories    FOREIGN KEY (CategoryId)    REFERENCES dbo.Categories(Id),
    CONSTRAINT FK_Products_SubCategories FOREIGN KEY (SubCategoryId) REFERENCES dbo.SubCategories(Id),
    CONSTRAINT FK_Products_Brands        FOREIGN KEY (BrandId)       REFERENCES dbo.Brands(Id),
    CONSTRAINT FK_Products_Vendors       FOREIGN KEY (VendorId)      REFERENCES dbo.Vendors(Id)
);
GO
CREATE UNIQUE INDEX UX_Products_Slug ON dbo.Products(Slug);
CREATE UNIQUE INDEX UX_Products_Sku  ON dbo.Products(Sku);
CREATE INDEX IX_Products_BrandId       ON dbo.Products(BrandId);
CREATE INDEX IX_Products_SubCategoryId ON dbo.Products(SubCategoryId);
CREATE INDEX IX_Products_VendorId      ON dbo.Products(VendorId);
CREATE INDEX IX_Products_Category_Active ON dbo.Products(CategoryId, IsActive);
GO

CREATE TABLE dbo.ComboPacks (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_ComboPacks PRIMARY KEY,
    Name          NVARCHAR(150)  NOT NULL,
    Slug          NVARCHAR(160)  NOT NULL,
    Description   NVARCHAR(1000) NULL,
    ImageUrl      NVARCHAR(500)  NULL,
    ComboPrice    DECIMAL(18,2)  NOT NULL,
    IsActive      BIT            NOT NULL CONSTRAINT DF_ComboPacks_IsActive  DEFAULT (1),
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_ComboPacks_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_ComboPacks_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL
);
GO
CREATE UNIQUE INDEX UX_ComboPacks_Slug ON dbo.ComboPacks(Slug);
GO

/* ===========================  GROUP D  ===================================== */

CREATE TABLE dbo.ProductImages (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductImages PRIMARY KEY,
    ProductId     INT            NOT NULL,
    Url           NVARCHAR(500)  NOT NULL,
    AltText       NVARCHAR(MAX)  NULL,
    DisplayOrder  INT            NOT NULL CONSTRAINT DF_ProductImages_DisplayOrder DEFAULT (0),
    IsPrimary     BIT            NOT NULL CONSTRAINT DF_ProductImages_IsPrimary DEFAULT (0),
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_ProductImages_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_ProductImages_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL,
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId)
        REFERENCES dbo.Products(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_ProductImages_ProductId ON dbo.ProductImages(ProductId);
GO

CREATE TABLE dbo.ProductVariants (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductVariants PRIMARY KEY,
    ProductId     INT            NOT NULL,
    ColorId       INT            NULL,
    SizeId        INT            NULL,
    Sku           NVARCHAR(80)   NOT NULL,
    Price         DECIMAL(18,2)  NOT NULL,
    MrpPrice      DECIMAL(18,2)  NULL,
    IsActive      BIT            NOT NULL CONSTRAINT DF_ProductVariants_IsActive  DEFAULT (1),
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_ProductVariants_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_ProductVariants_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL,
    CONSTRAINT FK_ProductVariants_Products FOREIGN KEY (ProductId)
        REFERENCES dbo.Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProductVariants_Colors FOREIGN KEY (ColorId) REFERENCES dbo.Colors(Id),
    CONSTRAINT FK_ProductVariants_Sizes  FOREIGN KEY (SizeId)  REFERENCES dbo.Sizes(Id)
);
GO
CREATE UNIQUE INDEX UX_ProductVariants_Sku ON dbo.ProductVariants(Sku);
CREATE INDEX IX_ProductVariants_ProductId  ON dbo.ProductVariants(ProductId);
CREATE INDEX IX_ProductVariants_ColorId    ON dbo.ProductVariants(ColorId);
CREATE INDEX IX_ProductVariants_SizeId     ON dbo.ProductVariants(SizeId);
GO

CREATE TABLE dbo.ComboPackItems (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_ComboPackItems PRIMARY KEY,
    ComboPackId   INT            NOT NULL,
    ProductId     INT            NOT NULL,
    Quantity      INT            NOT NULL CONSTRAINT DF_ComboPackItems_Quantity DEFAULT (1),
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_ComboPackItems_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_ComboPackItems_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL,
    CONSTRAINT FK_ComboPackItems_ComboPacks FOREIGN KEY (ComboPackId)
        REFERENCES dbo.ComboPacks(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ComboPackItems_Products FOREIGN KEY (ProductId)
        REFERENCES dbo.Products(Id)
);
GO
CREATE INDEX IX_ComboPackItems_ComboPackId ON dbo.ComboPackItems(ComboPackId);
CREATE INDEX IX_ComboPackItems_ProductId   ON dbo.ComboPackItems(ProductId);
GO

CREATE TABLE dbo.Reviews (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reviews PRIMARY KEY,
    ProductId     INT            NOT NULL,
    CustomerId    INT            NOT NULL,
    Rating        INT            NOT NULL,   -- typically 1..5
    Title         NVARCHAR(150)  NULL,
    Comment       NVARCHAR(2000) NULL,
    IsApproved    BIT            NOT NULL CONSTRAINT DF_Reviews_IsApproved DEFAULT (0),
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_Reviews_IsDeleted  DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_Reviews_CreatedAt  DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL,
    CONSTRAINT FK_Reviews_Products FOREIGN KEY (ProductId)
        REFERENCES dbo.Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Reviews_Customers FOREIGN KEY (CustomerId)
        REFERENCES dbo.Customers(Id)
);
GO
CREATE INDEX IX_Reviews_ProductId  ON dbo.Reviews(ProductId);
CREATE INDEX IX_Reviews_CustomerId ON dbo.Reviews(CustomerId);
GO

CREATE TABLE dbo.WishlistItems (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_WishlistItems PRIMARY KEY,
    CustomerId    INT            NOT NULL,
    ProductId     INT            NOT NULL,
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_WishlistItems_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_WishlistItems_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL,
    CONSTRAINT FK_WishlistItems_Customers FOREIGN KEY (CustomerId)
        REFERENCES dbo.Customers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WishlistItems_Products FOREIGN KEY (ProductId)
        REFERENCES dbo.Products(Id)   -- EF: Cascade (handled app-side; kept NO ACTION to avoid multiple cascade paths)
);
GO
CREATE INDEX IX_WishlistItems_ProductId ON dbo.WishlistItems(ProductId);
CREATE UNIQUE INDEX UX_WishlistItems_Customer_Product
    ON dbo.WishlistItems(CustomerId, ProductId) WHERE IsDeleted = 0;
GO

CREATE TABLE dbo.Offers (
    Id              INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Offers PRIMARY KEY,
    Title           NVARCHAR(150)  NOT NULL,
    Slug            NVARCHAR(160)  NOT NULL,
    Description     NVARCHAR(500)  NULL,
    BannerImageUrl  NVARCHAR(500)  NULL,
    OfferType       INT            NOT NULL,   -- 0=PercentageOff, 1=FlatOff
    Value           DECIMAL(18,2)  NOT NULL,
    Scope           INT            NOT NULL,   -- 0=EntireStore, 1=Category, 2=Product
    CategoryId      INT            NULL,
    ProductId       INT            NULL,
    Priority        INT            NOT NULL CONSTRAINT DF_Offers_Priority DEFAULT (0),
    StartsAtUtc     DATETIME2      NOT NULL,
    EndsAtUtc       DATETIME2      NOT NULL,
    IsActive        BIT            NOT NULL CONSTRAINT DF_Offers_IsActive  DEFAULT (1),
    IsDeleted       BIT            NOT NULL CONSTRAINT DF_Offers_IsDeleted DEFAULT (0),
    CreatedAtUtc    DATETIME2      NOT NULL CONSTRAINT DF_Offers_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc    DATETIME2      NULL,
    CONSTRAINT FK_Offers_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id),
    CONSTRAINT FK_Offers_Products   FOREIGN KEY (ProductId)  REFERENCES dbo.Products(Id)
);
GO
CREATE UNIQUE INDEX UX_Offers_Slug ON dbo.Offers(Slug);
CREATE INDEX IX_Offers_CategoryId  ON dbo.Offers(CategoryId);
CREATE INDEX IX_Offers_ProductId   ON dbo.Offers(ProductId);
CREATE INDEX IX_Offers_Active_Window ON dbo.Offers(IsActive, StartsAtUtc, EndsAtUtc);
GO

CREATE TABLE dbo.Notifications (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY,
    CustomerId    INT            NOT NULL,
    Title         NVARCHAR(200)  NOT NULL,
    Message       NVARCHAR(1000) NOT NULL,
    LinkUrl       NVARCHAR(300)  NULL,
    Type          INT            NOT NULL,   -- 0=General,1=OrderPlaced,...7=SupportReply
    IsRead        BIT            NOT NULL CONSTRAINT DF_Notifications_IsRead    DEFAULT (0),
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_Notifications_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL,
    CONSTRAINT FK_Notifications_Customers FOREIGN KEY (CustomerId)
        REFERENCES dbo.Customers(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_Notifications_Customer_IsRead ON dbo.Notifications(CustomerId, IsRead);
GO

/* ===========================  GROUP E  ===================================== */

CREATE TABLE dbo.Inventories (
    Id               INT          IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventories PRIMARY KEY,
    ProductVariantId INT          NOT NULL,
    QuantityOnHand   INT          NOT NULL CONSTRAINT DF_Inventories_OnHand   DEFAULT (0),
    QuantityReserved INT          NOT NULL CONSTRAINT DF_Inventories_Reserved DEFAULT (0),
    ReorderLevel     INT          NOT NULL CONSTRAINT DF_Inventories_Reorder  DEFAULT (0),
    IsDeleted        BIT          NOT NULL CONSTRAINT DF_Inventories_IsDeleted DEFAULT (0),
    CreatedAtUtc     DATETIME2    NOT NULL CONSTRAINT DF_Inventories_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc     DATETIME2    NULL,
    -- NOTE: QuantityAvailable is a computed/ignored property in the app
    -- (= QuantityOnHand - QuantityReserved); it is NOT a stored column.
    CONSTRAINT FK_Inventories_ProductVariants FOREIGN KEY (ProductVariantId)
        REFERENCES dbo.ProductVariants(Id) ON DELETE CASCADE
);
GO
CREATE UNIQUE INDEX UX_Inventories_ProductVariantId ON dbo.Inventories(ProductVariantId);
GO

CREATE TABLE dbo.CartItems (
    Id               INT          IDENTITY(1,1) NOT NULL CONSTRAINT PK_CartItems PRIMARY KEY,
    CartId           INT          NOT NULL,
    ProductVariantId INT          NOT NULL,
    Quantity         INT          NOT NULL CONSTRAINT DF_CartItems_Quantity DEFAULT (1),
    UnitPrice        DECIMAL(18,2) NOT NULL,
    IsDeleted        BIT          NOT NULL CONSTRAINT DF_CartItems_IsDeleted DEFAULT (0),
    CreatedAtUtc     DATETIME2    NOT NULL CONSTRAINT DF_CartItems_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc     DATETIME2    NULL,
    -- NOTE: LineTotal is computed/ignored in the app (= Quantity * UnitPrice).
    CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId)
        REFERENCES dbo.Carts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_ProductVariants FOREIGN KEY (ProductVariantId)
        REFERENCES dbo.ProductVariants(Id)
);
GO
CREATE INDEX IX_CartItems_CartId           ON dbo.CartItems(CartId);
CREATE INDEX IX_CartItems_ProductVariantId ON dbo.CartItems(ProductVariantId);
GO

CREATE TABLE dbo.Orders (
    Id                INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
    OrderNumber       NVARCHAR(32)   NOT NULL,
    CustomerId        INT            NOT NULL,
    ShippingAddressId INT            NOT NULL,
    CouponId          INT            NULL,
    SubTotal          DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Orders_SubTotal  DEFAULT (0),
    DiscountAmount    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Orders_Discount  DEFAULT (0),
    ShippingFee       DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Orders_Shipping  DEFAULT (0),
    TaxAmount         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Orders_Tax       DEFAULT (0),
    GrandTotal        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Orders_Grand     DEFAULT (0),
    Status            INT            NOT NULL CONSTRAINT DF_Orders_Status     DEFAULT (0), -- 0=Pending..7=Returned
    IsDeleted         BIT            NOT NULL CONSTRAINT DF_Orders_IsDeleted  DEFAULT (0),
    CreatedAtUtc      DATETIME2      NOT NULL CONSTRAINT DF_Orders_CreatedAt  DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc      DATETIME2      NULL,
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId)        REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_Orders_Addresses FOREIGN KEY (ShippingAddressId) REFERENCES dbo.Addresses(Id),
    CONSTRAINT FK_Orders_Coupons   FOREIGN KEY (CouponId)          REFERENCES dbo.Coupons(Id)
);
GO
CREATE UNIQUE INDEX UX_Orders_OrderNumber ON dbo.Orders(OrderNumber);
CREATE INDEX IX_Orders_CustomerId        ON dbo.Orders(CustomerId);
CREATE INDEX IX_Orders_CouponId          ON dbo.Orders(CouponId);
CREATE INDEX IX_Orders_ShippingAddressId ON dbo.Orders(ShippingAddressId);
GO

/* ===========================  GROUP F  ===================================== */

CREATE TABLE dbo.OrderItems (
    Id               INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderItems PRIMARY KEY,
    OrderId          INT            NOT NULL,
    ProductVariantId INT            NOT NULL,
    ProductName      NVARCHAR(250)  NOT NULL,
    VariantSku       NVARCHAR(80)   NOT NULL,
    ColorName        NVARCHAR(MAX)  NULL,
    SizeName         NVARCHAR(MAX)  NULL,
    Quantity         INT            NOT NULL,
    UnitPrice        DECIMAL(18,2)  NOT NULL,
    LineTotal        DECIMAL(18,2)  NOT NULL,
    IsDeleted        BIT            NOT NULL CONSTRAINT DF_OrderItems_IsDeleted DEFAULT (0),
    CreatedAtUtc     DATETIME2      NOT NULL CONSTRAINT DF_OrderItems_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc     DATETIME2      NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId)
        REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_ProductVariants FOREIGN KEY (ProductVariantId)
        REFERENCES dbo.ProductVariants(Id)
);
GO
CREATE INDEX IX_OrderItems_OrderId          ON dbo.OrderItems(OrderId);
CREATE INDEX IX_OrderItems_ProductVariantId ON dbo.OrderItems(ProductVariantId);
GO

CREATE TABLE dbo.Payments (
    Id                INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
    OrderId           INT            NOT NULL,
    Amount            DECIMAL(18,2)  NOT NULL,
    Currency          NVARCHAR(3)    NOT NULL CONSTRAINT DF_Payments_Currency DEFAULT ('INR'),
    Method            INT            NOT NULL,   -- 0=COD,1=Razorpay,2=PhonePe,3=Upi,4=CreditCard,5=DebitCard,6=NetBanking
    Status            INT            NOT NULL CONSTRAINT DF_Payments_Status DEFAULT (0), -- 0=Pending..5=PartiallyRefunded
    GatewayOrderId    NVARCHAR(100)  NULL,
    GatewayPaymentId  NVARCHAR(100)  NULL,
    GatewaySignature  NVARCHAR(MAX)  NULL,
    PaidAtUtc         DATETIME2      NULL,
    IsDeleted         BIT            NOT NULL CONSTRAINT DF_Payments_IsDeleted DEFAULT (0),
    CreatedAtUtc      DATETIME2      NOT NULL CONSTRAINT DF_Payments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc      DATETIME2      NULL,
    CONSTRAINT FK_Payments_Orders FOREIGN KEY (OrderId)
        REFERENCES dbo.Orders(Id) ON DELETE CASCADE
);
GO
CREATE UNIQUE INDEX UX_Payments_OrderId ON dbo.Payments(OrderId);
GO

CREATE TABLE dbo.Shipments (
    Id                   INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Shipments PRIMARY KEY,
    OrderId              INT            NOT NULL,
    Courier              NVARCHAR(100)  NOT NULL,
    TrackingNumber       NVARCHAR(80)   NOT NULL,
    Status               INT            NOT NULL CONSTRAINT DF_Shipments_Status DEFAULT (0), -- 0=LabelCreated..5=Failed
    ShippedAtUtc         DATETIME2      NULL,
    EstimatedDeliveryUtc DATETIME2      NULL,
    DeliveredAtUtc       DATETIME2      NULL,
    IsDeleted            BIT            NOT NULL CONSTRAINT DF_Shipments_IsDeleted DEFAULT (0),
    CreatedAtUtc         DATETIME2      NOT NULL CONSTRAINT DF_Shipments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc         DATETIME2      NULL,
    CONSTRAINT FK_Shipments_Orders FOREIGN KEY (OrderId)
        REFERENCES dbo.Orders(Id) ON DELETE CASCADE
);
GO
CREATE UNIQUE INDEX UX_Shipments_OrderId  ON dbo.Shipments(OrderId);
CREATE INDEX IX_Shipments_TrackingNumber  ON dbo.Shipments(TrackingNumber);
GO

CREATE TABLE dbo.Returns (
    Id             INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_Returns PRIMARY KEY,
    ReturnNumber   NVARCHAR(32)   NOT NULL,
    OrderId        INT            NOT NULL,
    CustomerId     INT            NOT NULL,
    Reason         INT            NOT NULL,   -- 0=DefectiveOrDamaged..6=Other
    Status         INT            NOT NULL CONSTRAINT DF_Returns_Status DEFAULT (0), -- 0=Requested..4=Refunded
    Comments       NVARCHAR(1000) NULL,
    ResolutionNote NVARCHAR(1000) NULL,
    RefundAmount   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_Returns_Refund DEFAULT (0),
    ResolvedAtUtc  DATETIME2      NULL,
    IsDeleted      BIT            NOT NULL CONSTRAINT DF_Returns_IsDeleted DEFAULT (0),
    CreatedAtUtc   DATETIME2      NOT NULL CONSTRAINT DF_Returns_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc   DATETIME2      NULL,
    CONSTRAINT FK_Returns_Orders    FOREIGN KEY (OrderId)    REFERENCES dbo.Orders(Id),
    CONSTRAINT FK_Returns_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id)
);
GO
CREATE UNIQUE INDEX UX_Returns_ReturnNumber ON dbo.Returns(ReturnNumber);
CREATE INDEX IX_Returns_OrderId    ON dbo.Returns(OrderId);
CREATE INDEX IX_Returns_CustomerId ON dbo.Returns(CustomerId);
GO

CREATE TABLE dbo.SupportTickets (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupportTickets PRIMARY KEY,
    TicketNumber  NVARCHAR(32)   NOT NULL,
    CustomerId    INT            NOT NULL,
    OrderId       INT            NULL,
    Category      INT            NOT NULL,   -- 0=Order,1=Payment,2=ReturnRefund,3=Product,4=Other
    Status        INT            NOT NULL CONSTRAINT DF_SupportTickets_Status DEFAULT (0), -- 0=Open..3=Closed
    Subject       NVARCHAR(200)  NOT NULL,
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_SupportTickets_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_SupportTickets_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL,
    CONSTRAINT FK_SupportTickets_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_SupportTickets_Orders    FOREIGN KEY (OrderId)    REFERENCES dbo.Orders(Id)
);
GO
CREATE UNIQUE INDEX UX_SupportTickets_TicketNumber ON dbo.SupportTickets(TicketNumber);
CREATE INDEX IX_SupportTickets_CustomerId ON dbo.SupportTickets(CustomerId);
CREATE INDEX IX_SupportTickets_OrderId    ON dbo.SupportTickets(OrderId);
GO

/* ===========================  GROUP G  ===================================== */

CREATE TABLE dbo.ShipmentEvents (
    Id            INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_ShipmentEvents PRIMARY KEY,
    ShipmentId    INT            NOT NULL,
    Status        INT            NOT NULL,   -- mirrors ShipmentStatus
    Note          NVARCHAR(300)  NULL,
    OccurredAtUtc DATETIME2      NOT NULL CONSTRAINT DF_ShipmentEvents_Occurred DEFAULT (SYSUTCDATETIME()),
    IsDeleted     BIT            NOT NULL CONSTRAINT DF_ShipmentEvents_IsDeleted DEFAULT (0),
    CreatedAtUtc  DATETIME2      NOT NULL CONSTRAINT DF_ShipmentEvents_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc  DATETIME2      NULL,
    CONSTRAINT FK_ShipmentEvents_Shipments FOREIGN KEY (ShipmentId)
        REFERENCES dbo.Shipments(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_ShipmentEvents_ShipmentId ON dbo.ShipmentEvents(ShipmentId);
GO

CREATE TABLE dbo.ReturnItems (
    Id               INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReturnItems PRIMARY KEY,
    ReturnId         INT            NOT NULL,
    OrderItemId      INT            NOT NULL,
    ProductVariantId INT            NOT NULL,
    ProductName      NVARCHAR(250)  NOT NULL,
    VariantSku       NVARCHAR(80)   NOT NULL,
    ColorName        NVARCHAR(MAX)  NULL,
    SizeName         NVARCHAR(MAX)  NULL,
    Quantity         INT            NOT NULL,
    UnitPrice        DECIMAL(18,2)  NOT NULL,
    LineTotal        DECIMAL(18,2)  NOT NULL,
    IsDeleted        BIT            NOT NULL CONSTRAINT DF_ReturnItems_IsDeleted DEFAULT (0),
    CreatedAtUtc     DATETIME2      NOT NULL CONSTRAINT DF_ReturnItems_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc     DATETIME2      NULL,
    CONSTRAINT FK_ReturnItems_Returns FOREIGN KEY (ReturnId)
        REFERENCES dbo.Returns(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ReturnItems_OrderItems FOREIGN KEY (OrderItemId)
        REFERENCES dbo.OrderItems(Id),
    CONSTRAINT FK_ReturnItems_ProductVariants FOREIGN KEY (ProductVariantId)
        REFERENCES dbo.ProductVariants(Id)
);
GO
CREATE INDEX IX_ReturnItems_ReturnId         ON dbo.ReturnItems(ReturnId);
CREATE INDEX IX_ReturnItems_OrderItemId      ON dbo.ReturnItems(OrderItemId);
CREATE INDEX IX_ReturnItems_ProductVariantId ON dbo.ReturnItems(ProductVariantId);
GO

CREATE TABLE dbo.SupportMessages (
    Id              INT            IDENTITY(1,1) NOT NULL CONSTRAINT PK_SupportMessages PRIMARY KEY,
    SupportTicketId INT            NOT NULL,
    AuthorName      NVARCHAR(150)  NOT NULL,
    Body            NVARCHAR(4000) NOT NULL,
    IsStaff         BIT            NOT NULL CONSTRAINT DF_SupportMessages_IsStaff DEFAULT (0),
    IsDeleted       BIT            NOT NULL CONSTRAINT DF_SupportMessages_IsDeleted DEFAULT (0),
    CreatedAtUtc    DATETIME2      NOT NULL CONSTRAINT DF_SupportMessages_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAtUtc    DATETIME2      NULL,
    CONSTRAINT FK_SupportMessages_SupportTickets FOREIGN KEY (SupportTicketId)
        REFERENCES dbo.SupportTickets(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_SupportMessages_SupportTicketId ON dbo.SupportMessages(SupportTicketId);
GO

PRINT 'NK Silk: 32 tables created successfully.';
GO

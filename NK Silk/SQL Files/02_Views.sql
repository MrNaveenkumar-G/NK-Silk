/* ============================================================================
   NK Silk - Views Script  (Microsoft SQL Server)
   ----------------------------------------------------------------------------
   Practical reporting / lookup views derived from the EF Core model.
   Run AFTER 01_Tables.sql. All views exclude soft-deleted rows (IsDeleted = 0)
   and decode the INT enum columns into readable text.
   ============================================================================ */
GO

/* ---------- Product catalog (flattened, live & active) -------------------- */
CREATE OR ALTER VIEW dbo.vw_ProductCatalog
AS
SELECT  p.Id                AS ProductId,
        p.Sku,
        p.Name              AS ProductName,
        p.Slug,
        p.BasePrice,
        p.MrpPrice,
        c.Name              AS CategoryName,
        sc.Name             AS SubCategoryName,
        b.Name              AS BrandName,
        v.Name              AS VendorName,
        p.IsActive,
        p.IsFeatured,
        p.CreatedAtUtc
FROM    dbo.Products       p
JOIN    dbo.Categories     c  ON c.Id  = p.CategoryId
LEFT JOIN dbo.SubCategories sc ON sc.Id = p.SubCategoryId
LEFT JOIN dbo.Brands       b  ON b.Id  = p.BrandId
LEFT JOIN dbo.Vendors      v  ON v.Id  = p.VendorId
WHERE   p.IsDeleted = 0;
GO

/* ---------- Variant stock view (with computed availability) --------------- */
CREATE OR ALTER VIEW dbo.vw_VariantStock
AS
SELECT  pv.Id              AS ProductVariantId,
        pv.Sku             AS VariantSku,
        p.Id               AS ProductId,
        p.Name             AS ProductName,
        col.Name           AS ColorName,
        sz.Name            AS SizeName,
        pv.Price,
        pv.MrpPrice,
        i.QuantityOnHand,
        i.QuantityReserved,
        (ISNULL(i.QuantityOnHand,0) - ISNULL(i.QuantityReserved,0)) AS QuantityAvailable,
        i.ReorderLevel,
        pv.IsActive
FROM    dbo.ProductVariants pv
JOIN    dbo.Products        p   ON p.Id = pv.ProductId
LEFT JOIN dbo.Colors        col ON col.Id = pv.ColorId
LEFT JOIN dbo.Sizes         sz  ON sz.Id  = pv.SizeId
LEFT JOIN dbo.Inventories   i   ON i.ProductVariantId = pv.Id AND i.IsDeleted = 0
WHERE   pv.IsDeleted = 0;
GO

/* ---------- Order summary (header + customer + status text) --------------- */
CREATE OR ALTER VIEW dbo.vw_OrderSummary
AS
SELECT  o.Id            AS OrderId,
        o.OrderNumber,
        o.CreatedAtUtc  AS OrderDateUtc,
        cu.Id           AS CustomerId,
        cu.FullName     AS CustomerName,
        cu.Email        AS CustomerEmail,
        o.SubTotal,
        o.DiscountAmount,
        o.ShippingFee,
        o.TaxAmount,
        o.GrandTotal,
        o.Status        AS StatusCode,
        CASE o.Status
            WHEN 0 THEN 'Pending'        WHEN 1 THEN 'Confirmed'
            WHEN 2 THEN 'Packed'         WHEN 3 THEN 'Shipped'
            WHEN 4 THEN 'OutForDelivery' WHEN 5 THEN 'Delivered'
            WHEN 6 THEN 'Cancelled'      WHEN 7 THEN 'Returned'
            ELSE 'Unknown' END          AS StatusText,
        pay.Status      AS PaymentStatusCode,
        CASE pay.Status
            WHEN 0 THEN 'Pending'   WHEN 1 THEN 'Authorized'
            WHEN 2 THEN 'Paid'      WHEN 3 THEN 'Failed'
            WHEN 4 THEN 'Refunded'  WHEN 5 THEN 'PartiallyRefunded'
            ELSE NULL END           AS PaymentStatusText
FROM    dbo.Orders     o
JOIN    dbo.Customers  cu  ON cu.Id = o.CustomerId
LEFT JOIN dbo.Payments pay ON pay.OrderId = o.Id AND pay.IsDeleted = 0
WHERE   o.IsDeleted = 0;
GO

/* ---------- Order line detail --------------------------------------------- */
CREATE OR ALTER VIEW dbo.vw_OrderLineDetail
AS
SELECT  oi.Id          AS OrderItemId,
        o.OrderNumber,
        oi.OrderId,
        oi.ProductName,
        oi.VariantSku,
        oi.ColorName,
        oi.SizeName,
        oi.Quantity,
        oi.UnitPrice,
        oi.LineTotal
FROM    dbo.OrderItems oi
JOIN    dbo.Orders     o ON o.Id = oi.OrderId
WHERE   oi.IsDeleted = 0 AND o.IsDeleted = 0;
GO

/* ---------- Low-stock alert (at or below reorder level) ------------------- */
CREATE OR ALTER VIEW dbo.vw_LowStock
AS
SELECT  pv.Sku             AS VariantSku,
        p.Name             AS ProductName,
        i.QuantityOnHand,
        i.QuantityReserved,
        (i.QuantityOnHand - i.QuantityReserved) AS QuantityAvailable,
        i.ReorderLevel
FROM    dbo.Inventories    i
JOIN    dbo.ProductVariants pv ON pv.Id = i.ProductVariantId
JOIN    dbo.Products        p  ON p.Id  = pv.ProductId
WHERE   i.IsDeleted = 0
  AND   (i.QuantityOnHand - i.QuantityReserved) <= i.ReorderLevel;
GO

/* ---------- Active offers (currently within their window) ----------------- */
CREATE OR ALTER VIEW dbo.vw_ActiveOffers
AS
SELECT  of.Id,
        of.Title,
        of.Slug,
        CASE of.OfferType WHEN 0 THEN 'PercentageOff' WHEN 1 THEN 'FlatOff' ELSE 'Unknown' END AS OfferType,
        of.Value,
        CASE of.Scope WHEN 0 THEN 'EntireStore' WHEN 1 THEN 'Category' WHEN 2 THEN 'Product' ELSE 'Unknown' END AS Scope,
        c.Name  AS CategoryName,
        p.Name  AS ProductName,
        of.StartsAtUtc,
        of.EndsAtUtc,
        of.Priority
FROM    dbo.Offers      of
LEFT JOIN dbo.Categories c ON c.Id = of.CategoryId
LEFT JOIN dbo.Products   p ON p.Id = of.ProductId
WHERE   of.IsDeleted = 0
  AND   of.IsActive  = 1
  AND   SYSUTCDATETIME() BETWEEN of.StartsAtUtc AND of.EndsAtUtc;
GO

/* ---------- Product rating roll-up (approved reviews only) ---------------- */
CREATE OR ALTER VIEW dbo.vw_ProductRatings
AS
SELECT  p.Id            AS ProductId,
        p.Name          AS ProductName,
        COUNT(r.Id)     AS ReviewCount,
        CAST(AVG(CAST(r.Rating AS DECIMAL(4,2))) AS DECIMAL(4,2)) AS AverageRating
FROM    dbo.Products p
LEFT JOIN dbo.Reviews r ON r.ProductId = p.Id AND r.IsDeleted = 0 AND r.IsApproved = 1
WHERE   p.IsDeleted = 0
GROUP BY p.Id, p.Name;
GO

PRINT 'NK Silk: 7 views created successfully.';
GO

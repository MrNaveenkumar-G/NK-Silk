# NK Silk — Business Requirements Document (BRD)

**Project:** NK Silk Textile E-Commerce Platform
**Version:** 1.0
**Stack:** ASP.NET Core 8 MVC · C# · EF Core · SQL Server · Bootstrap 5 · jQuery
**Status of this document:** Reflects the platform as implemented (foundation + customer commerce + payments + admin) plus the planned roadmap.

---

## 1. Project Overview
NK Silk is an online textile retail platform for sarees, silk and cotton apparel, modelled on leading Indian brands (Ramraj, Chennai Silks, Pothys, Nalli). It enables customers to browse a rich catalogue, manage a cart, register/sign in, check out, and pay (COD or online via Razorpay), while staff manage the catalogue, inventory and orders through an admin back-office.

## 2. Business Goals
| # | Goal | Success measure |
|---|------|-----------------|
| G1 | Sell textile products online across India | Orders placed & fulfilled |
| G2 | Showcase textile-specific attributes (fabric, GSM, wash care, occasion) | Attribute coverage per product |
| G3 | Frictionless buy journey | Browse → cart → checkout → pay in ≤ 5 steps |
| G4 | Efficient back-office | Product/inventory/order management without engineering |
| G5 | India-first payments | Razorpay (UPI/Card/Net Banking) + COD |
| G6 | Scale to 100k+ products, 1M+ customers | Indexed schema, paged queries, layered architecture |

## 3. Customer Types
- **Guest** — browses catalogue and builds a cart (cookie-keyed); must sign in to check out.
- **Registered customer** — account, order history, saved shipping details.
- **Admin/staff** — back-office access (role-gated).
- **Vendor** — third-party sellers managing their own products, inventory and order lines (role-gated `/Vendor`).

## 4. User Roles & Permissions
| Role | Permissions |
|------|-------------|
| Guest | Browse, search, cart |
| Customer | All guest actions + checkout, pay, view own orders, track shipments, raise returns & support tickets, notifications, profile |
| Admin | Dashboard, product/category CRUD, inventory, order status, refunds, returns, shipments, support, reports, customer list |
| Vendor | Seller dashboard, own products (show/hide), own inventory (set stock), own order lines |

## 5. Functional Requirements (implemented ✓ / roadmap ◻)
- ✓ Catalogue: categories, products, variants (colour/size), images
- ✓ Search & category filtering with pagination
- ✓ Product detail with textile spec sheet
- ✓ Cart: add/update/remove via AJAX, persistent (cookie key)
- ✓ Auth: register, login, logout, profile (cookie auth, hashed passwords)
- ✓ Checkout: address capture, order summary, COD + online
- ✓ Orders: placement, inventory deduction, order history, order detail
- ✓ Payments: Razorpay order create + signature-verified capture + refund (simulation fallback in dev)
- ✓ Admin: dashboard, product/category management, inventory, order management, customers
- ✓ Wishlist, Reviews UI, Coupons UI
- ✓ Returns workflow: customer RMA on delivered orders → admin approve/reject/pickup/refund (restock + payment reversal)
- ✓ Notifications: in-app feed + navbar bell; email/SMS dispatched via a pluggable sender (logging simulator in dev)
- ✓ Logistics: per-order shipment, courier/AWB, tracking-event timeline, order-status sync, customer tracking page
- ✓ Customer support: tickets (optionally order-linked), threaded replies, admin status lifecycle
- ✓ Analytics/Reports: revenue, AOV, daily chart, top products/categories, status & returns summary, low-stock, CSV
- ✓ Vendor portal: seller dashboard (commission/payout), vendor-scoped products/inventory/orders (role-gated)
- ✓ Offer/campaign engine: time-boxed %/flat offers (store/category/product) priced consistently across storefront, cart, checkout & API
- ✓ Combo packs: curated bundles at a special price with automatic cart-level bundle saving
- ✓ REST API `/api/v1` (JWT): auth, catalogue, offers and order endpoints for the mobile app
- ✓ RBAC (Roles/CustomerRole) + audit-log trail on key entities
- ✓ SEO: sitemap.xml, robots.txt, canonical/OpenGraph tags, JSON-LD product structured data
- ✓ Payments: gateway factory — Razorpay + PhonePe/UPI/Card/Net-Banking (simulators until live keys)
- ✓ Notifications: SMTP email + HTTP SMS transport (config-gated), falling back to logging in dev
- ✓ REST API: cart, place-order and Swagger UI added (full `/api/v1` surface)
- ✓ Scale: output caching, faceted search service (swappable), CDN media resolver, background worker
- ✓ Accounts: address book, email verification, forgot/reset password
- ✓ Ops: Docker + compose, CI pipeline, `/health` probe, deployment-architecture doc
- ◻ Live gateway credentials; distributed cache (Redis) + search engine + read replicas at very large scale

## 6. Non-Functional Requirements
| Category | Requirement |
|----------|-------------|
| Security | Hashed passwords (PBKDF2), antiforgery on all POSTs, role-based admin, payment signature verification, soft deletes, open-redirect guards |
| Performance | AsNoTracking reads, projected queries, DB indexes on slugs/SKUs/keys, paged listings |
| Scalability | Layered architecture, repository/UoW, stateless web tier (cookie auth) |
| Maintainability | Clean separation (Domain/Application/Infrastructure/Web), DI, EF migrations |
| Usability | Responsive Bootstrap 5 UI, mobile-friendly |
| Availability | Stateless app suitable for horizontal scaling behind a load balancer |

## 7. Textile Industry Requirements
Fabric type, material composition, GSM, wash care, occasion, seasonal collection — modelled as first-class fields on the Product entity. Colour and size are normalised lookups driving per-SKU variants and stock.

## 8. Future Scalability Requirements
- Read replicas + output/response caching for catalogue
- Search service (Azure Cognitive Search / Elasticsearch) for 100k+ SKUs
- CDN for product imagery
- Background workers for notifications, invoicing, analytics
- Vendor multi-tenancy

## 9. Competitor Analysis (summary)
| Brand | Strength | NK Silk parity |
|-------|----------|----------------|
| Ramraj | Cotton menswear, COD trust | COD, category model ✓ |
| Chennai Silks | Wide catalogue, offers | Catalogue + coupons (roadmap) |
| Pothys / Nalli | Premium silk merchandising | Textile attributes, variants ✓ |

## 10. Revenue Model
- Direct product sales (primary)
- *(Roadmap)* vendor commissions, shipping margin, promoted listings, seasonal campaigns.

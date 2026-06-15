# NK Silk — Textile E-Commerce Platform

Production-style textile e-commerce platform (sarees, silk & cotton wear) built on
**ASP.NET Core 8 MVC + EF Core + SQL Server + Bootstrap 5 + jQuery**, inspired by
Ramraj / Chennai Silks / Pothys / Nalli.

This repository is the **runnable foundation** — a clean layered architecture with a
working customer-facing vertical slice (catalogue → product detail → cart). Remaining
modules (auth, checkout, payments, admin, vendor, returns, analytics) build on top of it.

## Architecture (layered / clean)

```
NKSilk.sln
├─ src/
│  ├─ NKSilk.Domain          # Entities + enums (no dependencies)
│  ├─ NKSilk.Application      # Service interfaces, services, ViewModels, repository abstractions
│  ├─ NKSilk.Infrastructure   # EF Core DbContext, configurations, generic Repository + UnitOfWork, seed
│  └─ NKSilk.Web              # MVC controllers, Razor views, Bootstrap 5 UI, wwwroot
└─ tests/
   └─ NKSilk.Tests            # xUnit: PromotionService unit tests + WebApplicationFactory API tests
```

- **Repository + Unit of Work** — `IRepository<T>` / `IUnitOfWork` (Application) implemented over EF Core (Infrastructure).
- **Service layer** — `ICatalogService`, `ICartService` hold use-case logic; controllers stay thin.
- **Dependency Injection** — `AddApplication()` and `AddInfrastructure(config)` extension methods.
- **Soft delete** — every entity derives from `BaseEntity`; a global query filter hides `IsDeleted` rows.
- **Audit stamps** — `CreatedAtUtc` / `UpdatedAtUtc` set automatically in `SaveChangesAsync`.

## Data model

Entities: Category, SubCategory, Brand, Product, ProductImage, ProductVariant, Color, Size,
Inventory, Customer, Address, Cart, CartItem, Order, OrderItem, Payment, Coupon, Review,
WishlistItem, Return, ReturnItem, Notification, Vendor, Shipment, ShipmentEvent, SupportTicket, SupportMessage,
Offer, ComboPack, ComboPackItem, Role, CustomerRole, AuditLog.
Account columns added: address book (`Address.IsDefault`), email-verification & password-reset tokens on `Customer`.
Textile-specific fields live on `Product` (FabricType, MaterialComposition, GSM, WashCare, Occasion, Collection).

## Prerequisites

- .NET 8 SDK
- SQL Server LocalDB **or** SQL Server / Express (update the connection string)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef --version 8.0.8`

## Configuration

Connection string lives in `src/NKSilk.Web/appsettings.json` → `ConnectionStrings:DefaultConnection`.
Default targets LocalDB:

```
Server=(localdb)\MSSQLLocalDB;Database=NKSilkDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

For SQL Express use e.g. `Server=.\SQLEXPRESS;Database=NKSilkDb;Trusted_Connection=True;TrustServerCertificate=True`.

## Run

```powershell
dotnet build
dotnet run --project src/NKSilk.Web
```

## Tests

```powershell
dotnet test            # 14 tests — PromotionService unit tests + API integration tests
```

`NKSilk.Tests` boots the real app via `WebApplicationFactory<Program>` over an in-memory EF store
(no SQL Server needed), exercising health, the storefront, the `/api/v1` surface (incl. JWT login →
orders), search, and sitemap; plus pure unit tests for the offer/combo promotions engine. The same
`dotnet test` runs in CI (`.github/workflows/ci.yml`).

On first launch the app **applies migrations and seeds demo data automatically**
(6 products across Sarees / Men's Wear / Kids). Browse to the printed URL.

### EF migrations (manual)

```powershell
dotnet ef migrations add <Name> --project src/NKSilk.Infrastructure --startup-project src/NKSilk.Web --output-dir Data/Migrations
dotnet ef database update --project src/NKSilk.Infrastructure --startup-project src/NKSilk.Web
```

## What works today

**Storefront**
- Home: hero, shop-by-category, featured products
- Catalogue listing with category filter, search, and pagination
- Product detail: image carousel, variant (colour/size) selector, textile spec sheet
- Cart: AJAX add / update / remove, live navbar badge, antiforgery-protected

**Accounts & buying**
- Register / Login / Logout / Profile (cookie auth, PBKDF2-hashed passwords)
- Checkout: shipping address, order summary, COD or online payment
- Orders: placement with stock deduction, order confirmation, My Orders + order detail
- Payments: Razorpay order-create + signature-verified capture + refund
  (runs in a **dev simulator** until you set real keys — see below)

**Admin portal** (`/Admin`, role-gated)
- Dashboard: product/order/customer counts, revenue, pending & low-stock
- Products (create/edit/show-hide), Categories, Inventory (set stock), Orders (status + refund), Customers
- Returns: review queue with status filters; approve/reject/pickup/refund; pending-returns alert on the dashboard
- Shipments: per-order courier/AWB, tracking-event timeline, auto order-status sync
- Reports: sales analytics + CSV export · Support: ticket queue, threaded replies, status lifecycle
- Offers & Combo Packs (promotion CRUD) · Roles & Audit (RBAC assignment + change-trail viewer)

### Admin login (seeded automatically)
```
Email:    admin@nksilk.com
Password: Admin@123
```

### Enabling real Razorpay
Set keys in `src/NKSilk.Web/appsettings.json` → `Razorpay:KeyId` / `Razorpay:KeySecret`.
With keys present the live gateway + signature verification activate; without them the
checkout uses a built-in simulator so the flow is testable locally.

## Documentation
See [`/docs`](docs): BRD, SRS, ER diagram (Mermaid), and Routes/API design.

**Engagement (latest)**
- Coupons: validated at checkout (% or flat, min-order, cap, expiry, usage limit) + admin CRUD; sample codes `FESTIVE10`, `FLAT200`
- Reviews: customer submits on product page → admin moderation queue → approved reviews show on the storefront
- Wishlist: add/remove (heart toggle) + wishlist page + navbar badge

**Post-purchase**
- Returns: customer raises an RMA against a *delivered* order (select items/quantities + reason) → admin
  approve / reject / mark picked-up / refund. Refund restocks inventory, reverses the payment
  (gateway when live), and moves the order → Returned. Partial and repeat returns supported.
- Notifications: in-app feed + navbar bell (unread badge) for order placed, status changes, payment
  received, return, shipment and support events. Each event also dispatches an email/SMS copy through
  `INotificationSender` (a logging simulator in dev — swap for SMTP/SendGrid/Twilio in production).

**Logistics, Support, Analytics & Marketplace**
- Logistics: admin creates a shipment (courier + AWB) per order and posts tracking events; each event
  syncs the order status (Packed → Shipped → Out-for-delivery → Delivered) and notifies the customer.
  Customers get a tracking-history timeline at `/Tracking/Order/{number}`.
- Customer Support: customers raise tickets (optionally tied to an order), thread replies with staff;
  admin queue with status lifecycle (Open → AwaitingCustomer → Resolved → Closed); staff replies notify.
- Analytics / Reports (`/Admin/Reports`): revenue, order count, AOV, daily-revenue chart, top products,
  revenue by category, status breakdown, returns summary, low-stock list, and CSV export.
- Vendor Portal (`/Vendor`, role-gated): seller dashboard (sales, commission, net payout), vendor-scoped
  product visibility, inventory editing, and order-line view. Demo seller: `seller@nksilk.com` / `Seller@123`.

**Promotions, API, RBAC & SEO**
- Offers: time-boxed % / flat campaigns scoped to the whole store, a category, or a product, with banners.
  A shared promotions engine prices offers identically on product cards, the cart, checkout and the API.
- Combo Packs: curated bundles at a special price; "Add combo to cart" drops the components in and the
  bundle saving is applied automatically when all are present. Admin CRUD + storefront browse/detail.
- REST API (`/api/v1`, JWT): `auth/register|login` issue a bearer token; `products`, `products/{slug}`,
  `categories`, `offers` are public; `orders` is token-secured. `{ data, error }` envelope.
- RBAC + Audit: a `Roles`/`CustomerRole` model (Admin/Vendor/Customer) drives login claims; admin assigns
  roles per customer. Every create/update/delete on key entities is written to an `AuditLog` trail (admin viewer).
- SEO: `/sitemap.xml`, `/robots.txt`, canonical + OpenGraph tags and JSON-LD Product structured data.

**Platform, payments, scale & ops**
- Payments: a gateway **factory** selects per method — Razorpay (live/simulated) plus PhonePe, UPI,
  Card and Net-Banking simulators behind the same contract; checkout offers all options.
- Notifications: `ConfigurableNotificationSender` sends real **SMTP email** and **HTTP SMS** when
  `Email:SmtpHost` / `Sms:ApiUrl` are configured, else logs (dev). No app-code change to go live.
- REST API: cart (`GET`/`POST`/`PUT`/`DELETE /api/v1/cart/items`), `POST /api/v1/orders`, and
  **Swagger UI** at `/swagger`; enums (de)serialize as strings.
- Scale: **output caching** on public API reads, an `ISearchService` (SQL-backed, faceted; swappable
  for Elasticsearch) at `GET /api/v1/search`, a CDN media-URL resolver (`Cdn:BaseUrl`), and a
  **background worker** (low-stock monitor → admin notifications).
- Accounts: **address book** (saved addresses, default, prefilled at checkout), **email verification**,
  and **forgot/reset password**.
- Ops: `Dockerfile` + `docker-compose.yml` (web + SQL Server), GitHub Actions **CI**, `/health` probe,
  and a deployment-architecture doc.

## Roadmap (remaining from the spec)
- Live gateway credentials (PhonePe/UPI/card/net-banking currently run as simulators)
- Distributed cache (Redis) + dedicated search engine + read replicas at very large scale
- Payment callback over the REST API (web checkout callback is implemented)

### Demo logins
```
Admin:  admin@nksilk.com  / Admin@123
Seller: seller@nksilk.com / Seller@123
```
```

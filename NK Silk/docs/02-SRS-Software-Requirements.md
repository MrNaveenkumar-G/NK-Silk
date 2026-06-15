# NK Silk — Software Requirements Specification (SRS)

**Version:** 1.0 · **Stack:** ASP.NET Core 8 MVC, EF Core, SQL Server, Bootstrap 5, jQuery

---

## 1. Introduction
### 1.1 Purpose
Defines functional and non-functional software requirements for the NK Silk platform.
### 1.2 Scope
Customer storefront, authentication, cart, checkout, orders, payments, and an admin back-office. Built as a layered ASP.NET Core solution.
### 1.3 Definitions
- **Variant (SKU):** product + colour + size with its own price and stock.
- **Cart key:** opaque GUID cookie identifying a guest cart.
- **Soft delete:** `IsDeleted` flag filtered globally; rows are never physically removed.

## 2. Overall Description
### 2.1 Architecture
```
NKSilk.Web  →  NKSilk.Application  →  NKSilk.Domain
      \              ↑
       → NKSilk.Infrastructure (EF Core, repositories, payment gateway)
```
Patterns: Repository + Unit of Work, Service layer, Dependency Injection, Areas (Admin).

### 2.2 User Classes
Guest, Registered Customer, Admin (role claim), Vendor (role claim + VendorId claim).

### 2.3 Operating Environment
Windows/Linux, .NET 8 runtime, SQL Server / LocalDB, modern browsers.

## 3. Functional Requirements (by module)

### FR-1 Catalogue
- FR-1.1 List active categories with product counts.
- FR-1.2 List products with pagination (default 12/page), category filter, keyword search (name/fabric/occasion).
- FR-1.3 Product detail: images, variants, textile attributes, rating summary.

### FR-2 Cart
- FR-2.1 Add a variant (qty ≥ 1); merge quantity if line exists.
- FR-2.2 Update line quantity (0 ⇒ remove); remove line.
- FR-2.3 Return live item count for the navbar badge.
- FR-2.4 Persist cart by cookie key across sessions.

### FR-3 Authentication
- FR-3.1 Register (unique email, password ≥ 6 chars, confirm match).
- FR-3.2 Login with generic failure message (no account enumeration).
- FR-3.3 Logout; protected pages redirect anonymous users to login.
- FR-3.4 Issue `Admin` role claim for admin accounts.

### FR-4 Checkout & Orders
- FR-4.1 Require authentication; capture shipping address + payment method.
- FR-4.2 Validate stock before placing; never partially fulfil.
- FR-4.3 Snapshot line data; deduct inventory; empty cart; generate order number.
- FR-4.4 COD ⇒ Confirmed; online ⇒ Pending → gateway.
- FR-4.5 Customer can view own order list and detail (scoped by customer id).

### FR-5 Payments
- FR-5.1 Create a gateway order for unpaid orders (Razorpay REST; simulated when no keys).
- FR-5.2 Verify callback signature (HMAC-SHA256) before marking Paid + Confirmed.
- FR-5.3 Refund a paid order (gateway call; status → Refunded).

### FR-6 Admin
- FR-6.1 Dashboard: product/order/customer counts, revenue, pending, low-stock, pending returns, recent orders.
- FR-6.2 Product CRUD (create seeds a default variant + inventory); show/hide toggle.
- FR-6.3 Category create/edit.
- FR-6.4 Inventory: view & set on-hand and reorder level per variant.
- FR-6.5 Orders: filter by status, view detail, update status, refund.
- FR-6.6 Customers: searchable list with order counts.
- FR-6.7 Returns: filter by status, view detail, approve/reject (with note), mark picked up, issue refund.

### FR-7 Returns
- FR-7.1 A return can be raised only against a **Delivered** order, for items not already fully returned.
- FR-7.2 Customer selects line quantities (≤ remaining) and a reason; system computes the refund amount.
- FR-7.3 Lifecycle: Requested → Approved/Rejected → PickedUp → Refunded (admin-driven).
- FR-7.4 Refund restocks the returned units, reverses the payment (gateway when live; manual for COD),
  moves the order → Returned, and marks payment Refunded/PartiallyRefunded.
- FR-7.5 Partial and repeat returns are supported (remaining quantity tracked per order line).

### FR-8 Notifications
- FR-8.1 Order/payment/return events create a per-customer in-app notification (title, message, deep link).
- FR-8.2 Navbar bell shows the unread count; the notifications page lists the latest 50; items mark read on open.
- FR-8.3 Each notification is also dispatched as email/SMS via `INotificationSender`; transport failures never
  roll back the originating use case (the in-app record is the source of truth).
- FR-8.4 The dev build uses a logging sender; production swaps in a real SMTP/SMS provider without app changes.

### FR-9 Logistics
- FR-9.1 Admin creates one shipment per order (courier + tracking number + optional ETA); blocked for unpaid/cancelled orders.
- FR-9.2 Admin appends tracking events (LabelCreated → PickedUp → InTransit → OutForDelivery → Delivered / Failed).
- FR-9.3 Each event syncs the order status (Packed/Shipped/OutForDelivery/Delivered) and notifies the customer.
- FR-9.4 Customers view a read-only tracking timeline scoped to their own orders.

### FR-10 Customer Support
- FR-10.1 Customer opens a ticket (subject, category, optional order link, first message); order link is validated to the customer.
- FR-10.2 Threaded replies from customer and staff; a customer reply re-opens the ticket, a staff reply sets AwaitingCustomer.
- FR-10.3 Admin status lifecycle: Open → AwaitingCustomer → Resolved → Closed; closed tickets reject further replies.
- FR-10.4 Staff replies and status changes notify the customer.

### FR-11 Reports & Analytics
- FR-11.1 Sales report over a 7/30/90-day window: total revenue, order count, average order value.
- FR-11.2 Daily revenue series (chart), top products, revenue by category, order-status breakdown, returns summary, low-stock list.
- FR-11.3 CSV export of the daily series and top products. Revenue excludes cancelled orders.

### FR-12 Vendor Portal
- FR-12.1 Vendor authenticates via the same cookie scheme with a `Vendor` role claim + `VendorId` claim; `/Vendor` is role-gated.
- FR-12.2 Every vendor query is scoped to `VendorId`; a vendor cannot read or mutate another seller's data.
- FR-12.3 Dashboard: product counts, units sold, gross sales, commission (rate × gross) and net payout.
- FR-12.4 Vendor manages own product visibility and inventory (set on-hand/reorder), and views order lines for own products.

### FR-13 Offers & Promotions
- FR-13.1 Admin CRUD for time-boxed offers: percentage or flat discount scoped to the store, a category, or a product, with priority.
- FR-13.2 A single promotions engine resolves the best active offer per product and any matched combo savings.
- FR-13.3 Offer pricing is applied identically on product cards, product detail, the cart, checkout/order totals and the REST API.

### FR-14 Combo Packs
- FR-14.1 Admin CRUD for bundles (component products + quantities + combo price).
- FR-14.2 Storefront browse/detail shows the bundle, regular price and saving; "Add combo to cart" adds the components.
- FR-14.3 When every component is present in the cart, the bundle saving is applied automatically at cart/checkout.

### FR-15 REST API (`/api/v1`)
- FR-15.1 `POST auth/register|login` validate credentials and return a signed JWT (HMAC-SHA256) with role claims.
- FR-15.2 Public: `GET products` (paged/filter), `products/{slug}`, `categories`, `offers` — offer prices included.
- FR-15.3 Secured (Bearer): `GET orders`, `orders/{number}` scoped to the token's customer. Envelope `{ data, error }`.

### FR-16 RBAC & Audit
- FR-16.1 Roles (Admin/Vendor/Customer) are modelled in `Roles`/`CustomerRole`; login emits a role claim per assignment.
- FR-16.2 Admin can view roles and assign/revoke a customer's roles; changes take effect at next sign-in.
- FR-16.3 Every create/update/delete on audited entities (Product, Order, Coupon, Offer, ComboPack, Return, Shipment, Customer, Vendor, Inventory, Category) is written to `AuditLog` with actor + changed fields.

### FR-17 SEO
- FR-17.1 `/sitemap.xml` lists home, catalogue, categories, products, offers and combos; `/robots.txt` references it and disallows back-office paths.
- FR-17.2 Product pages emit a canonical link, OpenGraph tags and JSON-LD `Product` (with offer price, availability and aggregate rating).

### FR-18 Payments (multi-gateway)
- FR-18.1 An `IPaymentGatewayFactory` resolves the gateway per `PaymentMethod`: Razorpay (live or simulated) and PhonePe/UPI/Card/Net-Banking simulators.
- FR-18.2 Checkout presents COD plus the online methods; online methods run the create-order → callback → capture flow against the resolved gateway.
- FR-18.3 Adding a live provider is a drop-in implementation of `IPaymentGateway` registered in the factory — no caller changes.

### FR-19 Notification transport
- FR-19.1 `ConfigurableNotificationSender` sends real email over SMTP when `Email:SmtpHost` is set, and SMS via an HTTP gateway when `Sms:ApiUrl` is set.
- FR-19.2 With neither configured it logs payloads (dev simulator). Transport failures are swallowed and never roll back the originating action.

### FR-20 REST API completeness & docs
- FR-20.1 Cart over the API: `GET /api/v1/cart`, `POST /cart/items`, `PUT /cart/items/{id}`, `DELETE /cart/items/{id}` (Bearer; per-customer API cart).
- FR-20.2 `POST /api/v1/orders` places an order from the API cart; `GET /api/v1/search` returns faceted results.
- FR-20.3 OpenAPI/Swagger UI at `/swagger`; enums (de)serialize as strings; envelope `{ data, error }`.

### FR-21 Scale & performance
- FR-21.1 Output caching (60s, vary-by-query) on public API catalogue/offer reads.
- FR-21.2 `ISearchService` abstraction with a SQL-backed faceted implementation; swappable for a search engine.
- FR-21.3 `IMediaUrlResolver` rewrites asset paths to `Cdn:BaseUrl` when configured.
- FR-21.4 A background `IHostedService` periodically flags low stock to admins (seam for invoicing/analytics workers).

### FR-22 Account self-service
- FR-22.1 Address book: CRUD of saved addresses with a default; checkout lists them and prefills the form.
- FR-22.2 Email verification: a token is issued on registration and confirmed via `/Account/VerifyEmail`.
- FR-22.3 Forgot/reset password: a time-limited token is emailed and redeemed at `/Account/ResetPassword`.

### FR-23 Operations
- FR-23.1 Containerized (Dockerfile + docker-compose with SQL Server); `/health` liveness/readiness probe.
- FR-23.2 GitHub Actions CI (restore → build → test → image); deployment architecture documented in `/docs/05`.

## 4. Non-Functional Requirements
- **Security:** PBKDF2 password hashing, antiforgery tokens, role authorisation, signature verification, HTTPS, soft delete, local-redirect validation.
- **Performance:** projected/no-tracking queries, indexed lookups, pagination.
- **Reliability:** order placement atomic within a single SaveChanges.
- **Maintainability:** layer boundaries enforced by project references.
- **Portability:** EF Core Code-First migrations; LocalDB or full SQL Server.

## 5. External Interfaces
- **Razorpay REST API** (`/v1/orders`, `/v1/payments/{id}/refund`) via Basic auth.
- **SQL Server** via EF Core.

## 6. Acceptance Criteria (verified)
Register→login→cart→checkout(COD)→confirmation→order history; online checkout→gateway→capture→Paid; admin login→manage catalogue/inventory/orders. All exercised end-to-end.

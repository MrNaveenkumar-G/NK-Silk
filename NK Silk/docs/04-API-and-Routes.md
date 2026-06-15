# NK Silk — Routes & API Design

The app is server-rendered MVC; cart and payment use AJAX/JSON endpoints. This documents the current HTTP surface (REST-style where it returns JSON) plus the planned pure-REST API.

Conventions: all mutating endpoints are `POST` and require an antiforgery token (`__RequestVerificationToken`). Protected endpoints require the auth cookie; admin endpoints require the `Admin` role.

---

## 1. Storefront (server-rendered)
| Method | Route | Purpose | Auth |
|--------|-------|---------|------|
| GET | `/` | Home: categories + featured | — |
| GET | `/Catalog?category=&search=&page=` | Product listing (paged) | — |
| GET | `/Catalog/Details/{slug}` | Product detail | — |
| GET | `/Cart` | Cart page | — |

## 2. Cart (AJAX / JSON)
| Method | Route | Body | Response |
|--------|-------|------|----------|
| POST | `/Cart/Add` | productVariantId, quantity | `{success, itemCount, subTotal}` or `{success:false, message}` |
| POST | `/Cart/Update` | cartItemId, quantity | `{success, itemCount, subTotal}` |
| POST | `/Cart/Remove` | cartItemId | `{success, itemCount, subTotal}` |
| GET | `/Cart/Count` | — | `{itemCount}` |

**Validation/errors:** unknown/inactive variant ⇒ `{success:false, message}`; quantity < 1 normalised to 1 (add) or treated as remove (update).

## 3. Account
| Method | Route | Body | Notes |
|--------|-------|------|-------|
| GET/POST | `/Account/Register` | FullName, Email, PhoneNumber, Password, ConfirmPassword | Unique email; auto sign-in on success |
| GET/POST | `/Account/Login` | Email, Password, RememberMe | Generic failure message |
| POST | `/Account/Logout` | — | Requires auth |
| GET | `/Account/Profile` | — | Requires auth |
| GET | `/Account/VerifyEmail?token=` | — | Confirms an email-verification token |
| GET/POST | `/Account/ForgotPassword` | Email | Emails a reset link (silent if unknown) |
| GET/POST | `/Account/ResetPassword?token=` | Token, Password, Confirm | Redeems a time-limited reset token |
| GET | `/AddressBook` · GET/POST `/AddressBook/Edit/{id?}` · POST `/Delete` · `/SetDefault` | — | Saved-address book (auth) |

## 4. Checkout & Orders
| Method | Route | Body | Notes |
|--------|-------|------|-------|
| GET | `/Checkout` | — | Requires auth; redirects to Cart if empty |
| POST | `/Checkout/Place` | ContactName, PhoneNumber, Line1/2, City, State, PostalCode, PaymentMethod | COD ⇒ confirmation; online ⇒ `/Payment/Pay` |
| GET | `/Checkout/Confirmation?orderNumber=` | — | Customer-scoped |
| GET | `/Orders` | — | My orders |
| GET | `/Orders/Details/{orderNumber}` | — | Customer-scoped; shows a Return-items action when Delivered |

## 4a. Returns (requires auth, customer-scoped)
| Method | Route | Body | Notes |
|--------|-------|------|-------|
| GET | `/Returns` | — | My returns |
| GET | `/Returns/Details/{returnNumber}` | — | Customer-scoped |
| GET | `/Returns/Create/{orderNumber}` | — | Return form; 302 to order if order ineligible |
| POST | `/Returns/Create` | OrderNumber, Reason, Comments, Quantities[orderItemId] | Validates remaining qty; creates RMA |

## 4b. Notifications (requires auth, customer-scoped)
| Method | Route | Body | Notes |
|--------|-------|------|-------|
| GET | `/Notifications` | — | Latest 50; bell links here |
| GET | `/Notifications/Open/{id}` | — | Marks read, redirects to deep link (local-URL guarded) |
| POST | `/Notifications/MarkAllRead` | — | Marks all read |
| GET | `/Notifications/Count` | — | `{count}` — polled by the navbar bell badge |

## 4c. Tracking & Support (requires auth, customer-scoped)
| Method | Route | Body | Notes |
|--------|-------|------|-------|
| GET | `/Tracking/Order/{orderNumber}` | — | Shipment tracking timeline for own order |
| GET | `/Support` | — | My tickets |
| GET | `/Support/Details/{ticketNumber}` | — | Ticket thread |
| GET/POST | `/Support/Create` | Subject, Category, OrderNumber?, Message | Order link validated to the customer |
| POST | `/Support/Reply` | ticketNumber, body | Re-opens the ticket |

## 5. Payments
| Method | Route | Body | Notes |
|--------|-------|------|-------|
| GET | `/Payment/Pay?orderNumber=` | — | Renders Razorpay widget or dev simulator |
| POST | `/Payment/Callback` | orderNumber, razorpay_payment_id, razorpay_order_id, razorpay_signature | Verifies signature (live), marks Paid+Confirmed |

**Signature:** `HMAC_SHA256(razorpay_order_id + "|" + razorpay_payment_id, KeySecret)` compared in constant time.

## 6. Admin (area `/Admin`, role = Admin)
| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/Admin/Dashboard` | KPIs + recent orders |
| GET | `/Admin/Products?search=` | Product list |
| GET/POST | `/Admin/Products/Edit/{id?}` | Create/edit product |
| POST | `/Admin/Products/Toggle` | Show/hide product |
| GET | `/Admin/Categories` · POST `/Admin/Categories/Save` | List / create-edit |
| GET | `/Admin/Inventory?search=` · POST `/Admin/Inventory/Update` | View / set stock |
| GET | `/Admin/Orders?status=` · GET `/Admin/Orders/Details/{orderNumber}` | List / detail |
| POST | `/Admin/Orders/UpdateStatus` | Change order status (notifies customer) |
| POST | `/Admin/Orders/Refund` | Refund a paid order |
| GET | `/Admin/Returns?status=` · GET `/Admin/Returns/Details/{returnNumber}` | List / detail |
| POST | `/Admin/Returns/Approve` · `/Reject` (note) · `/PickedUp` · `/Refund` | Drive the return lifecycle |
| GET | `/Admin/Shipments?status=` · GET `/Admin/Shipments/Manage/{orderNumber}` | Shipment list / manage |
| POST | `/Admin/Shipments/Create` · `/AddEvent` | Create shipment / append tracking event |
| GET | `/Admin/Reports?days=` · GET `/Admin/Reports/Export?days=` | Sales analytics / CSV |
| GET | `/Admin/Support?status=` · GET `/Admin/Support/Details/{ticketNumber}` | Ticket queue / detail |
| POST | `/Admin/Support/Reply` · `/SetStatus` | Staff reply / change status |
| GET | `/Admin/Customers?search=` | Customer list (Manage access → RBAC) |
| GET | `/Admin/Offers` · GET/POST `/Admin/Offers/Edit/{id?}` · POST `/Toggle` · `/Delete` | Offer campaign CRUD |
| GET | `/Admin/Combos` · GET/POST `/Admin/Combos/Edit/{id?}` · POST `/Toggle` | Combo-pack CRUD |
| GET | `/Admin/Access/Roles` · GET `/Admin/Access/Customer/{id}` · POST `/SetRoles` | RBAC role assignment |
| GET | `/Admin/Access/Audit?entity=` | Audit-trail viewer |

## 6a. Vendor (area `/Vendor`, role = Vendor; every query scoped to VendorId)
| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/Vendor/Dashboard` | Sales, commission & payout summary |
| GET | `/Vendor/Products` · POST `/Vendor/Products/Toggle` | Own products / show-hide |
| GET | `/Vendor/Inventory` · POST `/Vendor/Inventory/Update` | Own stock / set on-hand & reorder |
| GET | `/Vendor/Orders` | Order lines for own products |

## 6b. Storefront promotions & SEO (anonymous)
| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/Offers` | Active offers / campaigns |
| GET | `/Combos` · `/Combos/Details/{slug}` | Combo browse / detail |
| POST | `/Combos/AddToCart` | Add a bundle's components to the cart |
| GET | `/sitemap.xml` · `/robots.txt` | SEO discovery |

## 7. Standard error handling
- `404` for unknown product slug / order number.
- `302 → /Account/Login` for anonymous access to protected/admin routes.
- Validation errors re-render the form with `ModelState` messages.
- AJAX endpoints return `{success:false, message}` rather than HTTP error codes for business failures.

## 8. Pure-REST API (`/api/v1`, JWT-secured) — implemented
Interactive docs: **`/swagger`** (OpenAPI 3.0, with an Authorize button for the bearer token).
```
POST   /api/v1/auth/register | /auth/login   → { token, expiresAtUtc, customerId, fullName, roles[] }
GET    /api/v1/products?category=&search=&page=&pageSize=   (offerPrice included; output-cached)
GET    /api/v1/products/{slug}
GET    /api/v1/categories
GET    /api/v1/offers
GET    /api/v1/search?q=&category=&page=&pageSize=          → results + category facets
GET    /api/v1/cart                           [Bearer]  per-customer API cart
POST   /api/v1/cart/items     { variantId, qty }           [Bearer]
PUT    /api/v1/cart/items/{cartItemId}  { qty }            [Bearer]
DELETE /api/v1/cart/items/{cartItemId}                     [Bearer]
POST   /api/v1/orders   { contactName, phoneNumber, line1.., paymentMethod }  [Bearer]
GET    /api/v1/orders | /orders/{orderNumber}              [Bearer]  customer-scoped
```
- **Auth:** bearer JWT (HMAC-SHA256), issuer/audience/key from `appsettings → Jwt`; role claims embedded.
- **Envelope:** `{ "data": ..., "error": null }`; errors `{ "data": null, "error": { "code", "message" } }`.
- **Enums:** (de)serialized as strings (e.g. `"paymentMethod": "CashOnDelivery"`, `"status": "Confirmed"`).
- **Codes:** `401` (missing/invalid token), `404` (`not_found`), `400` (`validation`/business), `200` otherwise.

*Roadmap:* the payment callback over JWT (web-checkout callback is implemented).

## 9. Ops endpoints
| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/health` | Liveness/readiness probe (`Healthy`) |
| GET | `/swagger` | OpenAPI UI for `/api/v1` |

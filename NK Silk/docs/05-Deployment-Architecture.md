# NK Silk — Deployment Architecture (Phase 7)

## 1. Runtime topology
```
                   ┌────────────┐
   Internet  ───▶  │   CDN      │  product imagery / static assets (Cdn:BaseUrl)
                   └─────┬──────┘
                         │ cache miss
                   ┌─────▼──────┐     health: GET /health
   Internet  ───▶  │  Reverse   │ ───▶ ┌──────────────┐   ┌──────────────┐
                   │  proxy /   │      │  NKSilk.Web   │ × N (stateless)  │
                   │  load bal. │ ───▶ │  (container)  │   │  (container)  │
                   └────────────┘      └──────┬───────┘   └──────┬───────┘
                                              │  EF Core           │
                                       ┌──────▼───────┐    ┌───────▼──────┐
                                       │ SQL Server   │───▶│ read replica │ (optional)
                                       │  (primary)   │    └──────────────┘
                                       └──────────────┘
   Background worker (in-process IHostedService): low-stock monitor, future invoicing/analytics.
   External (config-gated): Razorpay, SMTP/SendGrid, SMS gateway, search engine.
```

The web tier is **stateless** (cookie auth + DB-backed cart), so it scales horizontally behind a
load balancer. Session/cart state lives in SQL Server, not in-process.

## 2. Containerization
- **Dockerfile** — multi-stage (`sdk` build → `aspnet` runtime), listens on `:8080`, `HEALTHCHECK` hits `/health`.
- **docker-compose.yml** — `web` + `db` (SQL Server 2022) with a healthcheck gate and a persistent volume.
  Migrations + seeders run automatically on startup.

```bash
docker compose up --build        # app on http://localhost:8080
```

## 3. Configuration (env vars / appsettings)
| Key | Purpose | Default behaviour if unset |
|-----|---------|----------------------------|
| `ConnectionStrings__DefaultConnection` | SQL Server | LocalDB (dev) |
| `Jwt__Key` / `Jwt__Issuer` / `Jwt__Audience` | REST API token signing | dev key |
| `Razorpay__KeyId` / `Razorpay__KeySecret` | live payments | simulator |
| `Email__SmtpHost` (+ Port/User/Password/From) | real email | logs to console |
| `Sms__ApiUrl` (+ ApiKey/Sender) | real SMS | logs to console |
| `Cdn__BaseUrl` | serve assets via CDN | local paths |

## 4. Health, scaling & resilience
- **Health probe:** `GET /health` for liveness/readiness (k8s `livenessProbe`/`readinessProbe`).
- **Horizontal scale:** run N web replicas; the LB distributes traffic. No sticky sessions required.
- **Read replicas:** point read-heavy catalogue queries at a replica connection (seam: a read-only
  connection string + a replica `DbContext`); writes always go to the primary.
- **Caching:** output caching on the public API (`/api/v1` catalogue/offers); add a distributed cache
  (Redis) for multi-instance output/response cache sharing.
- **Search at scale:** swap `ISearchService` (`SqlSearchService`) for an Elasticsearch / Azure Cognitive
  Search implementation; the contract is unchanged.

## 5. CI/CD
- **.github/workflows/ci.yml** — restore → build (Release) → test → build container image on every push/PR.
- Promote the image to a registry and deploy to the target (App Service / AKS / ECS) with the env vars above.
- DB migrations apply automatically at startup; for zero-downtime, run `dotnet ef database update`
  as a pre-deploy step instead and disable startup auto-migrate in production.

## 6. Security posture (recap)
HTTPS/HSTS, PBKDF2 password hashing, antiforgery on all web POSTs, JWT (HMAC-SHA256) for the API,
role-based authorization (RBAC), payment signature verification, soft deletes, audit-log trail,
and a robots.txt that disallows `/Admin`, `/Vendor`, `/Cart`, `/Checkout`.

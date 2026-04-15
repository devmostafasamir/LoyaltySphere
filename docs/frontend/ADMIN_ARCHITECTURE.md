# 🏗️ Admin Dashboard Architecture

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         FRONTEND (Angular 18)                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐ │
│  │   Navigation     │  │  Admin Dashboard │  │  Campaigns   │ │
│  │   Component      │  │    Component     │  │  Component   │ │
│  │                  │  │                  │  │              │ │
│  │  - Sidebar       │  │  - Key Metrics   │  │  - Grid View │ │
│  │  - Route Links   │  │  - Tier Chart    │  │  - Create    │ │
│  │  - User Profile  │  │  - Transactions  │  │  - Toggle    │ │
│  └──────────────────┘  └──────────────────┘  └──────────────┘ │
│           │                      │                     │         │
│           └──────────────────────┼─────────────────────┘         │
│                                  │                               │
│                    ┌─────────────▼──────────────┐               │
│                    │     Admin Service          │               │
│                    │   (Reactive State)         │               │
│                    │                            │               │
│                    │  - campaigns: signal()     │               │
│                    │  - analytics: signal()     │               │
│                    │  - loading: signal()       │               │
│                    │  - HTTP methods            │               │
│                    └─────────────┬──────────────┘               │
│                                  │                               │
└──────────────────────────────────┼───────────────────────────────┘
                                   │
                                   │ HTTP/JSON
                                   │
┌──────────────────────────────────▼───────────────────────────────┐
│                      API GATEWAY / MIDDLEWARE                     │
├───────────────────────────────────────────────────────────────────┤
│  - Tenant Resolution (X-Tenant-Id header)                        │
│  - JWT Authentication                                             │
│  - Authorization (Admin/TenantAdmin roles)                        │
│  - Exception Handling                                             │
└──────────────────────────────────┬───────────────────────────────┘
                                   │
┌──────────────────────────────────▼───────────────────────────────┐
│                    BACKEND (.NET 8 Web API)                       │
├───────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              AdminController                                 │ │
│  │  [Authorize(Roles = "Admin,TenantAdmin")]                   │ │
│  ├─────────────────────────────────────────────────────────────┤ │
│  │                                                              │ │
│  │  Campaign Management:                                        │ │
│  │  ├─ GET    /api/v1/admin/campaigns                         │ │
│  │  ├─ GET    /api/v1/admin/campaigns/{id}                    │ │
│  │  ├─ POST   /api/v1/admin/campaigns                         │ │
│  │  ├─ POST   /api/v1/admin/campaigns/{id}/activate           │ │
│  │  └─ POST   /api/v1/admin/campaigns/{id}/deactivate         │ │
│  │                                                              │ │
│  │  Reward Rules:                                               │ │
│  │  ├─ GET    /api/v1/admin/reward-rules                      │ │
│  │  ├─ POST   /api/v1/admin/reward-rules                      │ │
│  │  ├─ PUT    /api/v1/admin/reward-rules/{id}                 │ │
│  │  ├─ POST   /api/v1/admin/reward-rules/{id}/activate        │ │
│  │  └─ POST   /api/v1/admin/reward-rules/{id}/deactivate      │ │
│  │                                                              │ │
│  │  Analytics:                                                  │ │
│  │  └─ GET    /api/v1/admin/analytics/dashboard               │ │
│  │                                                              │ │
│  └──────────────────────────┬───────────────────────────────────┘ │
│                             │                                     │
│                             │                                     │
│  ┌──────────────────────────▼──────────────────────────────────┐ │
│  │              ApplicationDbContext (EF Core)                  │ │
│  │                                                              │ │
│  │  - Campaigns DbSet                                           │ │
│  │  - RewardRules DbSet                                         │ │
│  │  - Customers DbSet                                           │ │
│  │  - Rewards DbSet                                             │ │
│  │  - TenantInterceptor (automatic tenant_id filtering)        │ │
│  └──────────────────────────┬───────────────────────────────────┘ │
│                             │                                     │
└─────────────────────────────┼─────────────────────────────────────┘
                              │
┌─────────────────────────────▼─────────────────────────────────────┐
│                    DATABASE (PostgreSQL)                          │
├───────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────┐  ┌──────────┐ │
│  │  campaigns  │  │ reward_rules │  │customers │  │ rewards  │ │
│  ├─────────────┤  ├──────────────┤  ├──────────┤  ├──────────┤ │
│  │ id          │  │ id           │  │ id       │  │ id       │ │
│  │ tenant_id   │  │ tenant_id    │  │tenant_id │  │tenant_id │ │
│  │ name        │  │ rule_name    │  │cust_id   │  │cust_id   │ │
│  │ type        │  │ points_per   │  │points    │  │points    │ │
│  │ bonus_pts   │  │ priority     │  │tier      │  │type      │ │
│  │ multiplier  │  │ is_active    │  │is_active │  │amount    │ │
│  │ cashback_%  │  │ ...          │  │...       │  │...       │ │
│  │ start_date  │  └──────────────┘  └──────────┘  └──────────┘ │
│  │ end_date    │                                                 │
│  │ is_active   │  Row-Level Security (RLS) Policies:            │
│  │ ...         │  - tenant_id = current_setting('app.tenant')   │
│  └─────────────┘  - Automatic filtering per tenant              │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
```

## Data Flow Diagrams

### 1. View Analytics Dashboard

```
User → Navigation → Admin Dashboard Component
                           ↓
                    Admin Service
                    (getDashboardAnalytics)
                           ↓
                    HTTP GET /api/v1/admin/analytics/dashboard
                           ↓
                    Tenant Middleware
                    (extracts tenant_id)
                           ↓
                    AdminController
                    (GetDashboardAnalytics)
                           ↓
                    ApplicationDbContext
                    (queries with tenant filter)
                           ↓
                    PostgreSQL
                    (RLS applies tenant_id filter)
                           ↓
                    Returns: DashboardAnalyticsResponse
                           ↓
                    Admin Service
                    (updates analytics signal)
                           ↓
                    Admin Dashboard Component
                    (UI updates automatically)
```

### 2. Create Campaign

```
User → Campaigns Component → Fill Form → Submit
                                          ↓
                                   Admin Service
                                   (createCampaign)
                                          ↓
                            HTTP POST /api/v1/admin/campaigns
                            Body: CreateCampaignRequest
                                          ↓
                                   Tenant Middleware
                                   (extracts tenant_id)
                                          ↓
                                   AdminController
                                   (CreateCampaign)
                                          ↓
                                   Campaign.Create()
                                   (domain entity factory)
                                          ↓
                                   ApplicationDbContext
                                   (adds campaign with tenant_id)
                                          ↓
                                   PostgreSQL
                                   (inserts with RLS)
                                          ↓
                                   Returns: CampaignResponse
                                          ↓
                                   Admin Service
                                   (updates campaigns signal)
                                          ↓
                                   Campaigns Component
                                   (new campaign appears in grid)
                                          ↓
                                   Toast Notification
                                   ("Campaign created successfully!")
```

### 3. Toggle Campaign Status

```
User → Campaigns Component → Click Toggle Button
                                     ↓
                              Admin Service
                              (activateCampaign or deactivateCampaign)
                                     ↓
                   HTTP POST /api/v1/admin/campaigns/{id}/activate
                                     ↓
                              Tenant Middleware
                                     ↓
                              AdminController
                              (ActivateCampaign)
                                     ↓
                              Find Campaign by ID
                              (tenant filter applied)
                                     ↓
                              campaign.Activate()
                              (domain method)
                                     ↓
                              SaveChanges()
                                     ↓
                              Returns: 204 No Content
                                     ↓
                              Admin Service
                              (updates campaign in signal)
                                     ↓
                              Campaigns Component
                              (button updates immediately)
                                     ↓
                              Toast Notification
                              ("Campaign activated!")
```

## Component Architecture

### Admin Dashboard Component

```
AdminDashboardComponent
├── Template
│   ├── Header Section
│   │   ├── Title
│   │   └── Description
│   │
│   ├── Key Metrics Grid (4 cards)
│   │   ├── Total Customers Card
│   │   ├── Points Awarded Card
│   │   ├── Points Redeemed Card
│   │   └── Active Campaigns Card
│   │
│   ├── Charts Row (2 columns)
│   │   ├── Tier Distribution Chart
│   │   │   └── Progress bars for each tier
│   │   └── Recent Transactions Timeline
│   │       └── Daily transaction cards
│   │
│   └── Quick Actions Section
│       ├── Create Campaign Button
│       ├── Add Reward Rule Button
│       └── View Customers Button
│
├── Component Class
│   ├── adminService (injected)
│   ├── analytics (computed signal)
│   ├── ngOnInit() → loadAnalytics()
│   ├── getTierColor()
│   ├── getTierBgColor()
│   └── getTierPercentage()
│
└── Styling
    ├── Cinematic red theme
    ├── Glassmorphism effects
    ├── Smooth animations
    └── Responsive grid
```

### Campaigns Component

```
CampaignsComponent
├── Template
│   ├── Header Section
│   │   ├── Title & Description
│   │   └── Create Campaign Button
│   │
│   ├── Filter Buttons
│   │   ├── All
│   │   ├── Active
│   │   └── Inactive
│   │
│   ├── Campaigns Grid
│   │   └── Campaign Cards
│   │       ├── Name & Type Badge
│   │       ├── Status Toggle
│   │       ├── Description
│   │       ├── Type-specific Value
│   │       ├── Duration
│   │       ├── Participations
│   │       └── Targeting Filters
│   │
│   └── Create Campaign Modal
│       └── Form
│           ├── Campaign Name
│           ├── Description
│           ├── Campaign Type Selector
│           ├── Type-specific Fields
│           ├── Date Range
│           ├── Target Segment
│           └── Min Transaction Amount
│
├── Component Class
│   ├── adminService (injected)
│   ├── toastService (injected)
│   ├── campaigns (computed signal)
│   ├── showCreateModal (boolean)
│   ├── filterActive (boolean | null)
│   ├── newCampaign (form model)
│   ├── ngOnInit() → loadCampaigns()
│   ├── createCampaign()
│   ├── toggleCampaignStatus()
│   ├── closeCreateModal()
│   ├── isValidCampaign()
│   └── getCampaignTypeBadge()
│
└── Styling
    ├── Card-based grid layout
    ├── Modal with backdrop
    ├── Form validation styles
    └── Type-specific badges
```

## State Management Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    Admin Service (Signals)                   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  campaigns = signal<Campaign[]>([])                         │
│  rewardRules = signal<RewardRule[]>([])                     │
│  analytics = signal<DashboardAnalytics | null>(null)        │
│  loading = signal(false)                                    │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  HTTP Request Flow:                                     │ │
│  │                                                          │ │
│  │  1. Set loading(true)                                   │ │
│  │  2. Make HTTP call                                      │ │
│  │  3. Update signal with response                         │ │
│  │  4. Set loading(false)                                  │ │
│  │  5. Components auto-update via computed()              │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
└─────────────────────────────────────────────────────────────┘
         │                    │                    │
         │                    │                    │
         ▼                    ▼                    ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   Admin      │    │  Campaigns   │    │  Navigation  │
│  Dashboard   │    │  Component   │    │  Component   │
│              │    │              │    │              │
│ analytics =  │    │ campaigns =  │    │ (no state)   │
│ computed()   │    │ computed()   │    │              │
└──────────────┘    └──────────────┘    └──────────────┘
```

## Security Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Security Layers                           │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Layer 1: HTTP Interceptors (Frontend)                      │
│  ├─ tenant.interceptor.ts → Adds X-Tenant-Id header        │
│  ├─ auth.interceptor.ts → Adds JWT Bearer token            │
│  └─ error.interceptor.ts → Handles 401/403 errors          │
│                                                              │
│  Layer 2: Middleware (Backend)                              │
│  ├─ TenantResolutionMiddleware → Extracts tenant_id        │
│  ├─ AuthenticationMiddleware → Validates JWT               │
│  └─ ExceptionHandlingMiddleware → Catches errors           │
│                                                              │
│  Layer 3: Authorization (Controller)                        │
│  └─ [Authorize(Roles = "Admin,TenantAdmin")]               │
│      → Checks user has required role                        │
│                                                              │
│  Layer 4: Tenant Isolation (EF Core)                        │
│  └─ TenantInterceptor → Adds tenant_id to all queries      │
│                                                              │
│  Layer 5: Database (PostgreSQL)                             │
│  └─ Row-Level Security (RLS) Policies                       │
│      → Final defense-in-depth layer                         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Technology Stack

```
┌─────────────────────────────────────────────────────────────┐
│                      Frontend Stack                          │
├─────────────────────────────────────────────────────────────┤
│  - Angular 18 (Standalone Components)                       │
│  - TypeScript 5.x                                            │
│  - Tailwind CSS v4                                           │
│  - Angular Signals (Reactive State)                         │
│  - RxJS (HTTP & Observables)                                │
│  - Angular Router (Lazy Loading)                            │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                      Backend Stack                           │
├─────────────────────────────────────────────────────────────┤
│  - .NET 8 (ASP.NET Core Web API)                            │
│  - C# 12                                                     │
│  - Entity Framework Core 8                                   │
│  - PostgreSQL 14+                                            │
│  - JWT Authentication                                        │
│  - Role-Based Authorization                                  │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    Design Patterns                           │
├─────────────────────────────────────────────────────────────┤
│  - Clean Architecture                                        │
│  - Domain-Driven Design (DDD)                               │
│  - Repository Pattern                                        │
│  - Factory Pattern (Campaign.Create)                        │
│  - Reactive Programming (Signals)                           │
│  - Dependency Injection                                      │
└─────────────────────────────────────────────────────────────┘
```

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Production Deployment                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────┐                                         │
│  │   CDN / Nginx  │ ← Static Assets (Angular)              │
│  └────────┬───────┘                                         │
│           │                                                  │
│  ┌────────▼───────┐                                         │
│  │  Load Balancer │                                         │
│  └────────┬───────┘                                         │
│           │                                                  │
│  ┌────────▼───────────────────────┐                        │
│  │   API Gateway / Reverse Proxy  │                        │
│  │   (Nginx / Azure App Gateway)  │                        │
│  └────────┬───────────────────────┘                        │
│           │                                                  │
│  ┌────────▼───────────────────────┐                        │
│  │   .NET 8 Web API Instances     │                        │
│  │   (Horizontal Scaling)         │                        │
│  └────────┬───────────────────────┘                        │
│           │                                                  │
│  ┌────────▼───────────────────────┐                        │
│  │   PostgreSQL Database          │                        │
│  │   (with RLS enabled)           │                        │
│  └────────────────────────────────┘                        │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

**This architecture provides:**
- ✅ Scalability (horizontal scaling)
- ✅ Security (multi-layer defense)
- ✅ Performance (reactive state, caching)
- ✅ Maintainability (clean architecture)
- ✅ Testability (dependency injection)


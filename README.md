# LoyaltySphere - Multi-Tenant Loyalty & Rewards Platform

## 🎯 Project Overview

**LoyaltySphere** is a production-ready, enterprise-grade loyalty and rewards platform inspired by real-world implementations like Loynova. It demonstrates modern microservices architecture, multi-tenancy, real-time processing, and a cinematic premium UI experience.

### Real-World Case Studies Implemented

1. **National Bank of Egypt** - Instant POS discounts and cashback on transactions
2. **Suez Canal Bank** - Real-time reward calculation with instant cashback
3. **Shell Egypt** - Points accumulation on fuel purchases with redemption catalog
4. **Kellogg** - Campaign-based rewards and customer engagement

## 🏗️ Architecture Highlights

- **Multi-Tenant Architecture**: Shared PostgreSQL database with Row-Level Security (RLS)
- **Microservices**: Domain-driven design with clean architecture
- **Real-Time Processing**: SignalR for live points updates and notifications
- **Event-Driven**: MassTransit + RabbitMQ with Outbox Pattern
- **Resilience**: Polly for retry policies and circuit breakers
- **Observability**: OpenTelemetry + distributed tracing + Serilog
- **Caching**: Redis for performance optimization
- **Security**: OIDC/OAuth2 with hierarchical RBAC

## 🎨 Frontend Features

### Cinematic Red Theme
- **Dominant Color**: Deep Red (#9F1239) - loyalty and premium feel
- **Accents**: Gold (#F59E0B) for rewards, Black (#0F172A) for depth
- **Design System**: Centralized Tailwind v4 with @theme directive
- **Animations**: Smooth reward pop effects, fade-ins, scale transitions
- **Real-Time**: Live points balance updates via SignalR
- **Icons**: Heroicons + Lucide (trophy, gift, sparkles, chart, wallet)

## 🚀 Quick Start

### Prerequisites
- Docker Desktop
- .NET 8 SDK
- Node.js 20+
- PostgreSQL (via Docker)

### One-Command Setup
```bash
docker compose up -d
```

This will start:
- PostgreSQL with RLS policies
- RabbitMQ for messaging
- Redis for caching
- API Gateway
- Reward Calculation Microservice
- Angular Frontend (http://localhost:4200)
- API (http://localhost:5000)

### Manual Setup (Development)

#### Backend
```bash
cd src/Services/LoyaltySphere.RewardService
dotnet restore
dotnet ef database update
dotnet run
```

#### Frontend
```bash
cd src/Web/loyalty-sphere-ui
npm install
npm start
```

## 📁 Project Structure

```
LoyaltySphere/
├── src/
│   ├── Services/
│   │   ├── LoyaltySphere.RewardService/      # Reward calculation microservice
│   │   ├── LoyaltySphere.TransactionService/ # Transaction processing
│   │   └── LoyaltySphere.ApiGateway/         # API Gateway with Ocelot
│   ├── BuildingBlocks/
│   │   ├── Common/                            # Shared kernel
│   │   ├── EventBus/                          # MassTransit abstractions
│   │   └── MultiTenancy/                      # Tenant resolution
│   └── Web/
│       └── loyalty-sphere-ui/                 # Angular 18 frontend
├── tests/
├── deployment/
│   ├── docker-compose.yml
│   └── k8s/                                   # Kubernetes manifests
└── .github/workflows/                         # CI/CD pipelines
```

## 🎤 Interview Talking Points

### 1. Multi-Tenancy Strategy
**Question**: "How did you implement multi-tenancy?"

**Answer**: "I used a shared database with Row-Level Security (RLS) approach. Each request includes a tenant identifier resolved via middleware (`TenantResolutionMiddleware`). The `ITenantContext` service provides tenant info throughout the request pipeline. PostgreSQL RLS policies automatically filter data at the database level, ensuring complete data isolation. This approach balances cost efficiency with security."

**Code Reference**: `TenantResolutionMiddleware.cs`, `ApplicationDbContext.cs` with RLS interceptor

### 2. Real-Time Processing
**Question**: "How do you handle real-time updates?"

**Answer**: "I implemented SignalR hubs for real-time communication. When a transaction occurs, the reward calculation happens asynchronously via MassTransit. Once processed, the `RewardHub` pushes updates to connected clients. The Angular frontend uses signals to reactively update the UI, showing live points balance and animated reward notifications."

**Code Reference**: `RewardHub.cs`, `reward.service.ts`, `dashboard.component.ts`

### 3. Event-Driven Architecture
**Question**: "How do you ensure reliable event processing?"

**Answer**: "I use the Outbox Pattern with MassTransit. Domain events are saved to an outbox table in the same transaction as business data. A background service polls the outbox and publishes events to RabbitMQ. This guarantees at-least-once delivery and maintains consistency between the database and message broker."

**Code Reference**: `OutboxMessage.cs`, `OutboxProcessor.cs`, `TransactionCreatedEvent.cs`

### 4. Resilience Patterns
**Question**: "How do you handle failures in distributed systems?"

**Answer**: "I use Polly for resilience. HTTP calls have retry policies with exponential backoff. Circuit breakers prevent cascading failures. For example, if the reward service is down, the circuit opens after 3 consecutive failures, and requests fail fast for 30 seconds before retrying. This protects downstream services."

**Code Reference**: `PollyPolicies.cs`, `RewardServiceClient.cs`

### 5. Observability
**Question**: "How do you monitor and debug distributed systems?"

**Answer**: "I implemented OpenTelemetry for distributed tracing. Each request gets a correlation ID that flows through all microservices. Serilog provides structured logging with context enrichment. Metrics track key business events (points awarded, redemptions). In production, this integrates with Jaeger for trace visualization and Prometheus for metrics."

**Code Reference**: `OpenTelemetryExtensions.cs`, `Program.cs` telemetry setup

### 6. Security & RBAC
**Question**: "How did you implement authorization?"

**Answer**: "I use hierarchical RBAC with OIDC/OAuth2. Roles include SuperAdmin, TenantAdmin, Manager, and Customer. The `AuthorizationPolicyProvider` dynamically creates policies based on permissions. Angular uses `angular-auth-oidc-client` for token management. API endpoints are protected with `[Authorize(Policy = "ManageRewards")]` attributes."

**Code Reference**: `PermissionAuthorizationHandler.cs`, `auth.service.ts`

### 7. Cinematic UI Design
**Question**: "Tell me about your frontend design approach."

**Answer**: "I created a cinematic red theme using Tailwind v4's @theme directive for centralized configuration. The deep red (#9F1239) conveys loyalty and premium feel. Gold accents highlight rewards. I implemented smooth animations for reward pop effects using Tailwind's animation utilities. The design is mobile-first, fully responsive, and uses Angular signals for reactive state management with OnPush change detection for optimal performance."

**Code Reference**: `styles.css` (@theme config), `reward-animation.component.ts`

### 8. CI/CD Pipeline
**Question**: "How do you deploy this application?"

**Answer**: "I have a complete GitHub Actions pipeline. On push, it runs tests, builds Docker images, pushes to a registry, and can deploy to Kubernetes. The pipeline includes code quality checks, security scanning, and automated versioning. Docker Compose handles local development, while Kubernetes manifests support production deployment with health checks, resource limits, and horizontal pod autoscaling."

**Code Reference**: `.github/workflows/ci-cd.yml`, `deployment/k8s/`

### 9. Performance Optimization
**Question**: "How did you optimize performance?"

**Answer**: "Multiple strategies: Redis caching for frequently accessed data (tenant configs, reward rules), EF Core query optimization with AsNoTracking for read-only queries, Angular lazy loading for routes, OnPush change detection, and bundle optimization. SignalR reduces polling overhead. Database indexes on tenant_id and frequently queried columns."

**Code Reference**: `CacheService.cs`, `app.routes.ts` (lazy loading)

### 10. Testing Strategy
**Question**: "How do you test this application?"

**Answer**: "I use xUnit for unit and integration tests. Integration tests use WebApplicationFactory with a test database. I test multi-tenancy isolation, reward calculation logic, and event publishing. Frontend uses Jasmine/Karma for component tests. The CI pipeline runs all tests before deployment."

**Code Reference**: `RewardCalculationServiceTests.cs`, `TenantIsolationTests.cs`

## 🎨 Cinematic Dashboard Preview

The dashboard features:
- **Hero Section**: Large points balance with animated counter and gold sparkle effects
- **Transaction Feed**: Real-time updates with smooth fade-in animations
- **Reward Cards**: Glassmorphism effect with red gradient borders
- **Charts**: Dark-themed analytics with red accent colors
- **Notifications**: Toast system with reward celebration animations
- **Mobile-First**: Fully responsive with touch-optimized interactions

## 🔧 Technology Stack

### Backend
- .NET 8 + ASP.NET Core Web API
- Entity Framework Core + PostgreSQL
- MassTransit + RabbitMQ
- SignalR
- Redis
- Polly
- OpenTelemetry + Serilog
- xUnit

### Frontend
- Angular 18 (standalone components)
- TypeScript
- Tailwind CSS v4
- angular-auth-oidc-client
- Heroicons + Lucide
- RxJS + Signals

### Infrastructure
- Docker + Docker Compose
- Kubernetes
- GitHub Actions
- PostgreSQL
- RabbitMQ
- Redis

## 📊 Domain Features

### Tenants
- National Bank of Egypt
- Suez Canal Bank
- Shell Egypt
- Kellogg

### Customer Features
- Earn points on transactions
- Instant cashback
- Campaign participation
- Reward catalog browsing
- Points redemption
- Transaction history

### Admin Features
- Tenant management
- Campaign creation
- Reward rule configuration
- Analytics dashboard
- Customer management

### POS Simulation
```bash
POST /api/v1/transactions
{
  "tenantId": "national-bank",
  "customerId": "cust-123",
  "amount": 1000.00,
  "merchantId": "merchant-456"
}
```

## 🎯 Key Differentiators

1. **Production-Ready**: Not a toy project - includes all enterprise patterns
2. **Real-Time**: Live updates without polling
3. **Scalable**: Microservices with horizontal scaling support
4. **Observable**: Full tracing and monitoring
5. **Secure**: Multi-tenant isolation + RBAC + OIDC
6. **Beautiful**: Cinematic UI that feels premium
7. **Testable**: Comprehensive test coverage
8. **Deployable**: One-command setup with Docker

## 📝 License

MIT License - Free for portfolio and learning purposes

## 👨‍💻 Author

Built as a portfolio project demonstrating modern full-stack architecture and design patterns.

---

**Ready to impress in your next interview!** 🚀

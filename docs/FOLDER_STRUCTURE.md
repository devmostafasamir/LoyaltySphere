# LoyaltySphere - Complete Folder Structure

```
LoyaltySphere/
│
├── README.md
├── LoyaltySphere.sln
├── .gitignore
├── docker-compose.yml
├── docker-compose.override.yml
│
├── .github/
│   └── workflows/
│       ├── ci-cd.yml
│       └── pr-validation.yml
│
├── src/
│   │
│   ├── BuildingBlocks/
│   │   │
│   │   ├── Common/
│   │   │   ├── LoyaltySphere.Common.csproj
│   │   │   ├── Domain/
│   │   │   │   ├── Entity.cs
│   │   │   │   ├── AggregateRoot.cs
│   │   │   │   ├── ValueObject.cs
│   │   │   │   └── IDomainEvent.cs
│   │   │   ├── Application/
│   │   │   │   ├── IUnitOfWork.cs
│   │   │   │   └── Result.cs
│   │   │   └── Infrastructure/
│   │   │       ├── Clock/
│   │   │       │   └── IDateTimeProvider.cs
│   │   │       └── Serialization/
│   │   │           └── JsonSerializerOptionsProvider.cs
│   │   │
│   │   ├── MultiTenancy/
│   │   │   ├── LoyaltySphere.MultiTenancy.csproj
│   │   │   ├── ITenantContext.cs
│   │   │   ├── TenantContext.cs
│   │   │   ├── TenantInfo.cs
│   │   │   ├── Middleware/
│   │   │   │   └── TenantResolutionMiddleware.cs
│   │   │   └── Extensions/
│   │   │       └── MultiTenancyExtensions.cs
│   │   │
│   │   ├── EventBus/
│   │   │   ├── LoyaltySphere.EventBus.csproj
│   │   │   ├── IIntegrationEvent.cs
│   │   │   ├── IEventBus.cs
│   │   │   ├── MassTransit/
│   │   │   │   ├── MassTransitEventBus.cs
│   │   │   │   └── EventBusExtensions.cs
│   │   │   └── Outbox/
│   │   │       ├── OutboxMessage.cs
│   │   │       ├── IOutboxRepository.cs
│   │   │       └── OutboxProcessor.cs
│   │   │
│   │   └── Observability/
│   │       ├── LoyaltySphere.Observability.csproj
│   │       ├── OpenTelemetry/
│   │       │   └── OpenTelemetryExtensions.cs
│   │       └── Logging/
│   │           └── SerilogExtensions.cs
│   │
│   ├── Services/
│   │   │
│   │   ├── RewardService/
│   │   │   ├── LoyaltySphere.RewardService.csproj
│   │   │   ├── Program.cs
│   │   │   ├── appsettings.json
│   │   │   ├── appsettings.Development.json
│   │   │   ├── Dockerfile
│   │   │   │
│   │   │   ├── Domain/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── Reward.cs
│   │   │   │   │   ├── RewardRule.cs
│   │   │   │   │   ├── Campaign.cs
│   │   │   │   │   └── Customer.cs
│   │   │   │   ├── ValueObjects/
│   │   │   │   │   ├── Points.cs
│   │   │   │   │   ├── Money.cs
│   │   │   │   │   └── TenantId.cs
│   │   │   │   ├── Events/
│   │   │   │   │   ├── RewardCalculatedEvent.cs
│   │   │   │   │   └── PointsAwardedEvent.cs
│   │   │   │   └── Services/
│   │   │   │       └── IRewardCalculationService.cs
│   │   │   │
│   │   │   ├── Application/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CalculateReward/
│   │   │   │   │   │   ├── CalculateRewardCommand.cs
│   │   │   │   │   │   └── CalculateRewardCommandHandler.cs
│   │   │   │   │   └── RedeemPoints/
│   │   │   │   │       ├── RedeemPointsCommand.cs
│   │   │   │   │       └── RedeemPointsCommandHandler.cs
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetCustomerBalance/
│   │   │   │   │   │   ├── GetCustomerBalanceQuery.cs
│   │   │   │   │   │   └── GetCustomerBalanceQueryHandler.cs
│   │   │   │   │   └── GetRewardHistory/
│   │   │   │   │       ├── GetRewardHistoryQuery.cs
│   │   │   │   │       └── GetRewardHistoryQueryHandler.cs
│   │   │   │   ├── IntegrationEvents/
│   │   │   │   │   ├── TransactionCreatedEvent.cs
│   │   │   │   │   └── TransactionCreatedEventHandler.cs
│   │   │   │   └── Services/
│   │   │   │       └── RewardCalculationService.cs
│   │   │   │
│   │   │   ├── Infrastructure/
│   │   │   │   ├── Persistence/
│   │   │   │   │   ├── ApplicationDbContext.cs
│   │   │   │   │   ├── Configurations/
│   │   │   │   │   │   ├── RewardConfiguration.cs
│   │   │   │   │   │   ├── CustomerConfiguration.cs
│   │   │   │   │   │   └── OutboxMessageConfiguration.cs
│   │   │   │   │   ├── Interceptors/
│   │   │   │   │   │   ├── TenantInterceptor.cs
│   │   │   │   │   │   └── OutboxInterceptor.cs
│   │   │   │   │   ├── Migrations/
│   │   │   │   │   └── Repositories/
│   │   │   │   │       ├── RewardRepository.cs
│   │   │   │   │       └── CustomerRepository.cs
│   │   │   │   ├── Caching/
│   │   │   │   │   └── RedisCacheService.cs
│   │   │   │   ├── Resilience/
│   │   │   │   │   └── PollyPolicies.cs
│   │   │   │   └── SignalR/
│   │   │   │       └── RewardHub.cs
│   │   │   │
│   │   │   └── Api/
│   │   │       ├── Controllers/
│   │   │       │   ├── RewardsController.cs
│   │   │       │   └── CustomersController.cs
│   │   │       ├── Middleware/
│   │   │       │   └── ExceptionHandlingMiddleware.cs
│   │   │       └── Extensions/
│   │   │           └── ServiceCollectionExtensions.cs
│   │   │
│   │   ├── TransactionService/
│   │   │   ├── LoyaltySphere.TransactionService.csproj
│   │   │   ├── Program.cs
│   │   │   ├── appsettings.json
│   │   │   ├── Dockerfile
│   │   │   │
│   │   │   ├── Domain/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── Transaction.cs
│   │   │   │   │   └── Merchant.cs
│   │   │   │   └── Events/
│   │   │   │       └── TransactionCreatedEvent.cs
│   │   │   │
│   │   │   ├── Application/
│   │   │   │   ├── Commands/
│   │   │   │   │   └── CreateTransaction/
│   │   │   │   │       ├── CreateTransactionCommand.cs
│   │   │   │   │       └── CreateTransactionCommandHandler.cs
│   │   │   │   └── Queries/
│   │   │   │       └── GetTransactionHistory/
│   │   │   │           ├── GetTransactionHistoryQuery.cs
│   │   │   │           └── GetTransactionHistoryQueryHandler.cs
│   │   │   │
│   │   │   ├── Infrastructure/
│   │   │   │   └── Persistence/
│   │   │   │       ├── TransactionDbContext.cs
│   │   │   │       └── Configurations/
│   │   │   │           └── TransactionConfiguration.cs
│   │   │   │
│   │   │   └── Api/
│   │   │       └── Controllers/
│   │   │           └── TransactionsController.cs
│   │   │
│   │   └── ApiGateway/
│   │       ├── LoyaltySphere.ApiGateway.csproj
│   │       ├── Program.cs
│   │       ├── ocelot.json
│   │       ├── ocelot.Development.json
│   │       └── Dockerfile
│   │
│   └── Web/
│       └── loyalty-sphere-ui/
│           ├── package.json
│           ├── angular.json
│           ├── tsconfig.json
│           ├── tailwind.config.js
│           ├── Dockerfile
│           │
│           ├── src/
│           │   ├── index.html
│           │   ├── main.ts
│           │   ├── styles.css                    # Centralized @theme config
│           │   │
│           │   ├── app/
│           │   │   ├── app.component.ts
│           │   │   ├── app.component.html
│           │   │   ├── app.routes.ts
│           │   │   ├── app.config.ts
│           │   │   │
│           │   │   ├── core/
│           │   │   │   ├── auth/
│           │   │   │   │   ├── auth.service.ts
│           │   │   │   │   ├── auth.config.ts
│           │   │   │   │   └── auth.guard.ts
│           │   │   │   ├── interceptors/
│           │   │   │   │   ├── tenant.interceptor.ts
│           │   │   │   │   ├── auth.interceptor.ts
│           │   │   │   │   └── error.interceptor.ts
│           │   │   │   ├── services/
│           │   │   │   │   ├── signalr.service.ts
│           │   │   │   │   ├── toast.service.ts
│           │   │   │   │   └── tenant.service.ts
│           │   │   │   └── models/
│           │   │   │       ├── tenant.model.ts
│           │   │   │       ├── customer.model.ts
│           │   │   │       └── reward.model.ts
│           │   │   │
│           │   │   ├── features/
│           │   │   │   ├── dashboard/
│           │   │   │   │   ├── dashboard.component.ts
│           │   │   │   │   ├── dashboard.component.html
│           │   │   │   │   ├── dashboard.component.css
│           │   │   │   │   └── components/
│           │   │   │   │       ├── points-balance/
│           │   │   │   │       │   ├── points-balance.component.ts
│           │   │   │   │       │   └── points-balance.component.html
│           │   │   │   │       ├── transaction-feed/
│           │   │   │   │       │   ├── transaction-feed.component.ts
│           │   │   │   │       │   └── transaction-feed.component.html
│           │   │   │   │       └── reward-animation/
│           │   │   │   │           ├── reward-animation.component.ts
│           │   │   │   │           └── reward-animation.component.html
│           │   │   │   │
│           │   │   │   ├── rewards/
│           │   │   │   │   ├── rewards.component.ts
│           │   │   │   │   ├── rewards.component.html
│           │   │   │   │   └── components/
│           │   │   │   │       ├── reward-catalog/
│           │   │   │   │       └── redemption-history/
│           │   │   │   │
│           │   │   │   ├── transactions/
│           │   │   │   │   ├── transactions.component.ts
│           │   │   │   │   └── transactions.component.html
│           │   │   │   │
│           │   │   │   └── admin/
│           │   │   │       ├── admin.component.ts
│           │   │   │       └── components/
│           │   │   │           ├── tenant-management/
│           │   │   │           ├── campaign-management/
│           │   │   │           └── analytics/
│           │   │   │
│           │   │   ├── shared/
│           │   │   │   ├── components/
│           │   │   │   │   ├── toast/
│           │   │   │   │   │   ├── toast.component.ts
│           │   │   │   │   │   └── toast.component.html
│           │   │   │   │   ├── loading-spinner/
│           │   │   │   │   ├── card/
│           │   │   │   │   └── button/
│           │   │   │   ├── directives/
│           │   │   │   │   └── animate-on-scroll.directive.ts
│           │   │   │   └── pipes/
│           │   │   │       ├── points-format.pipe.ts
│           │   │   │       └── currency-format.pipe.ts
│           │   │   │
│           │   │   └── layout/
│           │   │       ├── header/
│           │   │       │   ├── header.component.ts
│           │   │       │   └── header.component.html
│           │   │       ├── sidebar/
│           │   │       │   ├── sidebar.component.ts
│           │   │       │   └── sidebar.component.html
│           │   │       └── footer/
│           │   │           ├── footer.component.ts
│           │   │           └── footer.component.html
│           │   │
│           │   └── assets/
│           │       ├── icons/
│           │       └── images/
│           │
│           └── .dockerignore
│
├── tests/
│   ├── LoyaltySphere.RewardService.Tests/
│   │   ├── LoyaltySphere.RewardService.Tests.csproj
│   │   ├── Unit/
│   │   │   ├── Domain/
│   │   │   │   └── RewardCalculationTests.cs
│   │   │   └── Application/
│   │   │       └── CalculateRewardCommandHandlerTests.cs
│   │   └── Integration/
│   │       ├── Api/
│   │       │   └── RewardsControllerTests.cs
│   │       └── Infrastructure/
│   │           └── TenantIsolationTests.cs
│   │
│   └── LoyaltySphere.TransactionService.Tests/
│       ├── LoyaltySphere.TransactionService.Tests.csproj
│       └── Unit/
│           └── CreateTransactionCommandHandlerTests.cs
│
├── deployment/
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml
│   │
│   ├── k8s/
│   │   ├── namespace.yaml
│   │   ├── configmap.yaml
│   │   ├── secrets.yaml
│   │   ├── postgresql/
│   │   │   ├── statefulset.yaml
│   │   │   ├── service.yaml
│   │   │   └── pvc.yaml
│   │   ├── rabbitmq/
│   │   │   ├── deployment.yaml
│   │   │   └── service.yaml
│   │   ├── redis/
│   │   │   ├── deployment.yaml
│   │   │   └── service.yaml
│   │   ├── reward-service/
│   │   │   ├── deployment.yaml
│   │   │   ├── service.yaml
│   │   │   └── hpa.yaml
│   │   ├── transaction-service/
│   │   │   ├── deployment.yaml
│   │   │   ├── service.yaml
│   │   │   └── hpa.yaml
│   │   ├── api-gateway/
│   │   │   ├── deployment.yaml
│   │   │   ├── service.yaml
│   │   │   └── ingress.yaml
│   │   └── frontend/
│   │       ├── deployment.yaml
│   │       └── service.yaml
│   │
│   └── scripts/
│       ├── init-db.sql
│       ├── seed-data.sql
│       └── setup-rls.sql
│
└── docs/
    ├── architecture/
    │   ├── system-overview.md
    │   ├── multi-tenancy.md
    │   └── event-flow.md
    ├── api/
    │   └── openapi.yaml
    └── deployment/
        └── kubernetes-guide.md
```

## Key Directories Explained

### BuildingBlocks
Shared libraries used across all microservices:
- **Common**: Domain primitives, base entities, value objects
- **MultiTenancy**: Tenant resolution and context management
- **EventBus**: MassTransit integration and Outbox Pattern
- **Observability**: OpenTelemetry and Serilog configuration

### Services
Individual microservices following Clean Architecture:
- **RewardService**: Core loyalty logic, reward calculation, SignalR hub
- **TransactionService**: Transaction processing and POS simulation
- **ApiGateway**: Ocelot-based API gateway with routing and aggregation

### Web
Angular 18 frontend with standalone components:
- **core**: Singleton services, auth, interceptors
- **features**: Feature modules (dashboard, rewards, admin)
- **shared**: Reusable components, directives, pipes
- **layout**: Shell components (header, sidebar, footer)

### Tests
Comprehensive test coverage:
- **Unit Tests**: Domain logic, command handlers
- **Integration Tests**: API endpoints, database operations, tenant isolation

### Deployment
Infrastructure as code:
- **docker-compose.yml**: Local development environment
- **k8s/**: Production Kubernetes manifests with HPA, ingress, secrets
- **scripts/**: Database initialization and RLS setup

## File Count Summary
- **Backend C# Files**: ~80 files
- **Frontend TypeScript Files**: ~60 files
- **Configuration Files**: ~25 files
- **Test Files**: ~20 files
- **Deployment Files**: ~20 files
- **Total**: ~205 files

This structure follows industry best practices for microservices, clean architecture, and modern Angular development.

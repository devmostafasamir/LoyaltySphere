using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Application.Services;
using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Domain.Services;
using LoyaltySphere.RewardService.Domain.Strategies;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using LoyaltySphere.RewardService.Infrastructure.Repositories;
using LoyaltySphere.RewardService.Infrastructure.SignalR;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace LoyaltySphere.RewardService.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring services in dependency injection container.
/// Follows Single Responsibility Principle - each method configures one concern.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds multi-tenancy services to the container.
    /// </summary>
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        services.AddScoped<ITenantContext, TenantContext>();
        return services;
    }

    /// <summary>
    /// Adds database context and repository pattern implementation.
    /// </summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(30);
                });
        });

        // Repository Pattern - Dependency Inversion Principle
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IRewardRepository, RewardRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IRewardRuleRepository, RewardRuleRepository>();

        return services;
    }

    /// <summary>
    /// Adds application layer services (CQRS handlers, domain services).
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR for CQRS
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(IRewardCalculationService).Assembly);
        });

        // Application Services
        services.AddScoped<IRewardCalculationService, RewardCalculationService>();
        services.AddScoped<IRewardNotificationService, RewardNotificationService>();

        // Domain Services (Phase 3 - SOLID Refactoring)
        services.AddDomainServices();

        return services;
    }

    /// <summary>
    /// Adds domain services to the container.
    /// Phase 3: Extract domain logic into dedicated services following SRP.
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Domain Services - Single Responsibility Principle
        services.AddScoped<ITierCalculationService, TierCalculationService>();
        services.AddScoped<IRewardRuleSelector, RewardRuleSelector>();
        services.AddScoped<ICampaignEligibilityChecker, CampaignEligibilityChecker>();
        services.AddScoped<IPointsCapService, PointsCapService>();

        // Strategy Pattern - Open/Closed Principle
        services.AddSingleton<ICampaignStrategyFactory, CampaignStrategyFactory>();

        return services;
    }

    /// <summary>
    /// Adds Redis caching services with production-grade configuration.
    /// - SSL/TLS for Redis Cloud connections
    /// - Graceful degradation (app doesn't crash if Redis is down)
    /// - Connection event logging
    /// - Health check integration
    /// - Tenant-aware cache service
    /// - SignalR Redis backplane for scale-out
    /// </summary>
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");

        // Validate that user didn't accidentally paste Postgres connection string into Redis config in MonsterASP
        if (!string.IsNullOrEmpty(redisConnectionString) && redisConnectionString.Contains("Host="))
        {
            throw new ArgumentException("⚠️ CONFIGURATION ERROR: Your 'Redis' connection string contains 'Host='. You have accidentally pasted your PostgreSQL database connection string into the Redis connection string setting inside the MonsterASP control panel! Please correct the Redis configuration variable.");
        }

        // Parse connection string into ConfigurationOptions for fine-grained control
        var redisOptions = ConfigurationOptions.Parse(redisConnectionString!);
        redisOptions.AbortOnConnectFail = false;       // Don't crash if Redis is down at startup
        redisOptions.ConnectRetry = 3;                 // Retry connection up to 3 times
        redisOptions.ConnectTimeout = 5000;            // 5-second connection timeout
        redisOptions.SyncTimeout = 3000;               // 3-second sync operation timeout
        redisOptions.AsyncTimeout = 5000;              // 5-second async operation timeout
        redisOptions.ReconnectRetryPolicy = new ExponentialRetry(5000); // Exponential backoff on reconnect
        redisOptions.Ssl = redisOptions.Ssl;           // Respect ssl= in connection string
        redisOptions.ClientName = "LoyaltySphere-RewardService";

        // Register IConnectionMultiplexer as singleton with event logging
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();

            var multiplexer = ConnectionMultiplexer.Connect(redisOptions);

            multiplexer.ConnectionFailed += (sender, args) =>
            {
                logger.LogWarning(
                    "Redis connection FAILED: {EndPoint}, {FailureType}, {Exception}",
                    args.EndPoint,
                    args.FailureType,
                    args.Exception?.Message ?? "none");
            };

            multiplexer.ConnectionRestored += (sender, args) =>
            {
                logger.LogInformation(
                    "Redis connection RESTORED: {EndPoint}, {FailureType}",
                    args.EndPoint,
                    args.FailureType);
            };

            multiplexer.ErrorMessage += (sender, args) =>
            {
                logger.LogError("Redis server error: {Message}", args.Message);
            };

            multiplexer.InternalError += (sender, args) =>
            {
                logger.LogError(args.Exception, "Redis internal error on {EndPoint}", args.EndPoint);
            };

            if (multiplexer.IsConnected)
            {
                logger.LogInformation("✅ Redis connected successfully to {EndPoints}", string.Join(", ", redisOptions.EndPoints));
            }
            else
            {
                logger.LogWarning("⚠️ Redis connection deferred — will retry in background. App continues without cache.");
            }

            return multiplexer;
        });

        // Distributed cache backed by Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = redisOptions;
            options.InstanceName = "LoyaltySphere:";
        });

        // Tenant-aware cache service (application-layer abstraction)
        services.AddScoped<Application.Interfaces.ICacheService, Caching.RedisCacheService>();

        return services;
    }


    /// <summary>
    /// Adds message bus services using MassTransit and RabbitMQ.
    /// </summary>
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Register consumers from assembly
            x.AddConsumers(typeof(ServiceCollectionExtensions).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    /// <summary>
    /// Adds real-time SignalR services with Redis backplane for horizontal scaling.
    /// </summary>
    public static IServiceCollection AddRealTimeServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var signalRBuilder = services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        });

        // Configure Redis backplane if connection string is provided
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            signalRBuilder.AddStackExchangeRedis(redisConnectionString, options =>
            {
                options.Configuration = StackExchange.Redis.ConfigurationOptions.Parse(redisConnectionString);
                options.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("LoyaltySphere-SignalR");
            });
        }

        return services;
    }

    /// <summary>
    /// Adds API versioning configuration.
    /// </summary>
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Adds health checks for dependencies.
    /// </summary>
    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register RedisHealthCheck as singleton so it can be resolved by the health check system
        services.AddSingleton<Caching.RedisHealthCheck>();

        services.AddHealthChecks()
            .AddCheck<Caching.RedisHealthCheck>(
                "redis",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
                tags: new[] { "cache", "redis" });

        return services;
    }
}

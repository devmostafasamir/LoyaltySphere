using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Application.Services;
using LoyaltySphere.RewardService.Domain.Repositories;
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

        // Domain Services
        services.AddScoped<IRewardCalculationService, RewardCalculationService>();
        services.AddScoped<IRewardNotificationService, RewardNotificationService>();

        return services;
    }

    /// <summary>
    /// Adds Redis caching services.
    /// </summary>
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConnection = configuration.GetConnectionString("Redis");
            return ConnectionMultiplexer.Connect(redisConnection!);
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "LoyaltySphere:";
        });

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
    /// Adds real-time SignalR services.
    /// </summary>
    public static IServiceCollection AddRealTimeServices(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        });

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
        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "postgresql",
                tags: new[] { "db", "postgresql" })
            .AddRedis(
                configuration.GetConnectionString("Redis")!,
                name: "redis",
                tags: new[] { "cache", "redis" });
            // RabbitMQ health check commented out - requires package
            // .AddRabbitMQ(
            //     configuration["RabbitMQ:Host"]!,
            //     name: "rabbitmq",
            //     tags: new[] { "messaging", "rabbitmq" });

        return services;
    }
}

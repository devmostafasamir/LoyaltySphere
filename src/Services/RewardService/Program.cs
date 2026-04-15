using LoyaltySphere.MultiTenancy;
using LoyaltySphere.MultiTenancy.Middleware;
using LoyaltySphere.RewardService.Api.Middleware;
using LoyaltySphere.RewardService.Application.Services;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using LoyaltySphere.RewardService.Infrastructure.SignalR;
using Asp.Versioning;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Configure Serilog
// ============================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "RewardService")
    .WriteTo.Console()
    .WriteTo.File("logs/reward-service-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ============================================
// Add Services to Container
// ============================================

// Multi-Tenancy
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<TenantResolutionMiddleware>();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        });
});

// Redis Cache
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(configuration!);
});

// MediatR for CQRS
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// Application Services
builder.Services.AddScoped<IRewardCalculationService, RewardCalculationService>();
builder.Services.AddScoped<IRewardNotificationService, RewardNotificationService>();

// MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Register consumers
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        // Allow SignalR to use JWT from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LoyaltySphere Reward Service API",
        Version = "v1",
        Description = "Multi-tenant loyalty and rewards platform - Reward calculation microservice",
        Contact = new OpenApiContact
        {
            Name = "LoyaltySphere Team",
            Email = "support@loyaltysphere.com"
        }
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments
    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Required for SignalR
    });
});

// Health Checks
builder.Services.AddHealthChecks();
    // .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!) // Requires AspNetCore.HealthChecks.Npgsql package
    // .AddRedis(builder.Configuration.GetConnectionString("Redis")!) // Requires AspNetCore.HealthChecks.Redis package
    // .AddRabbitMQ(rabbitConnectionString: builder.Configuration["RabbitMQ:ConnectionString"]!); // Requires AspNetCore.HealthChecks.RabbitMQ package

// ============================================
// Build Application
// ============================================
var app = builder.Build();

// ============================================
// Configure HTTP Request Pipeline
// ============================================

// Exception Handling (must be first)
app.UseExceptionHandling();

// Serilog Request Logging
app.UseSerilogRequestLogging();

// Swagger (Development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reward Service API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at root
    });
}

// CORS
app.UseCors("AllowFrontend");

// HTTPS Redirection
app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Multi-Tenancy (after auth, before controllers)
app.UseMiddleware<TenantResolutionMiddleware>();

// Controllers
app.MapControllers();

// SignalR Hubs
app.MapHub<RewardHub>("/hubs/rewards");

// Health Checks
app.MapHealthChecks("/health");

// ============================================
// Database Migration (Development only)
// ============================================
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating the database");
    }
}

// ============================================
// Run Application
// ============================================
try
{
    Log.Information("Starting LoyaltySphere Reward Service");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

using LoyaltySphere.Common.Domain;
using LoyaltySphere.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LoyaltySphere.RewardService.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that converts domain events to outbox messages.
/// Implements the Outbox Pattern for reliable event publishing.
/// Domain events are saved in the same transaction as business data.
/// </summary>
public class OutboxInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<OutboxInterceptor> _logger;

    public OutboxInterceptor(ILogger<OutboxInterceptor> logger)
    {
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConvertDomainEventsToOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertDomainEventsToOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ConvertDomainEventsToOutboxMessages(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        // Get all entities with domain events
        var entitiesWithEvents = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        if (!entitiesWithEvents.Any())
        {
            return;
        }

        // Convert domain events to outbox messages
        var outboxMessages = new List<OutboxMessage>();

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                try
                {
                    var eventType = domainEvent.GetType();
                    var eventContent = JsonSerializer.Serialize(domainEvent, eventType);

                    var outboxMessage = new OutboxMessage(
                        type: $"{eventType.FullName}, {eventType.Assembly.GetName().Name}",
                        content: eventContent,
                        occurredOnUtc: DateTime.UtcNow);

                    outboxMessages.Add(outboxMessage);

                    _logger.LogDebug(
                        "Created outbox message for domain event {EventType}",
                        eventType.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error creating outbox message for domain event {EventType}",
                        domainEvent.GetType().Name);
                    
                    throw;
                }
            }

            // Clear domain events after converting to outbox messages
            entity.ClearDomainEvents();
        }

        // Add outbox messages to context
        if (outboxMessages.Any())
        {
            context.Set<OutboxMessage>().AddRange(outboxMessages);
            
            _logger.LogInformation(
                "Added {Count} domain events to outbox",
                outboxMessages.Count);
        }
    }
}

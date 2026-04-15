using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LoyaltySphere.EventBus.Outbox;

/// <summary>
/// Background service that processes outbox messages.
/// Polls the outbox table and publishes events to the message broker.
/// Ensures reliable event delivery with retry logic.
/// </summary>
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _processingInterval;
    private readonly int _batchSize;
    private readonly int _maxRetries;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger,
        TimeSpan? processingInterval = null,
        int batchSize = 100,
        int maxRetries = 3)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _processingInterval = processingInterval ?? TimeSpan.FromSeconds(10);
        _batchSize = batchSize;
        _maxRetries = maxRetries;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox Processor started. Processing interval: {Interval}s, Batch size: {BatchSize}",
            _processingInterval.TotalSeconds,
            _batchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Processor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        // Get DbContext (must be scoped)
        var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
        
        // Get event bus
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        // Get unprocessed messages
        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => !m.IsProcessed)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(_batchSize)
            .ToListAsync(cancellationToken);

        if (!messages.Any())
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                // Deserialize event
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    _logger.LogWarning(
                        "Could not find type {Type} for outbox message {Id}",
                        message.Type,
                        message.Id);
                    
                    message.MarkAsFailed($"Type not found: {message.Type}");
                    continue;
                }

                var @event = JsonSerializer.Deserialize(message.Content, eventType);
                if (@event == null)
                {
                    _logger.LogWarning(
                        "Could not deserialize outbox message {Id}",
                        message.Id);
                    
                    message.MarkAsFailed("Deserialization failed");
                    continue;
                }

                // Publish event
                await eventBus.PublishAsync(@event, cancellationToken);

                // Mark as processed
                message.MarkAsProcessed();

                _logger.LogDebug(
                    "Published outbox message {Id} of type {Type}",
                    message.Id,
                    message.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing outbox message {Id}",
                    message.Id);

                message.MarkAsFailed(ex.Message);

                if (!message.CanRetry(_maxRetries))
                {
                    _logger.LogWarning(
                        "Outbox message {Id} exceeded max retries ({MaxRetries})",
                        message.Id,
                        _maxRetries);
                }
            }
        }

        // Save changes
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Processed {Successful} outbox messages successfully, {Failed} failed",
            messages.Count(m => m.IsProcessed),
            messages.Count(m => !m.IsProcessed));
    }
}

/// <summary>
/// Interface for event bus abstraction.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}

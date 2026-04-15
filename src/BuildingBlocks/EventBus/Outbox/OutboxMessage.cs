namespace LoyaltySphere.EventBus.Outbox;

/// <summary>
/// Represents a domain event stored in the outbox for reliable publishing.
/// Implements the Outbox Pattern to ensure at-least-once delivery of events.
/// Events are saved in the same transaction as business data, then published asynchronously.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
    public bool IsProcessed { get; private set; }

    private OutboxMessage() { } // EF Core

    public OutboxMessage(
        string type,
        string content,
        DateTime occurredOnUtc)
    {
        Id = Guid.NewGuid();
        Type = type;
        Content = content;
        OccurredOnUtc = occurredOnUtc;
        IsProcessed = false;
        RetryCount = 0;
    }

    public void MarkAsProcessed()
    {
        IsProcessed = true;
        ProcessedOnUtc = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
        RetryCount++;
    }

    public bool CanRetry(int maxRetries) => RetryCount < maxRetries;
}

namespace TicketEvents.Api.Models;

public sealed class TicketEventRecord
{
    public required string EventId { get; init; }
    public required string TicketId { get; init; }
    public required string Type { get; init; }
    public required string Status { get; init; }
    public string? CustomerId { get; init; }
    public string? Subject { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public DateTimeOffset ProcessedAt { get; init; }
}

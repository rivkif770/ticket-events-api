namespace TicketEvents.Api.Models;

public sealed record TicketResponse(
    string TicketId,
    string CustomerId,
    string Subject,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record TicketEventResponse(
    string EventId,
    string TicketId,
    bool Duplicate);

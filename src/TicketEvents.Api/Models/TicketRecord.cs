namespace TicketEvents.Api.Models;

public sealed class TicketRecord
{
    public required string TicketId { get; init; }
    public required string CustomerId { get; init; }
    public required string Subject { get; init; }
    public required string Status { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace TicketEvents.Api.Models;

public sealed class TicketEventRequest : IValidatableObject
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string? EventId { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string? TicketId { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public string? CustomerId { get; init; }

    [Required]
    public string? Type { get; init; }

    [Required]
    public string? Status { get; init; }

    [StringLength(500)]
    public string? Subject { get; init; }

    [Required]
    public DateTimeOffset? OccurredAt { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Type is not ("Created" or "StatusChanged"))
        {
            yield return new ValidationResult(
                "type must be Created or StatusChanged.",
                [nameof(Type)]);
        }

        if (Status is not ("Open" or "InProgress" or "Resolved"))
        {
            yield return new ValidationResult(
                "status must be Open, InProgress, or Resolved.",
                [nameof(Status)]);
        }

        if (Type == "Created")
        {
            if (string.IsNullOrWhiteSpace(CustomerId))
            {
                yield return new ValidationResult(
                    "customerId is required for Created events.",
                    [nameof(CustomerId)]);
            }

            if (string.IsNullOrWhiteSpace(Subject))
            {
                yield return new ValidationResult(
                    "subject is required for Created events.",
                    [nameof(Subject)]);
            }
        }
    }
}

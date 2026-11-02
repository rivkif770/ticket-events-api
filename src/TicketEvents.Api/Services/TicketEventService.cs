using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TicketEvents.Api.Data;
using TicketEvents.Api.Models;

namespace TicketEvents.Api.Services;

public sealed record TicketEventProcessingResult(
    bool IsDuplicate = false,
    bool TicketWasCreated = false,
    string? ConflictMessage = null);

public sealed class TicketEventService(TicketDbContext db, ILogger<TicketEventService> logger)
{
    public async Task<TicketEventProcessingResult> ProcessAsync(
        TicketEventRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        if (await db.TicketEvents.AnyAsync(item => item.EventId == request.EventId, cancellationToken))
        {
            await transaction.RollbackAsync(cancellationToken);
            return new TicketEventProcessingResult(IsDuplicate: true);
        }

        var ticketEvent = new TicketEventRecord
        {
            EventId = request.EventId!,
            TicketId = request.TicketId!,
            Type = request.Type!,
            Status = request.Status!,
            CustomerId = request.CustomerId,
            Subject = request.Subject,
            OccurredAt = request.OccurredAt!.Value,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        db.TicketEvents.Add(ticketEvent);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            db.Entry(ticketEvent).State = EntityState.Detached;
            logger.LogInformation("Ignored duplicate ticket event {EventId}", request.EventId);
            return new TicketEventProcessingResult(IsDuplicate: true);
        }

        var ticket = await db.Tickets.SingleOrDefaultAsync(
            item => item.TicketId == request.TicketId,
            cancellationToken);

        if (request.Type == "Created")
        {
            if (ticket is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
                db.Entry(ticketEvent).State = EntityState.Detached;
                return new TicketEventProcessingResult(
                    ConflictMessage: $"Ticket '{request.TicketId}' already exists.");
            }

            db.Tickets.Add(new TicketRecord
            {
                TicketId = request.TicketId!,
                CustomerId = request.CustomerId!,
                Subject = request.Subject!,
                Status = request.Status!,
                UpdatedAt = request.OccurredAt!.Value
            });
        }
        else
        {
            if (ticket is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                db.Entry(ticketEvent).State = EntityState.Detached;
                return new TicketEventProcessingResult(
                    ConflictMessage: $"Ticket '{request.TicketId}' must be created before its status can change.");
            }

            // Keep the most recent state when older events arrive late.
            if (request.OccurredAt!.Value >= ticket.UpdatedAt)
            {
                ticket.Status = request.Status!;
                ticket.UpdatedAt = request.OccurredAt.Value;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new TicketEventProcessingResult(TicketWasCreated: request.Type == "Created");
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteErrorCode: 19 };
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketEvents.Api.Data;
using TicketEvents.Api.Models;

namespace TicketEvents.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController(TicketDbContext db) : ControllerBase
{
    [HttpGet("{ticketId}", Name = "GetTicketById")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> GetById(
        string ticketId,
        CancellationToken cancellationToken)
    {
        var ticket = await db.Tickets
            .AsNoTracking()
            .Where(item => item.TicketId == ticketId)
            .Select(item => new TicketResponse(
                item.TicketId,
                item.CustomerId,
                item.Subject,
                item.Status,
                item.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return ticket is null ? NotFound() : Ok(ticket);
    }
}

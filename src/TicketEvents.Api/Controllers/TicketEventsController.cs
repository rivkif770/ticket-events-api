using Microsoft.AspNetCore.Mvc;
using TicketEvents.Api.Models;
using TicketEvents.Api.Services;

namespace TicketEvents.Api.Controllers;

[ApiController]
[Route("api/ticket-events")]
public sealed class TicketEventsController(TicketEventService ticketEventService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<TicketEventResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<TicketEventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketEventResponse>> Post(
        [FromBody] TicketEventRequest request,
        CancellationToken cancellationToken)
    {
        var result = await ticketEventService.ProcessAsync(request, cancellationToken);

        if (result.ConflictMessage is not null)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Ticket event conflicts with the current state.",
                Detail = result.ConflictMessage,
                Status = StatusCodes.Status409Conflict
            });
        }

        var response = new TicketEventResponse(request.EventId!, request.TicketId!, result.IsDuplicate);
        if (result.IsDuplicate)
        {
            return Ok(response);
        }

        return result.TicketWasCreated
            ? CreatedAtRoute("GetTicketById", new { ticketId = request.TicketId }, response)
            : Ok(response);
    }
}

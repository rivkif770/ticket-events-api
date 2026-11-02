using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TicketEvents.Api.Data;
using TicketEvents.Api.Models;

namespace TicketEvents.Api.Tests;

public sealed class TicketEventsApiTests
{
    [Fact]
    public async Task RepeatedEventId_IsStoredOnce_AndDoesNotApplySecondPayload()
    {
        using var factory = new TicketEventsApiFactory();
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/ticket-events", NewEvent(
            "evt-created-3", "T-300", "Created", "Open", "2026-10-20T10:30:00Z"));

        var firstStatusChange = await client.PostAsJsonAsync("/api/ticket-events", NewEvent(
            "evt-status-3", "T-300", "StatusChanged", "Resolved", "2026-10-20T11:00:00Z",
            customerId: null, subject: null));
        var duplicateStatusChange = await client.PostAsJsonAsync("/api/ticket-events", NewEvent(
            "evt-status-3", "T-300", "StatusChanged", "Open", "2026-10-20T12:00:00Z",
            customerId: null, subject: null));

        Assert.Equal(HttpStatusCode.OK, firstStatusChange.StatusCode);
        Assert.Equal(HttpStatusCode.OK, duplicateStatusChange.StatusCode);
        var duplicate = await duplicateStatusChange.Content.ReadFromJsonAsync<TicketEventResponse>();
        Assert.True(duplicate?.Duplicate);

        var ticketResponse = await client.GetAsync("/api/tickets/T-300");
        Assert.Equal(HttpStatusCode.OK, ticketResponse.StatusCode);
        var ticket = await ticketResponse.Content.ReadFromJsonAsync<TicketResponse>();
        Assert.NotNull(ticket);
        Assert.Equal("Resolved", ticket.Status);
        Assert.Equal(DateTimeOffset.Parse("2026-10-20T11:00:00Z"), ticket.UpdatedAt);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
        Assert.Equal(2, await db.TicketEvents.CountAsync(item => item.TicketId == "T-300"));
        Assert.Equal(1, await db.TicketEvents.CountAsync(item => item.EventId == "evt-status-3"));
    }

    private static object NewEvent(
        string eventId,
        string ticketId,
        string type,
        string status,
        string occurredAt,
        string? customerId = "C-100",
        string? subject = "Customer cannot log in") => new
        {
            eventId,
            ticketId,
            customerId,
            type,
            status,
            subject,
            occurredAt
        };
}

public sealed class TicketEventsApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public TicketEventsApiFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<TicketDbContext>>();
            services.RemoveAll<TicketDbContext>();
            services.AddSingleton(_connection);
            services.AddDbContext<TicketDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

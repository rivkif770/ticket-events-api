using Microsoft.EntityFrameworkCore;
using TicketEvents.Api.Data;
using TicketEvents.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Tickets") ?? "Data Source=tickets.db";

builder.Services.AddDbContext<TicketDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<TicketEventService>();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();

// Create the small local database automatically for this exercise.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapControllers();
app.Run();

public partial class Program { }

using Microsoft.EntityFrameworkCore;
using TicketEvents.Api.Models;

namespace TicketEvents.Api.Data;

public sealed class TicketDbContext(DbContextOptions<TicketDbContext> options) : DbContext(options)
{
    public DbSet<TicketRecord> Tickets => Set<TicketRecord>();
    public DbSet<TicketEventRecord> TicketEvents => Set<TicketEventRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TicketRecord>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(ticket => ticket.TicketId);
            entity.Property(ticket => ticket.TicketId).HasMaxLength(100);
            entity.Property(ticket => ticket.CustomerId).HasMaxLength(100).IsRequired();
            entity.Property(ticket => ticket.Subject).HasMaxLength(500).IsRequired();
            entity.Property(ticket => ticket.Status).HasMaxLength(32).IsRequired();
        });

        modelBuilder.Entity<TicketEventRecord>(entity =>
        {
            entity.ToTable("TicketEvents");
            entity.HasKey(ticketEvent => ticketEvent.EventId);
            entity.Property(ticketEvent => ticketEvent.EventId).HasMaxLength(100);
            entity.Property(ticketEvent => ticketEvent.TicketId).HasMaxLength(100).IsRequired();
            entity.Property(ticketEvent => ticketEvent.Type).HasMaxLength(32).IsRequired();
            entity.Property(ticketEvent => ticketEvent.Status).HasMaxLength(32).IsRequired();
            entity.Property(ticketEvent => ticketEvent.CustomerId).HasMaxLength(100);
            entity.Property(ticketEvent => ticketEvent.Subject).HasMaxLength(500);
            entity.HasIndex(ticketEvent => ticketEvent.TicketId);
        });
    }
}

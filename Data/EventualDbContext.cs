using eventual.Models;
using Microsoft.EntityFrameworkCore;

namespace eventual.Data;

public class EventualDbContext(DbContextOptions<EventualDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Lote> Lotes => Set<Lote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lote>()
            .HasOne(l => l.Event)
            .WithMany(e => e.Lotes)
            .HasForeignKey(l => l.EventId);
        
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Lote)
            .WithMany(l => l.Orders)
            .HasForeignKey(o => o.LoteId);               

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Order)
            .WithMany(o => o.Tickets)
            .HasForeignKey(t => t.OrderId);
        
        modelBuilder.Entity<Ticket>()
            .HasIndex(i => i.Code)
            .IsUnique();
    }    

}
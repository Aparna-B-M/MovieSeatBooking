using Microsoft.EntityFrameworkCore;
using MovieSeatBooking.Models;

namespace MovieSeatBooking.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Show> Shows { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique Idempotency Key
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.IdempotencyKey)
                .IsUnique();

            // RowVersion for optimistic concurrency
            modelBuilder.Entity<Seat>()
                .Property(s => s.RowVersion)
                .IsRowVersion();
        }
    }
}
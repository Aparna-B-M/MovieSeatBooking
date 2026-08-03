using Microsoft.EntityFrameworkCore;
using MovieSeatBooking.Data;

namespace MovieSeatBooking.Services;

public class SeatHoldCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SeatHoldCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var expiredSeats = await context.Seats
                .Where(x => x.Status == "HELD"
                && x.HoldUntil < DateTime.UtcNow)
                .ToListAsync();

            foreach (var seat in expiredSeats)
            {
                seat.Status = "AVAILABLE";
                seat.HoldUntil = null;
            }

            await context.SaveChangesAsync();

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
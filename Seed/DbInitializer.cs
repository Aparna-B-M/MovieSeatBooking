using MovieSeatBooking.Data;
using MovieSeatBooking.Models;

namespace MovieSeatBooking.Seed
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            if (context.Shows.Any())
                return;

            var show = new Show
            {
                Id = Guid.NewGuid(),
                MovieName = "Spider-Man: No Way Home",
                ShowTime = DateTime.UtcNow.AddHours(2)
            };

            context.Shows.Add(show);

            var seats = new List<Seat>();

            for (int i = 1; i <= 50; i++)
            {
                seats.Add(new Seat
                {
                    Id = Guid.NewGuid(),
                    ShowId = show.Id,
                    SeatNumber = $"A{i}",
                    Status = "AVAILABLE"
                });
            }

            context.Seats.AddRange(seats);

            context.SaveChanges();
        }
    }
}
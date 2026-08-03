namespace MovieSeatBooking.Models
{
    public class SeatBookingRequest
    {
        public Guid ShowId { get; set; }

        public List<string> SeatNumbers { get; set; } = new();

        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
using System.ComponentModel.DataAnnotations;

namespace MovieSeatBooking.Models;

public class Booking
{
    [Key]
    public Guid Id { get; set; }

    public Guid ShowId { get; set; }

    public Guid SeatId { get; set; }

    public DateTime BookedAt { get; set; }
    public string Status { get; set; } = "CONFIRMED";
    public string IdempotencyKey { get; set; } = string.Empty;
}
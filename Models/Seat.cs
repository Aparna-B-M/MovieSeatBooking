using System.ComponentModel.DataAnnotations;

namespace MovieSeatBooking.Models;

public class Seat
{
    [Key]
    public Guid Id { get; set; }

    public Guid ShowId { get; set; }

    public string SeatNumber { get; set; }

    public string Status { get; set; } = "AVAILABLE";

    public DateTime? HoldUntil { get; set; }
    
     [Timestamp]
    public byte[]? RowVersion { get; set; }
}
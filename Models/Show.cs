using System.ComponentModel.DataAnnotations;

namespace MovieSeatBooking.Models;

public class Show
{
    [Key]
    public Guid Id { get; set; }

    public string MovieName { get; set; }

    public DateTime ShowTime { get; set; }
}
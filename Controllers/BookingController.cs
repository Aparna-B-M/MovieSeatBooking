using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieSeatBooking.Data;
using MovieSeatBooking.Models;

namespace MovieSeatBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }


       [HttpPost("hold")]
public async Task<IActionResult> HoldSeat([FromBody] SeatBookingRequest request)
{
    await using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        var seats = await _context.Seats
            .Where(x => x.ShowId == request.ShowId &&
                        request.SeatNumbers.Contains(x.SeatNumber))
            .ToListAsync();

        // Validate all seats exist
        if (seats.Count != request.SeatNumbers.Count)
        {
           return NotFound(
    request.SeatNumbers.Count == 1
        ? $"Seat {request.SeatNumbers.First()} was not found."
        : "One or more selected seats were not found.");
        }

        // Validate all seats are available
       var unavailableSeats = seats
    .Where(s => s.Status != "AVAILABLE")
    .Select(s => s.SeatNumber)
    .ToList();

if (unavailableSeats.Any())
{
   return BadRequest(
    unavailableSeats.Count == 1
        ? $"Seat {unavailableSeats.First()} is not available."
        : $"Seats {string.Join(", ", unavailableSeats)} are not available.");
}

        // Hold all seats
        foreach (var seat in seats)
        {
            seat.Status = "HELD";
            seat.HoldUntil = DateTime.UtcNow.AddSeconds(30);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return Conflict(
    request.SeatNumbers.Count == 1
        ? "The selected seat was already held by another user."
        : "One or more selected seats were already held by another user.");
        }

        await transaction.CommitAsync();

        return Ok(new
        {
            message = seats.Count == 1
    ? $"Seat {seats.First().SeatNumber} held successfully."
    : $"Seats {string.Join(", ", seats.Select(s => s.SeatNumber))} held successfully.",
            seats = seats.Select(s => s.SeatNumber),
            expiresAt = seats.First().HoldUntil
        });
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

 [HttpPost("confirm")]
public async Task<IActionResult> ConfirmBooking([FromBody] SeatBookingRequest request)
{
    var existingBookings = await _context.Bookings
        .Where(b => b.IdempotencyKey.StartsWith(request.IdempotencyKey))
        .ToListAsync();

    if (existingBookings.Any())
    {
        return Ok(new
        {
            Message = request.SeatNumbers.Count == 1
                ? "Booking already exists for the selected seat."
                : "Booking already exists for the selected seats.",
            BookingIds = existingBookings.Select(x => x.Id)
        });
    }

    await using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        var seats = await _context.Seats
            .Where(x => x.ShowId == request.ShowId &&
                        request.SeatNumbers.Contains(x.SeatNumber))
            .ToListAsync();

        // Validate all seats exist
        if (seats.Count != request.SeatNumbers.Count)
        {
            return NotFound(
                request.SeatNumbers.Count == 1
                    ? "Selected seat was not found."
                    : "One or more selected seats were not found.");
        }

        // Validate all seats are HELD
        var notHeldSeats = seats
            .Where(s => s.Status != "HELD")
            .Select(s => s.SeatNumber)
            .ToList();

        if (notHeldSeats.Any())
        {
           return BadRequest(
    notHeldSeats.Count == 1
        ? $"Seat {notHeldSeats.First()} is not held."
        : $"Seats {string.Join(", ", notHeldSeats)} are not held.");
        }

        // Validate expired holds
        var expiredSeats = seats
            .Where(s => s.HoldUntil == null || s.HoldUntil < DateTime.UtcNow)
            .ToList();

        if (expiredSeats.Any())
        {
            foreach (var seat in expiredSeats)
            {
                seat.Status = "AVAILABLE";
                seat.HoldUntil = null;
            }

            await _context.SaveChangesAsync();

            return BadRequest(
                expiredSeats.Count == 1
                    ? $"Hold expired for seat {expiredSeats.First().SeatNumber}."
                    : $"Hold expired for seats: {string.Join(", ", expiredSeats.Select(s => s.SeatNumber))}.");
        }

        // Book every seat
        foreach (var seat in seats)
        {
            seat.Status = "BOOKED";
            seat.HoldUntil = null;

            _context.Bookings.Add(new Booking
            {
                Id = Guid.NewGuid(),
                ShowId = request.ShowId,
                SeatId = seat.Id,
                BookedAt = DateTime.UtcNow,
                Status = "CONFIRMED",
                IdempotencyKey = $"{request.IdempotencyKey}-{seat.SeatNumber}"
            });
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            return Conflict(
                request.SeatNumbers.Count == 1
                    ? "The selected seat was booked by another user."
                    : "One or more selected seats were booked by another user.");
        }

        await transaction.CommitAsync();

        return Ok(new
        {
            Message = seats.Count == 1
                ? $"Booking confirmed for seat {seats.First().SeatNumber}."
                : $"Booking confirmed for seats: {string.Join(", ", seats.Select(s => s.SeatNumber))}.",
            Seats = seats.Select(s => s.SeatNumber)
        });
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

[HttpGet("availability/{showId}")]
public async Task<IActionResult> GetAvailability(Guid showId)
{
    var available = await _context.Seats.CountAsync(x =>
        x.ShowId == showId &&
        x.Status == "AVAILABLE");

    var held = await _context.Seats.CountAsync(x =>
        x.ShowId == showId &&
        x.Status == "HELD");

    var booked = await _context.Seats.CountAsync(x =>
        x.ShowId == showId &&
        x.Status == "BOOKED");

    return Ok(new
    {
        available,
        held,
        booked
    });
}

[HttpGet("seats/{showId}")]
public async Task<IActionResult> GetSeats(Guid showId)
{
    var seats = await _context.Seats
        .Where(s => s.ShowId == showId)
        .OrderBy(s => s.SeatNumber)
        .Select(s => new
        {
            s.SeatNumber,
            s.Status,
            s.HoldUntil
        })
        .ToListAsync();

    return Ok(seats);
}
    }
}
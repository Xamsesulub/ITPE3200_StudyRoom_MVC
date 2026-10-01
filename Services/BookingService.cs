using MVC.Models;
using MVC.DAL;
using Microsoft.EntityFrameworkCore;

namespace MVC.Services;

public class BookingService
{
    // The service uses the database context to read and save bookings.
    private readonly AppDbContext _db;

    public BookingService(AppDbContext db) => _db = db;

    // Check for an overlapping booking before a new booking is saved.
    // A booking overlaps when it starts before another booking ends and ends after it starts.
    public async Task<bool> TryCreateBookingAsync(
        int roomId,
        int userId,
        DateTime start,
        DateTime end)
    {
        bool hasOverlap = await _db.Bookings.AnyAsync(b =>
            b.RoomId == roomId && b.StartTime < end && b.EndTime > start);

        if (hasOverlap)
        {
            return false;
        }

        _db.Bookings.Add(new BookingModel
        {
            RoomId = roomId,
            UserId = userId,
            StartTime = start,
            EndTime = end
        });
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<List<BookingModel>> GetBookingsForRoomAsync(int roomId, DateTime date)
    {
        // Return only bookings for the selected room and date.
        DateTime nextDay = date.Date.AddDays(1);

        return _db.Bookings
            .AsNoTracking()
            .Where(booking =>
                booking.RoomId == roomId &&
                booking.StartTime >= date.Date &&
                booking.StartTime < nextDay)
            .ToListAsync();
    }

    public Task<List<BookingModel>> GetBookingsForUserAsync(int userId)
    {
        // Include room information because it is displayed with each reservation.
        return _db.Bookings
            .AsNoTracking()
            .Include(booking => booking.Room)
            .Where(booking => booking.UserId == userId)
            .OrderBy(booking => booking.StartTime)
            .ToListAsync();
    }

    public async Task<bool> CancelBookingAsync(int bookingId, int userId)
    {
        // Matching both IDs prevents one user from cancelling another user's booking.
        BookingModel? booking = await _db.Bookings
            .FirstOrDefaultAsync(item => item.BookingId == bookingId && item.UserId == userId);

        if (booking is null)
        {
            return false;
        }

        _db.Bookings.Remove(booking);
        await _db.SaveChangesAsync();
        return true;
    }
}

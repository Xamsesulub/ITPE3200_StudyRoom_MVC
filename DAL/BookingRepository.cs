using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.DAL;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<BookingRepository> _logger;

    public BookingRepository(AppDbContext db, ILogger<BookingRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Check for an overlapping booking before a new booking is saved.
    // A booking overlaps when it starts before another booking ends and ends after it starts.


    public async Task<bool> TryCreate(int roomId, int userId, DateTime start, DateTime end)
    {
        try
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
        catch (Exception e)
        {
            _logger.LogError(e, "");
            return false;
        }
    }

    public async Task<List<BookingModel>?> GetForRoom(int roomId, DateTime date)
    {
        try
        {
            // Return only bookings for the selected room and date.
            DateTime nextDay = date.Date.AddDays(1);

            return await _db.Bookings
                .AsNoTracking()
                .Where(booking =>
                    booking.RoomId == roomId &&
                    booking.StartTime >= date.Date &&
                    booking.StartTime < nextDay)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "");
            return null;

        }
    }

    public async Task<List<BookingModel>?> GetForUser(int userId)
    {
        try
        {
            // Include room information because it is displayed with each reservation.
            return await _db.Bookings
                .AsNoTracking()
                .Include(booking => booking.Room)
                .Where(booking => booking.UserId == userId)
                .OrderBy(booking => booking.StartTime)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "");
            return null;
        }
    }

    public async Task<bool> Cancel(int bookingId, int userId)
    {
        try
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
        catch (Exception e)
        {
            _logger.LogError(e, "[BookingRepository]");
            return false;
        }
    }
}
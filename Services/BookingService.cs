using MVC.Models;
using MVC.DAL;
using Microsoft.EntityFrameworkCore;

namespace MVC.Services{
public class BookingService
{
    private readonly AppDbContext _db;

    public BookingService(AppDbContext db) => _db = db;

    public async Task<bool> TryCreateBookingAsync(int roomId, DateTime start, DateTime end)
    {
        bool hasOverlap = await _db.Bookings.AnyAsync(b =>
        b.RoomId == roomId && b.StartTime < end && b.EndTime > start);

        if (hasOverlap) return false;

        _db.Bookings.Add(new BookingModel{ RoomId = roomId, StartTime = start, EndTime = end});
        await _db.SaveChangesAsync();
        return true;
    }
}
}
/* Denne er kun skrevet av av forelesningen så vi kan jobbe videre med bookingsystemet :) */
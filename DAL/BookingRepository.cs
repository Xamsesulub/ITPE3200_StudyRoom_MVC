using Microsoft.EntityFrameworkCore;
using MVC.Models;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<BookingRepository> _logger;

    public BookingRepository(AppDbContext db, ILogger<BookingRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Making sure 
    public async Task<bool> TryCreate(int roomId, int userId, DateTime start, DateTime end)
    {
        
    }

    public async Task<List<BookingModel>?> GetForRoom(int roomId, DateTime date)
    {
        
    }

    public async Task<List<BookingModel>?> GetForUser(int userId)
    {
        
    }

    public async Task<BookingModel?> Cancel(int bookingId, int userId)
    {
        
    }


}
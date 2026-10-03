using MVC.Models;

namespace MVC.DAL;

public interface IBookingRepository
{
    Task<bool> TryCreate(int roomId, int userId, DateTime start, DateTime end);
    Task<List<BookingModel>?> GetForRoom(int roomId, DateTime date);
    Task<List<BookingModel>?> GetForUser(int userId);
    Task<BookingModel?> Cancel(int bookingId, int userId);
}
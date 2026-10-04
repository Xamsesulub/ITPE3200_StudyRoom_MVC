using MVC.Models;

namespace MVC.DAL;

public record RoomFilter( // To efficienty search and for easier changes if needed
    string? Building,
    int? Floor,
    int? MinimumCapacity,
    bool HasScreen,
    bool HasWhiteboard);

public interface IRoomRepository
{
    Task<List<RoomsModel>?> Search(RoomFilter filter);
    Task<List<RoomsModel>?> GetAll();
    Task<RoomsModel?> GetById(int id);
    Task<RoomsModel?> GetByIdReadOnly(int id);
    Task<bool> Exists(int id);
    Task<bool> Create(RoomsModel room);
    Task<bool> Update(RoomsModel room);
    Task<bool> Delete(int id);
}
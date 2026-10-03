using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.DAL;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<RoomRepository> _logger;

    public RoomRepository(AppDbContext db, ILogger<RoomRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<RoomsModel>?> Search(RoomFilter f)
    {
        try
        {
            IQueryable<RoomsModel> query = _db.Rooms.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(f.Building))
            {
                string building = f.Building.Trim();
                query = query.Where(r => EF.Functions.Like(r.RoomBuilding, $"%{building}%"));
            }
            if (f.Floor is >= 0)
                query = query.Where(r => r.RoomFloor == f.Floor);
            if (f.MinimumCapacity is > 0)
                query = query.Where(r => r.RoomCapacity >= f.MinimumCapacity);
            if (f.HasScreen)
                query = query.Where(r => r.RoomScreen);
            if (f.HasWhiteboard)
                query = query.Where(r => r.RoomWhiteboard);

            return await query
                .OrderBy(r => r.RoomBuilding)
                .ThenBy(r => r.RoomFloor)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[RoomRepository] Search failed for filter {@Filter}", f);
            return null;
        }
    }

    public async Task<List<RoomsModel>?> GetAll()
    {
        try
        {
            return await _db.Rooms.AsNoTracking()
                .OrderBy(r => r.RoomBuilding)
                .ThenBy(r => r.RoomFloor)
                .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[RoomRepository] GetAll failed");
            return null;
        }
    }

    public async Task<RoomsModel?> GetById(int id)
    {
        try
        {
            return await _db.Rooms.FindAsync(id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[RoomRepository] GetById failed for room {RoomId}", id);
            return null;
        }
    }

    public async Task<RoomsModel?> GetByIdReadOnly(int id)
    {
        try
        {
            return await _db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.RoomId == id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[RoomRepository] GetByIdReadOnly failed for room {RoomId}", id);
            return null;
        }
    }

    public async Task<bool> Exists(int id)
    {
        try
        {
            return await _db.Rooms.AnyAsync(r => r.RoomId == id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[RoomRepository] Exists failed for room {RoomId}", id);
            return false;
        }
    }

    public async Task<bool> Create(RoomsModel room)
    {
        try
        {
            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[RoomRepository] Create failed for room {RoomId}", room.RoomId);
            return false;
        }
    }

    public async Task<bool> Update(RoomsModel room)
    {
        try
        {
            _db.Rooms.Update(room);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[RoomRepository] Update failed for room {RoomId}", room.RoomId);
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null)
            {
                _logger.LogWarning("[RoomRepository] Room {RoomId} not found for deletion", id);
                return false;
            }

            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[RoomRepository] Delete failed for room {RoomId}", id);
            return false;
        }
    }
}
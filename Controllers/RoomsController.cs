using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.ViewModels;
using MVC.DAL;

namespace MVC.Controllers;

public class RoomsController : Controller
{
    private readonly RoomsDbContext _context;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(RoomsDbContext context, ILogger<RoomsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Table(
        string? building,
        int? floor,
        int? minimumCapacity,
        bool hasScreen = false,
        bool hasWhiteboard = false)
    {
        try
        {
            IQueryable<RoomsModel> query = _context.Rooms.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(building))
            {
                string buildingFilter = building.Trim();
                query = query.Where(room =>
                    EF.Functions.Like(room.RoomBuilding, $"%{buildingFilter}%"));
            }

            if (floor is >= 0)
            {
                query = query.Where(room => room.RoomFloor == floor);
            }

            if (minimumCapacity is > 0)
            {
                query = query.Where(room => room.RoomCapacity >= minimumCapacity);
            }

            if (hasScreen)
            {
                query = query.Where(room => room.RoomScreen);
            }

            if (hasWhiteboard)
            {
                query = query.Where(room => room.RoomWhiteboard);
            }

            List<RoomsModel> rooms = await query
                .OrderBy(room => room.RoomBuilding)
                .ThenBy(room => room.RoomFloor)
                .ToListAsync();

            var roomsViewModel = new RoomsViewModel(rooms, "Table")
            {
                Building = building,
                Floor = floor,
                MinimumCapacity = minimumCapacity,
                HasScreen = hasScreen,
                HasWhiteboard = hasWhiteboard
            };

            return View(roomsViewModel);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Kunne ikke søke etter rom i databasen.");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(Table));
    }

    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        try
        {
            List<RoomsModel> rooms = await _context.Rooms
                .AsNoTracking()
                .OrderBy(room => room.RoomBuilding)
                .ThenBy(room => room.RoomFloor)
                .ToListAsync();

            var roomsViewModel = new RoomsViewModel(rooms, "Manage");
            return View(roomsViewModel);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Kunne ikke hente rom for administrasjon.");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            _logger.LogWarning("Detaljsiden ble åpnet uten rom-ID.");
            return BadRequest();
        }

        RoomsModel? room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(room => room.RoomId == id);

        if (room is null)
        {
            _logger.LogWarning("Rom med ID {RoomId} ble ikke funnet.", id);
            return NotFound();
        }

        return View(room);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("RoomBuilding,RoomFloor,RoomCapacity,RoomScreen,RoomWhiteboard")]
        RoomsModel room)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Forsøk på å opprette rom med ugyldige verdier.");
            return View(room);
        }

        try
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Rom {RoomId} i {RoomBuilding} ble opprettet.",
                room.RoomId,
                room.RoomBuilding);

            TempData["SuccessMessage"] = "Rommet ble opprettet.";
            return RedirectToAction(nameof(Manage));
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Kunne ikke opprette rom i databasen.");
            ModelState.AddModelError(
                string.Empty,
                "Rommet kunne ikke lagres. Prøv igjen senere.");
            return View(room);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            _logger.LogWarning("Redigeringssiden ble åpnet uten rom-ID.");
            return BadRequest();
        }

        RoomsModel? room = await _context.Rooms.FindAsync(id);

        if (room is null)
        {
            _logger.LogWarning(
                "Rom med ID {RoomId} ble ikke funnet ved redigering.",
                id);
            return NotFound();
        }

        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("RoomId,RoomBuilding,RoomFloor,RoomCapacity,RoomScreen,RoomWhiteboard")]
        RoomsModel room)
    {
        if (id != room.RoomId)
        {
            _logger.LogWarning(
                "Rom-ID i adressen ({RouteId}) var ulik rom-ID i skjemaet ({FormId}).",
                id,
                room.RoomId);
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Forsøk på å oppdatere rom med ID {RoomId} med ugyldige verdier.",
                room.RoomId);
            return View(room);
        }

        try
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Rom med ID {RoomId} ble oppdatert.",
                room.RoomId);

            TempData["SuccessMessage"] = "Rommet ble oppdatert.";
            return RedirectToAction(nameof(Manage));
        }
        catch (DbUpdateConcurrencyException exception)
        {
            bool roomExists = await _context.Rooms
                .AnyAsync(existingRoom => existingRoom.RoomId == room.RoomId);

            if (!roomExists)
            {
                _logger.LogWarning(
                    exception,
                    "Rom med ID {RoomId} ble slettet før det kunne oppdateres.",
                    room.RoomId);
                return NotFound();
            }

            _logger.LogError(
                exception,
                "En konflikt oppstod ved oppdatering av rom med ID {RoomId}.",
                room.RoomId);
            ModelState.AddModelError(
                string.Empty,
                "Rommet ble endret av noen andre. Last siden på nytt og prøv igjen.");
            return View(room);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(
                exception,
                "Kunne ikke oppdatere rom med ID {RoomId}.",
                room.RoomId);
            ModelState.AddModelError(
                string.Empty,
                "Rommet kunne ikke oppdateres. Prøv igjen senere.");
            return View(room);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            _logger.LogWarning("Slettesiden ble åpnet uten rom-ID.");
            return BadRequest();
        }

        RoomsModel? room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(room => room.RoomId == id);

        if (room is null)
        {
            _logger.LogWarning(
                "Rom med ID {RoomId} ble ikke funnet ved sletting.",
                id);
            return NotFound();
        }

        return View(room);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        RoomsModel? room = await _context.Rooms.FindAsync(id);

        if (room is null)
        {
            _logger.LogWarning(
                "Rom med ID {RoomId} ble ikke funnet da sletting ble bekreftet.",
                id);
            return NotFound();
        }

        try
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Rom med ID {RoomId} ble slettet.", id);
            TempData["SuccessMessage"] = "Rommet ble slettet.";
            return RedirectToAction(nameof(Manage));
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(
                exception,
                "Kunne ikke slette rom med ID {RoomId}.",
                id);
            TempData["ErrorMessage"] =
                "Rommet kunne ikke slettes. Det kan være knyttet til en reservasjon.";
            return RedirectToAction(nameof(Manage));
        }
    }
}

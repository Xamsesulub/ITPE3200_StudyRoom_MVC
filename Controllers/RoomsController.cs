using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.ViewModels;
using MVC.DAL;

namespace MVC.Controllers;

public class RoomsController : Controller
{
    // The database context gives the controller access to room data,
    // while the logger records errors and important events.
    private readonly RoomsDbContext _context;
    private readonly ILogger<RoomsController> _logger;

    // ASP.NET Core provides the database context and logger through dependency injection.
    public RoomsController(RoomsDbContext context, ILogger<RoomsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Displays the room search page and filters rooms using the user's search choices.
    // All filters are optional, so the page displays every room when no filters are selected.
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
            // AsNoTracking is used because these rooms are only displayed and will not be changed.
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

            // The completed query is sorted and then executed against the database.
            List<RoomsModel> rooms = await query
                .OrderBy(room => room.RoomBuilding)
                .ThenBy(room => room.RoomFloor)
                .ToListAsync();

            // The view model contains both the results and the selected filters,
            // allowing the page to keep the user's search values after a search.
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
            // Technical details are written to the log while the user receives a safe error response.
            _logger.LogError(exception, "Kunne ikke søke etter rom i databasen.");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    // Redirects the default Rooms address to the room search page.
    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(Table));
    }

    // Loads every room as read-only data for the administration page.
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

    // Finds one room by its ID and displays its complete information.
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

    // Displays an empty form for registering a new room.
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Validates the submitted room and saves it when all values are valid.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("RoomId,RoomBuilding,RoomFloor,RoomCapacity,RoomScreen,RoomWhiteboard")]
        RoomsModel room)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Forsøk på å opprette rom med ugyldige verdier.");
            return View(room);
        }

        // Room numbers must be unique because RoomId is the primary key.
        bool roomNumberExists = await _context.Rooms
            .AnyAsync(existingRoom => existingRoom.RoomId == room.RoomId);

        if (roomNumberExists)
        {
            _logger.LogWarning(
                "Forsøk på å opprette et rom med eksisterende romnummer {RoomId}.",
                room.RoomId);
            ModelState.AddModelError(
                nameof(RoomsModel.RoomId),
                "Romnummeret er allerede registrert.");
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

    // Loads an existing room and displays its current values in the edit form.
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

    // Validates the edited values and saves the changes to the database.
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
            // A concurrency error can occur if another user changes or deletes the room first.
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

    // Displays the selected room so the user can confirm the deletion.
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

    // Deletes the room only after the user has confirmed the action.
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

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

    public async Task<IActionResult> Table()
    {
        try
        {
            List<RoomsModel> rooms = await _context.Rooms
                .AsNoTracking()
                .OrderBy(room => room.RoomBuilding)
                .ThenBy(room => room.RoomFloor)
                .ToListAsync();

            var roomsViewModel = new RoomsViewModel(rooms, "Table");
            return View(roomsViewModel);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Kunne ikke hente rom fra databasen.");
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
            return RedirectToAction(nameof(Table));
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
}

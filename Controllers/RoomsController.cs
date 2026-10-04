using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using MVC.ViewModels;
using MVC.DAL;
using Microsoft.AspNetCore.Authorization;

namespace MVC.Controllers;

public class RoomsController : Controller
{
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(IRoomRepository roomRepository, ILogger<RoomsController> logger)
    {
        _roomRepository = roomRepository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Table(
        string? building,
        int? floor,
        int? minimumCapacity,
        bool hasScreen = false,
        bool hasWhiteboard = false)
    {
        var filter = new RoomFilter(building, floor, minimumCapacity, hasScreen, hasWhiteboard);
        List<RoomsModel>? rooms = await _roomRepository.Search(filter);

        if (rooms == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

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

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction(nameof(Table));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Manage()
    {
        List<RoomsModel>? rooms = await _roomRepository.GetAll();

        if (rooms == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var roomsViewModel = new RoomsViewModel(rooms, "Manage");
        return View(roomsViewModel);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            _logger.LogWarning("[RoomsController] Detaljsiden ble åpnet uten rom-ID.");
            return BadRequest();
        }

        RoomsModel? room = await _roomRepository.GetByIdReadOnly(id.Value);

        if (room is null)
        {
            _logger.LogWarning("[RoomsController] Rom med ID {RoomId} ble ikke funnet.", id);
            return NotFound();
        }

        return View(room);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [Bind("RoomId,RoomBuilding,RoomFloor,RoomCapacity,RoomScreen,RoomWhiteboard")]
        RoomsModel room)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[RoomsController] Forsøk på å opprette rom med ugyldige verdier.");
            return View(room);
        }

        if (await _roomRepository.Exists(room.RoomId))
        {
            ModelState.AddModelError(nameof(RoomsModel.RoomId), "Romnummeret er allerede registrert.");
            return View(room);
        }

        if (!await _roomRepository.Create(room))
        {
            ModelState.AddModelError(string.Empty, "Rommet kunne ikke lagres. Prøv igjen senere.");
            return View(room);
        }

        _logger.LogInformation("[RoomsController] Rom {RoomId} i {RoomBuilding} ble opprettet.",
            room.RoomId, room.RoomBuilding);
        TempData["SuccessMessage"] = "Rommet ble opprettet.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            _logger.LogWarning("[RoomsController] Redigeringssiden ble åpnet uten rom-ID.");
            return BadRequest();
        }

        RoomsModel? room = await _roomRepository.GetById(id.Value);

        if (room is null)
        {
            _logger.LogWarning(
                "[RoomsController] Rom med ID {RoomId} ble ikke funnet ved redigering.",
                id);
            return NotFound();
        }

        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("RoomId,RoomBuilding,RoomFloor,RoomCapacity,RoomScreen,RoomWhiteboard")]
        RoomsModel room)
    {
        if (id != room.RoomId)
        {
            _logger.LogWarning(
                "[RoomsController] Rom-ID i adressen ({RouteId}) var ulik rom-ID i skjemaet ({FormId}).",
                id, room.RoomId);
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "[RoomsController] Forsøk på å oppdatere rom med ID {RoomId} med ugyldige verdier.",
                room.RoomId);
            return View(room);
        }

        if (!await _roomRepository.Update(room))
        {
            if (!await _roomRepository.Exists(room.RoomId))
            {
                return NotFound();   // deleted by someone else
            }

            ModelState.AddModelError(string.Empty, "Rommet kunne ikke oppdateres. Prøv igjen senere.");
            return View(room);
        }

        _logger.LogInformation("[RoomsController] Rom med ID {RoomId} ble oppdatert.", room.RoomId);
        TempData["SuccessMessage"] = "Rommet ble oppdatert.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            _logger.LogWarning("[RoomsController] Slettesiden ble åpnet uten rom-ID.");
            return BadRequest();
        }

        RoomsModel? room = await _roomRepository.GetByIdReadOnly(id.Value);

        if (room is null)
        {
            _logger.LogWarning(
                "[RoomsController] Rom med ID {RoomId} ble ikke funnet ved sletting.",
                id);
            return NotFound();
        }

        return View(room);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await _roomRepository.Delete(id))
        {
            TempData["ErrorMessage"] = "Rommet kunne ikke slettes. Det kan være knyttet til en reservasjon.";
            return RedirectToAction(nameof(Manage));
        }

        _logger.LogInformation("[RoomsController] Rom med ID {RoomId} ble slettet.", id);
        TempData["SuccessMessage"] = "Rommet ble slettet.";
        return RedirectToAction(nameof(Manage));
    }
}

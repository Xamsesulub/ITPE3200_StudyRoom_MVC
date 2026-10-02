using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.DAL;
using MVC.Models;
using MVC.Services;
using MVC.ViewModels;

namespace MVC.Controllers;

public class BookingsController : Controller
{
    // A temporary user is used until the login page is connected to authentication.
    private const int DemoUserId = 1;

    // The database provides room data, while the service handles booking rules.
    private readonly AppDbContext _context;
    private readonly BookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        AppDbContext context,
        BookingService bookingService,
        ILogger<BookingsController> logger)
    {
        _context = context;
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Reserve(int id, DateTime? date)
    {
        // Load the room selected on the Find rooms page.
        RoomsModel? room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.RoomId == id);

        if (room is null)
        {
            return NotFound();
        }

        // Use today's date when the user has not selected a date yet.
        DateTime selectedDate = (date ?? DateTime.Today).Date;
        if (selectedDate < DateTime.Today)
        {
            selectedDate = DateTime.Today;
        }

        ReserveRoomViewModel model = await BuildReserveViewModelAsync(room, selectedDate);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reserve(ReserveRoomViewModel model)
    {
        // Read the room again from the database instead of trusting room details from the form.
        RoomsModel? room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.RoomId == model.RoomId);

        if (room is null)
        {
            return NotFound();
        }

        // Check the booking information on the server before saving it.
        if (model.Date.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.Date), "Datoen kan ikke være i fortiden.");
        }

        if (model.NumberOfPeople > room.RoomCapacity)
        {
            ModelState.AddModelError(
                nameof(model.NumberOfPeople),
                $"Rommet har bare plass til {room.RoomCapacity} personer.");
        }

        bool validSlot = TryReadTimeSlot(
            model.Date,
            model.SelectedSlot,
            out DateTime start,
            out DateTime end);

        if (!validSlot)
        {
            ModelState.AddModelError(nameof(model.SelectedSlot), "Velg et gyldig tidspunkt.");
        }
        else if (start < DateTime.Now)
        {
            ModelState.AddModelError(nameof(model.SelectedSlot), "Tidspunktet kan ikke være i fortiden.");
        }

        if (!ModelState.IsValid)
        {
            // Rebuild the time slots so the form can be shown again with error messages.
            ReserveRoomViewModel invalidModel = await BuildReserveViewModelAsync(room, model.Date);
            invalidModel.SelectedSlot = model.SelectedSlot;
            invalidModel.NumberOfPeople = model.NumberOfPeople;
            invalidModel.Purpose = model.Purpose;
            return View(invalidModel);
        }

        try
        {
            // The service returns false when another booking uses the same room and time.
            bool created = await _bookingService.TryCreateBookingAsync(
                room.RoomId,
                DemoUserId,
                start,
                end);

            if (!created)
            {
                ModelState.AddModelError(
                    nameof(model.SelectedSlot),
                    "Rommet er allerede booket i dette tidsrommet.");

                ReserveRoomViewModel unavailableModel = await BuildReserveViewModelAsync(room, model.Date);
                unavailableModel.NumberOfPeople = model.NumberOfPeople;
                unavailableModel.Purpose = model.Purpose;
                return View(unavailableModel);
            }

            _logger.LogInformation(
                "[BookingsController] Bruker {UserId} reserverte rom {RoomId} fra {StartTime} til {EndTime}.",
                DemoUserId,
                room.RoomId,
                start,
                end);

            TempData["SuccessMessage"] = "Reservasjonen er lagret.";
            return RedirectToAction(nameof(MyBookings));
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Kunne ikke lage en reservasjon for rom {RoomId}.", room.RoomId);
            ModelState.AddModelError(string.Empty, "Reservasjonen kunne ikke lagres. Prøv igjen.");

            ReserveRoomViewModel errorModel = await BuildReserveViewModelAsync(room, model.Date);
            errorModel.NumberOfPeople = model.NumberOfPeople;
            errorModel.Purpose = model.Purpose;
            return View(errorModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyBookings()
    {
        // Split the user's bookings into upcoming and completed reservations.
        List<BookingModel> bookings = await _bookingService.GetBookingsForUserAsync(DemoUserId);
        DateTime now = DateTime.Now;

        var model = new MyBookingsViewModel
        {
            Upcoming = bookings
                .Where(booking => booking.EndTime >= now)
                .Select(ToListItem)
                .ToList(),
            Past = bookings
                .Where(booking => booking.EndTime < now)
                .OrderByDescending(booking => booking.StartTime)
                .Select(ToListItem)
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        // Only a booking that belongs to the current user can be cancelled.
        bool cancelled = await _bookingService.CancelBookingAsync(id, DemoUserId);
        TempData[cancelled ? "SuccessMessage" : "ErrorMessage"] = cancelled
            ? "Reservasjonen er avbestilt."
            : "Reservasjonen ble ikke funnet.";

        _logger.LogWarning("[BookingsController] Booking {id} har blitt fjernet for bruker med id {user}.", id, DemoUserId); 

        return RedirectToAction(nameof(MyBookings));
    }

    private async Task<ReserveRoomViewModel> BuildReserveViewModelAsync(
        RoomsModel room,
        DateTime date)
    {
        // Compare the room's bookings with one-hour time slots from 08:00 to 20:00.
        List<BookingModel> bookings = await _bookingService
            .GetBookingsForRoomAsync(room.RoomId, date);

        var slots = new List<TimeSlotViewModel>();
        for (int hour = 8; hour < 20; hour++)
        {
            DateTime start = date.Date.AddHours(hour);
            DateTime end = start.AddHours(1);
            bool overlaps = bookings.Any(booking =>
                booking.StartTime < end && booking.EndTime > start);

            slots.Add(new TimeSlotViewModel
            {
                Value = $"{start:HH:mm}-{end:HH:mm}",
                Label = $"{start:HH:mm}–{end:HH:mm}",
                IsAvailable = !overlaps && start >= DateTime.Now
            });
        }

        return new ReserveRoomViewModel
        {
            RoomId = room.RoomId,
            RoomBuilding = room.RoomBuilding,
            RoomFloor = room.RoomFloor,
            RoomCapacity = room.RoomCapacity,
            RoomScreen = room.RoomScreen,
            RoomWhiteboard = room.RoomWhiteboard,
            Date = date.Date,
            NumberOfPeople = Math.Min(4, room.RoomCapacity),
            Slots = slots
        };
    }

    private static bool TryReadTimeSlot(
        DateTime date,
        string? selectedSlot,
        out DateTime start,
        out DateTime end)
    {
        // Convert a value such as "08:00-09:00" into start and end times.
        start = default;
        end = default;
        if (string.IsNullOrWhiteSpace(selectedSlot))
        {
            return false;
        }

        string[] parts = selectedSlot.Split('-', StringSplitOptions.TrimEntries);

        if (parts.Length != 2 ||
            !TimeOnly.TryParseExact(parts[0], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly startTime) ||
            !TimeOnly.TryParseExact(parts[1], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly endTime))
        {
            return false;
        }

        start = date.Date.Add(startTime.ToTimeSpan());
        end = date.Date.Add(endTime.ToTimeSpan());
        return end > start;
    }

    private static BookingListItemViewModel ToListItem(BookingModel booking)
    {
        // Prepare the database model for display on the My bookings page.
        return new BookingListItemViewModel
        {
            BookingId = booking.BookingId,
            RoomId = booking.RoomId,
            RoomBuilding = booking.Room.RoomBuilding,
            Date = booking.StartTime.Date,
            TimeSlot = $"{booking.StartTime:HH:mm}–{booking.EndTime:HH:mm}"
        };
    }
}

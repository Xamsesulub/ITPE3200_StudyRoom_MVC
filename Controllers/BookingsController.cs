using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using MVC.DAL;
using MVC.Models;
using MVC.ViewModels;

namespace MVC.Controllers;

public class BookingsController : Controller
{
    // A temporary user is used until the login page is connected to authentication.
    private const int DemoUserId = 1;

    // The repository handles the buisness logic and the database operations
    private readonly IBookingRepository _bookingRepository;
    // We also need the RoomRepository for Reserve
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository,
        ILogger<BookingsController> logger)
    {
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Reserve(int id, DateTime? date)
    {
        // Load the room selected on the Find rooms page.
        RoomsModel? room = await _roomRepository.GetByIdReadOnly(id);

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
        RoomsModel? room = await _roomRepository.GetByIdReadOnly(model.RoomId);


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

        bool created = await _bookingRepository.TryCreate(room.RoomId, DemoUserId, start, end);

        if (created)
        {
            _logger.LogInformation(
                "[BookingsController] Bruker {UserId} reserverte rom {RoomId} fra {StartTime} til {EndTime}.",
                DemoUserId, room.RoomId, start, end);

            TempData["SuccessMessage"] = "Reservasjonen er lagret.";
            return RedirectToAction(nameof(MyBookings));
        }

        ModelState.AddModelError(
            nameof(model.SelectedSlot),
            "Rommet kunne ikke reserveres. Det kan være booket allerede.");

        ReserveRoomViewModel failedModel = await BuildReserveViewModelAsync(room, model.Date);
        failedModel.NumberOfPeople = model.NumberOfPeople;
        failedModel.Purpose = model.Purpose;
        return View(failedModel);
    }
    

    [HttpGet]
    public async Task<IActionResult> MyBookings()
    {
        // Split the user's bookings into upcoming and completed reservations.
        List<BookingModel>? bookings = await _bookingRepository.GetForUser(DemoUserId);

        if (bookings == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

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
        bool cancelled = await _bookingRepository.Cancel(id, DemoUserId);

        _logger.LogWarning("[BookingsController] Booking {id} har blitt fjernet for bruker med id {user}.", id, DemoUserId);

        return RedirectToAction(nameof(MyBookings));
    }

    private async Task<ReserveRoomViewModel> BuildReserveViewModelAsync(
        RoomsModel room,
        DateTime date)
    {
        // Compare the room's bookings with one-hour time slots from 08:00 to 20:00.
        List<BookingModel> bookings = await _bookingRepository.GetForRoom(room.RoomId, date) ?? new List<BookingModel>();

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

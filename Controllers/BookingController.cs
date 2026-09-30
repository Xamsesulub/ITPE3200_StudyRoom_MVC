using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using MVC.Services;

namespace MVC.Controllers;

public class BookingController : Controller
{
    private readonly BookingService _bookingService;

    public BookingController(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public IActionResult Create(int roomId, DateTime start, DateTime slutt)
    {
        return View(new BookingModel { RoomId = roomId, StartTime = start, EndTime = slutt });
    }

    [HttpPost]
    public async Task<IActionResult> Create(BookingModel booking)
    {
        if (booking.EndTime <= booking.StartTime)
            ModelState.AddModelError("", "Sluttid må være etter starttid.");

        if (!ModelState.IsValid)
            return View(booking);

        bool ok = await _bookingService.TryCreateBookingAsync(booking.RoomId, booking.StartTime, booking.EndTime);

        if (!ok)
        {
            ModelState.AddModelError("", "Rommet er allerede booket i dette tidsrommet.");
            return View(booking);
        }

        return RedirectToAction("Index", "Rooms"); // 
    }
}
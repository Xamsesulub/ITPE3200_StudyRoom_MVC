using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.ViewModels;
using MVC.DAL;

namespace MVC.Controllers;

public class RoomsController : Controller
{
    private readonly RoomsDbContext _context;

    public RoomsController(RoomsDbContext context)
    {
        _context = context;
    }

    public IActionResult Table()
    {
        List<RoomsModel> rooms = _context.Rooms.AsNoTracking().ToList();
        var roomsViewModel = new RoomsViewModel(rooms, "Table");
        return View(roomsViewModel);
    }
}
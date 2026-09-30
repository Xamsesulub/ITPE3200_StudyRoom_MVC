using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.DAL;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    public DbSet<RoomsModel> Rooms { get; set; }

    public DbSet<BookingModel> Bookings{ get; set; }
}
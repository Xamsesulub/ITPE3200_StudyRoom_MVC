using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.DAL;

public class RoomsDbContext : DbContext
{
    public RoomsDbContext(DbContextOptions<RoomsDbContext> options) : base(options)
    {
        
    }
    public DbSet<RoomsModel> Rooms { get; set; }
}
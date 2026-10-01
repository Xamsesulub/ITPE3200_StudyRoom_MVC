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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Testrom som foelger med migrasjonen, slik at alle paa gruppa faar
        // samme utgangspunkt og ikke maa legge inn rom manuelt hver for seg.
        // Id-ene starter paa 101 fordi HasData krever eksplisitte id-er, og rom
        // som noen har lagt inn selv via /Rooms/Create har id 1, 2, 3 og
        // oppover. Uten avstand krasjer migrasjonen med UNIQUE constraint.
        modelBuilder.Entity<RoomsModel>().HasData(
            new RoomsModel { RoomId = 101, RoomBuilding = "Pilestredet 35", RoomFloor = 1, RoomCapacity = 10, RoomScreen = true, RoomWhiteboard = false },
            new RoomsModel { RoomId = 102, RoomBuilding = "Pilestredet 35", RoomFloor = 3, RoomCapacity = 6, RoomScreen = true, RoomWhiteboard = true },
            new RoomsModel { RoomId = 103, RoomBuilding = "Pilestredet 35", RoomFloor = 4, RoomCapacity = 4, RoomScreen = false, RoomWhiteboard = true },
            new RoomsModel { RoomId = 104, RoomBuilding = "Pilestredet 32", RoomFloor = 6, RoomCapacity = 4, RoomScreen = false, RoomWhiteboard = true },
            new RoomsModel { RoomId = 105, RoomBuilding = "Pilestredet 48", RoomFloor = 2, RoomCapacity = 8, RoomScreen = true, RoomWhiteboard = true },
            new RoomsModel { RoomId = 106, RoomBuilding = "Pilestredet 48", RoomFloor = 5, RoomCapacity = 2, RoomScreen = false, RoomWhiteboard = false },
            new RoomsModel { RoomId = 107, RoomBuilding = "Holbergs terrasse", RoomFloor = 1, RoomCapacity = 12, RoomScreen = true, RoomWhiteboard = false },
            new RoomsModel { RoomId = 108, RoomBuilding = "Holbergs terrasse", RoomFloor = 3, RoomCapacity = 6, RoomScreen = true, RoomWhiteboard = true }
        );
    }
}
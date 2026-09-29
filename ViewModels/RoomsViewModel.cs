using MVC.Models;

namespace MVC.ViewModels;

public class RoomsViewModel
{
    public List<RoomsModel> Rooms { get; set; }
    public string CurrentViewName { get; set; }
    public string? Building { get; set; }
    public int? Floor { get; set; }
    public int? MinimumCapacity { get; set; }
    public bool HasScreen { get; set; }
    public bool HasWhiteboard { get; set; }

    public RoomsViewModel(List<RoomsModel> rooms, string currentViewName)
    {
        Rooms = rooms;
        CurrentViewName = currentViewName;
    }
}

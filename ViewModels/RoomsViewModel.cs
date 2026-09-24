using MVC.Models;

namespace MVC.ViewModels;

public class RoomsViewModel
{
    public List<RoomsModel> Rooms { get; set; }
    public string CurrentViewName { get; set; }

    public RoomsViewModel(List<RoomsModel> rooms, string currentViewName)
    {
        Rooms = rooms;
        CurrentViewName = currentViewName;
    }
}
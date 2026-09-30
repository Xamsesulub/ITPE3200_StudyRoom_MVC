using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels;

public class TimeSlotViewModel
{
    public string Value { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }
}

public class ReserveRoomViewModel
{
    public int RoomId { get; set; }

    public string RoomBuilding { get; set; } = string.Empty;

    public int RoomFloor { get; set; }

    public int RoomCapacity { get; set; }

    public bool RoomScreen { get; set; }

    public bool RoomWhiteboard { get; set; }

    [Display(Name = "Dato")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [Display(Name = "Tidspunkt")]
    [Required(ErrorMessage = "Velg et tidspunkt.")]
    public string SelectedSlot { get; set; } = string.Empty;

    [Display(Name = "Antall personer")]
    [Range(1, 100, ErrorMessage = "Antall personer må være minst 1.")]
    public int NumberOfPeople { get; set; } = 4;

    [Display(Name = "Formål")]
    [StringLength(100)]
    public string? Purpose { get; set; }

    public List<TimeSlotViewModel> Slots { get; set; } = new();
}

public class BookingListItemViewModel
{
    public int BookingId { get; set; }

    public int RoomId { get; set; }

    public string RoomBuilding { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string TimeSlot { get; set; } = string.Empty;
}

public class MyBookingsViewModel
{
    public List<BookingListItemViewModel> Upcoming { get; set; } = new();

    public List<BookingListItemViewModel> Past { get; set; } = new();
}

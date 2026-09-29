using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public class RoomsModel
{
    [Key]
    [Range(1, 999999, ErrorMessage = "Romnummer må være mellom 1 og 999999.")]
    [Display(Name = "Rom")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Bygg må fylles ut.")]
    [StringLength(100, ErrorMessage = "Bygg kan ikke være lengre enn 100 tegn.")]
    [Display(Name = "Bygg")]
    public string RoomBuilding { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "Etasje må være mellom 0 og 100.")]
    [Display(Name = "Etasje")]
    public int RoomFloor { get; set; }

    [Range(1, 1000, ErrorMessage = "Kapasitet må være mellom 1 og 1000.")]
    [Display(Name = "Kapasitet")]
    public int RoomCapacity { get; set; }

    [Display(Name = "Skjerm")]
    public bool RoomScreen { get; set; }

    [Display(Name = "Whiteboard")]
    public bool RoomWhiteboard { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public class RoomsModel
{
    //Data Annotation so EF knows this is a primary key
    [Key]
    public int RoomId { get; set; }

    public string RoomBuilding { get; set; }

    public int RoomFloor { get; set; }
    
    public int RoomCapacity { get; set; }

    public bool RoomScreen { get; set; }

    public bool RoomWhiteboard { get; set; }
}

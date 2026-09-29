
using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class BookingModel
    {
        [Key]
        public int BookingId;
        public int RoomId;
        public int UserId;

        public DateTime StartTime;
        public DateTime EndTime;
    }
}
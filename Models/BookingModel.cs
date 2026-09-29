
using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class BookingModel
    {
        [Key]
        public int BookingId {get;set;}
        public int RoomId {get;set;}
        public int UserId {get;set;}

        public DateTime StartTime {get;set;}
        public DateTime EndTime {get;set;}
    }
}
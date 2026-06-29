using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace staymanager_pj.Models
{
    public class Room
    {
        public int Id { get; set; }

        public string RoomNumber { get; set; }

        public string RoomType { get; set; }

        public decimal BasePrice { get; set; }

        public decimal PricePerNight { get; set; }

        public int Capacity { get; set; }

        public RoomStatus Status { get; set; }

        public string Description { get; set; }

        public string ImagePath { get; set; }

        public DateTime CreatedAt { get; set; }

        public Room()
        {
            RoomNumber = string.Empty;
            RoomType = string.Empty;
            Description = string.Empty;
            ImagePath = string.Empty;
            Status = RoomStatus.Available;
            CreatedAt = DateTime.Now;
        }
    }
}


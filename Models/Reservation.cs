using System;

namespace staymanager_pj.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int RoomId { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int NumberOfGuests { get; set; }

        public ReservationStatus Status { get; set; }

        public decimal TotalPrice { get; set; }

        public string Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public Reservation()
        {
            CheckInDate = DateTime.Now;
            CheckOutDate = DateTime.Now.AddDays(1);
            NumberOfGuests = 1;
            Status = ReservationStatus.Pending;
            Note = string.Empty;
            CreatedAt = DateTime.Now;
        }
    }
}


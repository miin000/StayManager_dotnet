using staymanager_pj.Data;
using staymanager_pj.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace staymanager_pj.Services
{
    internal class ReservationService
    {
        public List<Reservation> GetAll()
        {
            using (var db = new AppDbContext())
            {
                return db.Reservations.OrderByDescending(x => x.CreatedAt).ToList();
            }
        }

        public Reservation GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                return db.Reservations.Find(id);
            }
        }

        public List<Reservation> GetByCustomer(int customerId)
        {
            using (var db = new AppDbContext())
            {
                return db.Reservations.Where(x => x.CustomerId == customerId).OrderByDescending(x => x.CreatedAt).ToList();
            }
        }

        public Reservation CreateReservation(Customer customer, int roomId, DateTime checkIn, DateTime checkOut, int guests, string note)
        {
            using (var db = new AppDbContext())
            {
                Customer dbCustomer = null;
                if (!string.IsNullOrWhiteSpace(customer.IdentityNumber))
                {
                    dbCustomer = db.Customers.FirstOrDefault(x => x.IdentityNumber == customer.IdentityNumber);
                }
                if (dbCustomer == null && !string.IsNullOrWhiteSpace(customer.PhoneNumber))
                {
                    dbCustomer = db.Customers.FirstOrDefault(x => x.PhoneNumber == customer.PhoneNumber);
                }
                if (dbCustomer == null)
                {
                    dbCustomer = customer;
                    db.Customers.Add(dbCustomer);
                    db.SaveChanges();
                }

                var room = db.Rooms.Find(roomId) ?? db.Rooms.OrderBy(x => x.Id).FirstOrDefault();
                if (room == null)
                {
                    throw new InvalidOperationException("Chưa có phòng trong hệ thống.");
                }

                var nights = Math.Max(1, (checkOut.Date - checkIn.Date).Days);
                var reservation = new Reservation
                {
                    CustomerId = dbCustomer.Id,
                    RoomId = room.Id,
                    CheckInDate = checkIn.Date,
                    CheckOutDate = checkOut.Date,
                    NumberOfGuests = guests,
                    Status = ReservationStatus.Confirmed,
                    TotalPrice = room.PricePerNight * nights,
                    Note = note,
                    CreatedAt = DateTime.Now
                };
                db.Reservations.Add(reservation);
                room.Status = RoomStatus.Reserved;
                db.SaveChanges();

                var invoice = new Invoice
                {
                    InvoiceCode = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    ReservationId = reservation.Id,
                    CustomerId = dbCustomer.Id,
                    RoomId = room.Id,
                    RoomCharge = reservation.TotalPrice,
                    ServiceCharge = reservation.TotalPrice * 0.1m,
                    Discount = 0,
                    Status = InvoiceStatus.Unpaid,
                    PaymentMethod = PaymentMethod.None,
                    CreatedAt = DateTime.Now
                };
                invoice.CalculateTotal();
                db.Invoices.Add(invoice);
                db.SaveChanges();

                return reservation;
            }
        }

        public void UpdateStatus(int reservationId, ReservationStatus status)
        {
            using (var db = new AppDbContext())
            {
                var reservation = db.Reservations.Find(reservationId);
                if (reservation == null) return;
                reservation.Status = status;
                var room = db.Rooms.Find(reservation.RoomId);
                if (room != null)
                {
                    if (status == ReservationStatus.CheckedIn) room.Status = RoomStatus.Occupied;
                    if (status == ReservationStatus.CheckedOut || status == ReservationStatus.Cancelled) room.Status = RoomStatus.Available;
                    if (status == ReservationStatus.Confirmed) room.Status = RoomStatus.Reserved;
                }
                db.SaveChanges();
            }
        }

        public void Cancel(int reservationId)
        {
            UpdateStatus(reservationId, ReservationStatus.Cancelled);
        }
    }
}


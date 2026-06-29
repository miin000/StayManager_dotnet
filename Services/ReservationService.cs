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
        public List<FrontDeskReservation> GetFrontDeskReservations()
        {
            if (!App.IsDatabaseAvailable)
            {
                return new List<FrontDeskReservation>();
            }

            using (var db = new AppDbContext())
            {
                var reservations = db.Reservations
                    .Where(x => x.Status == ReservationStatus.Pending
                             || x.Status == ReservationStatus.Confirmed
                             || x.Status == ReservationStatus.CheckedIn)
                    .OrderBy(x => x.CheckInDate)
                    .ThenBy(x => x.CheckOutDate)
                    .ToList();

                var customerIds = reservations.Select(x => x.CustomerId).Distinct().ToList();
                var roomIds = reservations.Select(x => x.RoomId).Distinct().ToList();
                var reservationIds = reservations.Select(x => x.Id).ToList();

                var customers = db.Customers
                    .Where(x => customerIds.Contains(x.Id))
                    .ToList()
                    .ToDictionary(x => x.Id);

                var rooms = db.Rooms
                    .Where(x => roomIds.Contains(x.Id))
                    .ToList()
                    .ToDictionary(x => x.Id);

                var invoices = db.Invoices
                    .Where(x => reservationIds.Contains(x.ReservationId))
                    .ToList()
                    .GroupBy(x => x.ReservationId)
                    .ToDictionary(x => x.Key, x => x.OrderByDescending(i => i.CreatedAt).First());

                return reservations.Select(reservation =>
                {
                    Customer customer;
                    Room room;
                    Invoice invoice;
                    customers.TryGetValue(reservation.CustomerId, out customer);
                    rooms.TryGetValue(reservation.RoomId, out room);
                    invoices.TryGetValue(reservation.Id, out invoice);

                    return new FrontDeskReservation
                    {
                        ReservationId = reservation.Id,
                        CustomerId = reservation.CustomerId,
                        CustomerName = customer != null ? customer.FullName : "Khách #" + reservation.CustomerId,
                        PhoneNumber = customer != null ? customer.PhoneNumber : string.Empty,
                        RoomId = reservation.RoomId,
                        RoomNumber = room != null ? room.RoomNumber : reservation.RoomId.ToString(),
                        RoomType = room != null ? room.RoomType : string.Empty,
                        CheckInDate = reservation.CheckInDate,
                        CheckOutDate = reservation.CheckOutDate,
                        NumberOfGuests = reservation.NumberOfGuests,
                        Status = reservation.Status,
                        TotalPrice = reservation.TotalPrice,
                        InvoiceId = invoice != null ? invoice.Id : 0,
                        InvoiceCode = invoice != null ? invoice.InvoiceCode : string.Empty,
                        InvoiceStatus = invoice != null ? invoice.Status : InvoiceStatus.Unpaid
                    };
                }).ToList();
            }
        }

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

    public class FrontDeskReservation
    {
        public int ReservationId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string PhoneNumber { get; set; }

        public int RoomId { get; set; }

        public string RoomNumber { get; set; }

        public string RoomType { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int NumberOfGuests { get; set; }

        public ReservationStatus Status { get; set; }

        public decimal TotalPrice { get; set; }

        public int InvoiceId { get; set; }

        public string InvoiceCode { get; set; }

        public InvoiceStatus InvoiceStatus { get; set; }

        public bool CanCheckIn
        {
            get { return Status == ReservationStatus.Pending || Status == ReservationStatus.Confirmed; }
        }

        public bool CanCheckOut
        {
            get { return Status == ReservationStatus.CheckedIn; }
        }
    }
}


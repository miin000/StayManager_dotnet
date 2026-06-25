using System;

namespace staymanager_pj.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public string InvoiceCode { get; set; }

        public int ReservationId { get; set; }

        public int CustomerId { get; set; }

        public int RoomId { get; set; }

        public decimal RoomCharge { get; set; }

        public decimal ServiceCharge { get; set; }

        public decimal Discount { get; set; }

        public decimal TotalAmount { get; set; }

        public InvoiceStatus Status { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? PaidAt { get; set; }

        public int? CreatedByEmployeeId { get; set; }

        public int? ConfirmedByEmployeeId { get; set; }

        public Invoice()
        {
            InvoiceCode = string.Empty;
            Status = InvoiceStatus.Unpaid;
            PaymentMethod = PaymentMethod.None;
            CreatedAt = DateTime.Now;
        }

        public void CalculateTotal()
        {
            TotalAmount = RoomCharge + ServiceCharge - Discount;

            if (TotalAmount < 0)
            {
                TotalAmount = 0;
            }
        }

        public void ConfirmPayment(PaymentMethod paymentMethod, int employeeId)
        {
            Status = InvoiceStatus.Paid;
            PaymentMethod = paymentMethod;
            PaidAt = DateTime.Now;
            ConfirmedByEmployeeId = employeeId;
        }
    }
}

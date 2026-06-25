using System;

namespace staymanager_pj.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }

        public string ItemName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public string Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public InvoiceItem()
        {
            ItemName = string.Empty;
            Note = string.Empty;
            Quantity = 1;
            CreatedAt = DateTime.Now;
        }

        public void CalculateTotal()
        {
            TotalPrice = Quantity * UnitPrice;
        }
    }
}

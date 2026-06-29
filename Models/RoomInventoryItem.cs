using System;

namespace staymanager_pj.Models
{
    public class RoomInventoryItem
    {
        public int Id { get; set; }

        public int RoomId { get; set; }

        public int InventoryItemId { get; set; }

        public string ItemName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public RoomInventoryItem()
        {
            ItemName = string.Empty;
            Quantity = 1;
            CreatedAt = DateTime.Now;
        }

        public void CalculateTotal()
        {
            TotalPrice = Quantity * UnitPrice;
        }
    }
}

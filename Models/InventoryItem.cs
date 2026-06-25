using System;

namespace staymanager_pj.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }

        public string ItemCode { get; set; }

        public string ItemName { get; set; }

        public string Category { get; set; }

        public int Quantity { get; set; }

        public int MinimumQuantity { get; set; }

        public string Unit { get; set; }

        public decimal ImportPrice { get; set; }

        public decimal SellingPrice { get; set; }

        public InventoryStatus Status { get; set; }

        public string Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public InventoryItem()
        {
            ItemCode = string.Empty;
            ItemName = string.Empty;
            Category = string.Empty;
            Unit = string.Empty;
            Note = string.Empty;
            Status = InventoryStatus.Available;
            CreatedAt = DateTime.Now;
        }

        public void UpdateStatus()
        {
            if (Quantity <= 0)
            {
                Status = InventoryStatus.OutOfStock;
            }
            else if (Quantity <= MinimumQuantity)
            {
                Status = InventoryStatus.LowStock;
            }
            else
            {
                Status = InventoryStatus.Available;
            }
        }
    }
}

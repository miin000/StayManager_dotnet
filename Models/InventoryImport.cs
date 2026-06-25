using System;

namespace staymanager_pj.Models
{
    public class InventoryImport
    {
        public int Id { get; set; }

        public string ImportCode { get; set; }

        public int InventoryItemId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public string SupplierName { get; set; }

        public string Note { get; set; }

        public DateTime ImportDate { get; set; }

        public int CreatedByEmployeeId { get; set; }

        public InventoryImport()
        {
            ImportCode = string.Empty;
            SupplierName = string.Empty;
            Note = string.Empty;
            ImportDate = DateTime.Now;
        }

        public void CalculateTotal()
        {
            TotalPrice = Quantity * UnitPrice;
        }
    }
}

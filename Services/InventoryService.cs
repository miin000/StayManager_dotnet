using staymanager_pj.Data;
using staymanager_pj.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace staymanager_pj.Services
{
    internal class InventoryService
    {
        public List<InventoryItem> GetAll()
        {
            using (var db = new AppDbContext())
            {
                return db.InventoryItems.OrderBy(x => x.ItemName).ToList();
            }
        }

        public InventoryItem GetById(int id)
        {
            using (var db = new AppDbContext())
            {
                return db.InventoryItems.Find(id);
            }
        }

        public void Save(InventoryItem item)
        {
            item.UpdateStatus();
            using (var db = new AppDbContext())
            {
                if (item.Id == 0)
                {
                    if (string.IsNullOrWhiteSpace(item.ItemCode))
                    {
                        item.ItemCode = "VT" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    }
                    db.InventoryItems.Add(item);
                }
                else
                {
                    var existing = db.InventoryItems.Find(item.Id);
                    if (existing == null) return;
                    existing.ItemCode = item.ItemCode;
                    existing.ItemName = item.ItemName;
                    existing.Category = item.Category;
                    existing.Quantity = item.Quantity;
                    existing.MinimumQuantity = item.MinimumQuantity;
                    existing.Unit = item.Unit;
                    existing.ImportPrice = item.ImportPrice;
                    existing.SellingPrice = item.SellingPrice;
                    existing.Status = item.Status;
                    existing.Note = item.Note;
                }
                db.SaveChanges();
            }
        }

        public void ImportStock(int itemId, int quantity, decimal unitPrice, string supplier, string note)
        {
            using (var db = new AppDbContext())
            {
                var item = db.InventoryItems.Find(itemId);
                if (item == null) return;
                item.Quantity += quantity;
                item.ImportPrice = unitPrice;
                item.UpdateStatus();

                var import = new InventoryImport
                {
                    ImportCode = "NK" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    InventoryItemId = itemId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    SupplierName = supplier,
                    Note = note,
                    ImportDate = DateTime.Now
                };
                import.CalculateTotal();
                db.InventoryImports.Add(import);
                db.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            using (var db = new AppDbContext())
            {
                var item = db.InventoryItems.Find(id);
                if (item == null) return;
                db.InventoryItems.Remove(item);
                db.SaveChanges();
            }
        }
    }
}


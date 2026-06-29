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
        private static int _nextFallbackId = 4;

        private static readonly List<InventoryItem> FallbackItems = new List<InventoryItem>
        {
            new InventoryItem { Id = 1, ItemCode = "VT001", ItemName = "Khăn tắm", Category = "Buồng phòng", Unit = "Cái", Quantity = 100, MinimumQuantity = 20, ImportPrice = 60000, SellingPrice = 90000, Status = InventoryStatus.Available },
            new InventoryItem { Id = 2, ItemCode = "VT002", ItemName = "Nước suối", Category = "Minibar", Unit = "Chai", Quantity = 200, MinimumQuantity = 50, ImportPrice = 5000, SellingPrice = 15000, Status = InventoryStatus.Available },
            new InventoryItem { Id = 3, ItemCode = "VT003", ItemName = "Dầu gội", Category = "Tiện nghi", Unit = "Chai", Quantity = 80, MinimumQuantity = 20, ImportPrice = 25000, SellingPrice = 45000, Status = InventoryStatus.Available }
        };

        public List<InventoryItem> GetAll()
        {
            if (!App.IsDatabaseAvailable)
            {
                return GetFallbackItems();
            }

            using (var db = new AppDbContext())
            {
                try
                {
                    return db.InventoryItems.OrderBy(x => x.ItemName).ToList();
                }
                catch
                {
                    return GetFallbackItems();
                }
            }
        }

        public List<InventoryItem> Search(string keyword)
        {
            keyword = (keyword ?? string.Empty).Trim().ToLower();

            if (!App.IsDatabaseAvailable)
            {
                return SearchFallback(keyword);
            }

            using (var db = new AppDbContext())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(keyword))
                    {
                        return db.InventoryItems.OrderBy(x => x.ItemName).ToList();
                    }

                    return db.InventoryItems
                        .Where(x => (x.ItemCode ?? string.Empty).ToLower().Contains(keyword)
                                 || (x.ItemName ?? string.Empty).ToLower().Contains(keyword)
                                 || (x.Category ?? string.Empty).ToLower().Contains(keyword))
                        .OrderBy(x => x.ItemName)
                        .ToList();
                }
                catch
                {
                    return SearchFallback(keyword);
                }
            }
        }

        public InventoryItem GetById(int id)
        {
            if (!App.IsDatabaseAvailable)
            {
                return CloneItem(FallbackItems.FirstOrDefault(x => x.Id == id));
            }

            using (var db = new AppDbContext())
            {
                try
                {
                    return db.InventoryItems.Find(id);
                }
                catch
                {
                    return CloneItem(FallbackItems.FirstOrDefault(x => x.Id == id));
                }
            }
        }

        public void Save(InventoryItem item)
        {
            item.UpdateStatus();

            if (!App.IsDatabaseAvailable)
            {
                SaveFallback(item);
                return;
            }

            using (var db = new AppDbContext())
            {
                try
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
                catch
                {
                    SaveFallback(item);
                }
            }
        }

        public void ImportStock(int itemId, int quantity, decimal unitPrice, string supplier, string note)
        {
            if (!App.IsDatabaseAvailable)
            {
                ImportFallback(itemId, quantity, unitPrice);
                return;
            }

            using (var db = new AppDbContext())
            {
                try
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
                catch
                {
                    ImportFallback(itemId, quantity, unitPrice);
                }
            }
        }

        public void Delete(int id)
        {
            if (!App.IsDatabaseAvailable)
            {
                DeleteFallback(id);
                return;
            }

            using (var db = new AppDbContext())
            {
                try
                {
                    var item = db.InventoryItems.Find(id);
                    if (item == null) return;
                    db.InventoryItems.Remove(item);
                    db.SaveChanges();
                }
                catch
                {
                    DeleteFallback(id);
                }
            }
        }

        private static List<InventoryItem> GetFallbackItems()
        {
            return FallbackItems.OrderBy(x => x.ItemName).Select(CloneItem).ToList();
        }

        private static List<InventoryItem> SearchFallback(string keyword)
        {
            var query = FallbackItems.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => (x.ItemCode ?? string.Empty).ToLower().Contains(keyword)
                                      || (x.ItemName ?? string.Empty).ToLower().Contains(keyword)
                                      || (x.Category ?? string.Empty).ToLower().Contains(keyword));
            }

            return query.OrderBy(x => x.ItemName).Select(CloneItem).ToList();
        }

        private static void SaveFallback(InventoryItem item)
        {
            if (item.Id == 0)
            {
                item.Id = _nextFallbackId++;
                if (string.IsNullOrWhiteSpace(item.ItemCode))
                {
                    item.ItemCode = "VT" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }
                FallbackItems.Add(CloneItem(item));
                return;
            }

            var existing = FallbackItems.FirstOrDefault(x => x.Id == item.Id);
            if (existing == null)
            {
                return;
            }

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

        private static void ImportFallback(int itemId, int quantity, decimal unitPrice)
        {
            var item = FallbackItems.FirstOrDefault(x => x.Id == itemId);
            if (item == null)
            {
                return;
            }

            item.Quantity += quantity;
            item.ImportPrice = unitPrice;
            item.UpdateStatus();
        }

        private static void DeleteFallback(int id)
        {
            var item = FallbackItems.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                FallbackItems.Remove(item);
            }
        }

        private static InventoryItem CloneItem(InventoryItem item)
        {
            if (item == null)
            {
                return null;
            }

            return new InventoryItem
            {
                Id = item.Id,
                ItemCode = item.ItemCode,
                ItemName = item.ItemName,
                Category = item.Category,
                Quantity = item.Quantity,
                MinimumQuantity = item.MinimumQuantity,
                Unit = item.Unit,
                ImportPrice = item.ImportPrice,
                SellingPrice = item.SellingPrice,
                Status = item.Status,
                Note = item.Note,
                CreatedAt = item.CreatedAt
            };
        }
    }
}




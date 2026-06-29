using staymanager_pj.Data;
using staymanager_pj.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace staymanager_pj.Services
{
    public class RoomService
    {
        private static int _nextFallbackRoomId = 4;
        private static int _nextFallbackRoomItemId = 1;

        private static readonly List<Room> FallbackRooms = new List<Room>
        {
            new Room { Id = 1, RoomNumber = "101", RoomType = "Deluxe", BasePrice = 1500000, PricePerNight = 1500000, Capacity = 2, Status = RoomStatus.Available, Description = "Phòng Deluxe hướng vườn" },
            new Room { Id = 2, RoomNumber = "201", RoomType = "Premium", BasePrice = 2500000, PricePerNight = 2500000, Capacity = 3, Status = RoomStatus.Available, Description = "Phòng Premium hướng biển" },
            new Room { Id = 3, RoomNumber = "301", RoomType = "Royal Suite", BasePrice = 5000000, PricePerNight = 5000000, Capacity = 4, Status = RoomStatus.Available, Description = "Suite cao cấp đầy đủ tiện nghi" }
        };

        private static readonly List<RoomInventoryItem> FallbackRoomItems = new List<RoomInventoryItem>();

        public List<Room> GetAll()
        {
            if (!App.IsDatabaseAvailable)
            {
                return GetFallbackRooms();
            }

            using (var db = new AppDbContext())
            {
                try
                {
                    return db.Rooms
                        .OrderBy(x => x.RoomNumber)
                        .ToList();
                }
                catch
                {
                    return GetFallbackRooms();
                }
            }
        }

        public Room GetById(int id)
        {
            if (!App.IsDatabaseAvailable)
            {
                return FallbackRooms.FirstOrDefault(x => x.Id == id);
            }

            using (var db = new AppDbContext())
            {
                try
                {
                    return db.Rooms.Find(id);
                }
                catch
                {
                    return FallbackRooms.FirstOrDefault(x => x.Id == id);
                }
            }
        }

        public List<RoomInventoryItem> GetItems(int roomId)
        {
            if (!App.IsDatabaseAvailable)
            {
                return GetFallbackItems(roomId);
            }

            using (var db = new AppDbContext())
            {
                try
                {
                    return db.RoomInventoryItems
                        .Where(x => x.RoomId == roomId)
                        .OrderBy(x => x.ItemName)
                        .ToList();
                }
                catch
                {
                    return GetFallbackItems(roomId);
                }
            }
        }

        public void Save(Room room, List<RoomInventoryItem> items)
        {
            items = items ?? new List<RoomInventoryItem>();
            NormalizeRoomPrice(room, items);

            if (!App.IsDatabaseAvailable)
            {
                SaveFallback(room, items);
                return;
            }

            using (var db = new AppDbContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    Room savedRoom;
                    if (room.Id == 0)
                    {
                        savedRoom = room;
                        savedRoom.CreatedAt = DateTime.Now;
                        db.Rooms.Add(savedRoom);
                        db.SaveChanges();
                    }
                    else
                    {
                        savedRoom = db.Rooms.Find(room.Id);
                        if (savedRoom == null)
                        {
                            return;
                        }

                        savedRoom.RoomNumber = room.RoomNumber;
                        savedRoom.RoomType = room.RoomType;
                        savedRoom.BasePrice = room.BasePrice;
                        savedRoom.PricePerNight = room.PricePerNight;
                        savedRoom.Capacity = room.Capacity;
                        savedRoom.Status = room.Status;
                        savedRoom.Description = room.Description;
                        savedRoom.ImagePath = room.ImagePath;
                    }

                    var existingItems = db.RoomInventoryItems.Where(x => x.RoomId == savedRoom.Id).ToList();
                    db.RoomInventoryItems.RemoveRange(existingItems);

                    foreach (var item in items.Where(x => x.Quantity > 0))
                    {
                        item.RoomId = savedRoom.Id;
                        item.CalculateTotal();
                        db.RoomInventoryItems.Add(item);
                    }

                    db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
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
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var room = db.Rooms.Find(id);
                    if (room == null)
                    {
                        return;
                    }

                    var items = db.RoomInventoryItems.Where(x => x.RoomId == id).ToList();
                    db.RoomInventoryItems.RemoveRange(items);
                    db.Rooms.Remove(room);
                    db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdateStatus(int id, RoomStatus status)
        {
            if (!App.IsDatabaseAvailable)
            {
                UpdateFallbackStatus(id, status);
                return;
            }

            using (var db = new AppDbContext())
            {
                try
                {
                    var room = db.Rooms.Find(id);
                    if (room == null)
                    {
                        return;
                    }

                    room.Status = status;
                    db.SaveChanges();
                }
                catch
                {
                    UpdateFallbackStatus(id, status);
                    return;
                }
            }
        }

        private static List<Room> GetFallbackRooms()
        {
            return FallbackRooms
                .OrderBy(x => x.RoomNumber)
                .Select(CloneRoom)
                .ToList();
        }

        private static List<RoomInventoryItem> GetFallbackItems(int roomId)
        {
            return FallbackRoomItems
                .Where(x => x.RoomId == roomId)
                .OrderBy(x => x.ItemName)
                .Select(CloneRoomItem)
                .ToList();
        }

        private static void SaveFallback(Room room, List<RoomInventoryItem> items)
        {
            if (room.Id == 0)
            {
                room.Id = _nextFallbackRoomId++;
                room.CreatedAt = DateTime.Now;
                FallbackRooms.Add(CloneRoom(room));
            }
            else
            {
                var existing = FallbackRooms.FirstOrDefault(x => x.Id == room.Id);
                if (existing == null)
                {
                    return;
                }

                existing.RoomNumber = room.RoomNumber;
                existing.RoomType = room.RoomType;
                existing.BasePrice = room.BasePrice;
                existing.PricePerNight = room.PricePerNight;
                existing.Capacity = room.Capacity;
                existing.Status = room.Status;
                existing.Description = room.Description;
                existing.ImagePath = room.ImagePath;
            }

            FallbackRoomItems.RemoveAll(x => x.RoomId == room.Id);
            foreach (var item in items.Where(x => x.Quantity > 0))
            {
                item.Id = _nextFallbackRoomItemId++;
                item.RoomId = room.Id;
                item.CalculateTotal();
                FallbackRoomItems.Add(CloneRoomItem(item));
            }
        }

        private static void DeleteFallback(int id)
        {
            var room = FallbackRooms.FirstOrDefault(x => x.Id == id);
            if (room != null)
            {
                FallbackRooms.Remove(room);
            }

            FallbackRoomItems.RemoveAll(x => x.RoomId == id);
        }

        private static void UpdateFallbackStatus(int id, RoomStatus status)
        {
            var room = FallbackRooms.FirstOrDefault(x => x.Id == id);
            if (room != null)
            {
                room.Status = status;
            }
        }

        private static Room CloneRoom(Room room)
        {
            return new Room
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                BasePrice = room.BasePrice,
                PricePerNight = room.PricePerNight,
                Capacity = room.Capacity,
                Status = room.Status,
                Description = room.Description,
                ImagePath = room.ImagePath,
                CreatedAt = room.CreatedAt
            };
        }

        private static RoomInventoryItem CloneRoomItem(RoomInventoryItem item)
        {
            return new RoomInventoryItem
            {
                Id = item.Id,
                RoomId = item.RoomId,
                InventoryItemId = item.InventoryItemId,
                ItemName = item.ItemName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                CreatedAt = item.CreatedAt
            };
        }

        private static void NormalizeRoomPrice(Room room, List<RoomInventoryItem> items)
        {
            if (room.BasePrice <= 0 && room.PricePerNight > 0)
            {
                room.BasePrice = room.PricePerNight;
            }

            items = items ?? new List<RoomInventoryItem>();
            foreach (var item in items)
            {
                item.Quantity = Math.Max(1, item.Quantity);
                item.CalculateTotal();
            }

            room.PricePerNight = room.BasePrice + items.Where(x => x.Quantity > 0).Sum(x => x.TotalPrice);
        }
    }
}




using staymanager_pj.Data;
using staymanager_pj.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace staymanager_pj.Services
{
    public class RoomService
    {
        private static readonly List<Room> FallbackRooms = new List<Room>
        {
            new Room { Id = 1, RoomNumber = "101", RoomType = "Deluxe", PricePerNight = 1500000, Capacity = 2, Status = RoomStatus.Available, Description = "Phòng Deluxe hướng vườn" },
            new Room { Id = 2, RoomNumber = "201", RoomType = "Premium", PricePerNight = 2500000, Capacity = 3, Status = RoomStatus.Available, Description = "Phòng Premium hướng biển" },
            new Room { Id = 3, RoomNumber = "301", RoomType = "Royal Suite", PricePerNight = 5000000, Capacity = 4, Status = RoomStatus.Available, Description = "Suite cao cấp đầy đủ tiện nghi" }
        };

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
                PricePerNight = room.PricePerNight,
                Capacity = room.Capacity,
                Status = room.Status,
                Description = room.Description,
                ImagePath = room.ImagePath,
                CreatedAt = room.CreatedAt
            };
        }
    }
}




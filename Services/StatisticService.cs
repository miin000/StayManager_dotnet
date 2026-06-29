using staymanager_pj.Data;
using staymanager_pj.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace staymanager_pj.Services
{
    internal class StatisticService
    {
        public DashboardStatistic GetDashboardStatistic()
        {
            try
            {
                if (App.IsDatabaseAvailable)
                {
                    using (var db = new AppDbContext())
                    {
                        var today = DateTime.Today;
                        var tomorrow = today.AddDays(1);
                        var monthStart = new DateTime(today.Year, today.Month, 1);
                        var nextMonth = monthStart.AddMonths(1);

                        var rooms = db.Rooms.ToList();
                        var reservations = db.Reservations.ToList();
                        var invoices = db.Invoices.Where(x => x.Status == InvoiceStatus.Paid).ToList();
                        var inventoryItems = db.InventoryItems.ToList();

                        return new DashboardStatistic
                        {
                            TodayRevenue = invoices.Where(x => GetRevenueDate(x) >= today && GetRevenueDate(x) < tomorrow).Sum(x => x.TotalAmount),
                            MonthlyRevenue = invoices.Where(x => GetRevenueDate(x) >= monthStart && GetRevenueDate(x) < nextMonth).Sum(x => x.TotalAmount),
                            TotalRooms = rooms.Count,
                            AvailableRooms = rooms.Count(x => x.Status == RoomStatus.Available),
                            OccupiedRooms = rooms.Count(x => x.Status == RoomStatus.Occupied),
                            ReservedRooms = rooms.Count(x => x.Status == RoomStatus.Reserved),
                            MaintenanceRooms = rooms.Count(x => x.Status == RoomStatus.Maintenance),
                            TodayCheckIns = reservations.Count(x => x.CheckInDate >= today && x.CheckInDate < tomorrow && x.Status != ReservationStatus.Cancelled),
                            TodayCheckOuts = reservations.Count(x => x.CheckOutDate >= today && x.CheckOutDate < tomorrow && x.Status != ReservationStatus.Cancelled),
                            PendingReservations = reservations.Count(x => x.Status == ReservationStatus.Pending),
                            TotalCustomers = db.Customers.Count(),
                            LowStockItems = inventoryItems.Count(IsLowStock)
                        };
                    }
                }
            }
            catch
            {
            }

            return BuildFallbackDashboardStatistic();
        }

        public List<RevenueStatistic> GetRevenueStatistics(int days = 7)
        {
            days = Math.Max(1, days);
            var end = DateTime.Today;
            var start = end.AddDays(-(days - 1));

            try
            {
                if (App.IsDatabaseAvailable)
                {
                    using (var db = new AppDbContext())
                    {
                        var invoices = db.Invoices
                            .Where(x => x.Status == InvoiceStatus.Paid && x.CreatedAt >= start)
                            .ToList();

                        return Enumerable.Range(0, days)
                            .Select(i => start.AddDays(i))
                            .Select(date => new RevenueStatistic
                            {
                                Date = date,
                                TotalRevenue = invoices.Where(x => GetRevenueDate(x).Date == date.Date).Sum(x => x.TotalAmount),
                                InvoiceCount = invoices.Count(x => GetRevenueDate(x).Date == date.Date)
                            })
                            .ToList();
                    }
                }
            }
            catch
            {
            }

            return Enumerable.Range(0, days)
                .Select(i => new RevenueStatistic { Date = start.AddDays(i), TotalRevenue = 0, InvoiceCount = 0 })
                .ToList();
        }

        public List<RoomOccupancyStatistic> GetRoomOccupancyStatistics()
        {
            var rooms = new RoomService().GetAll();

            return rooms
                .GroupBy(x => string.IsNullOrWhiteSpace(x.RoomType) ? "Khác" : x.RoomType)
                .OrderBy(x => x.Key)
                .Select(x =>
                {
                    var total = x.Count();
                    var occupied = x.Count(r => r.Status == RoomStatus.Occupied || r.Status == RoomStatus.Reserved);
                    return new RoomOccupancyStatistic
                    {
                        RoomType = x.Key,
                        TotalRooms = total,
                        OccupiedRooms = occupied,
                        OccupancyRate = total == 0 ? 0 : Math.Round(occupied * 100.0 / total, 1)
                    };
                })
                .ToList();
        }

        public InventoryStatistic GetInventoryStatistic()
        {
            var items = new InventoryService().GetAll();

            var statistic = new InventoryStatistic
            {
                TotalItems = items.Count,
                AvailableItems = items.Count(x => x.Status == InventoryStatus.Available && x.Quantity > 0),
                LowStockItems = items.Count(IsLowStock),
                OutOfStockItems = items.Count(x => x.Status == InventoryStatus.OutOfStock || x.Quantity <= 0),
                TotalImportCost = items.Sum(x => x.Quantity * x.ImportPrice)
            };

            try
            {
                if (App.IsDatabaseAvailable)
                {
                    using (var db = new AppDbContext())
                    {
                        var imports = db.InventoryImports.ToList();
                        if (imports.Any())
                        {
                            statistic.TotalImportCost = imports.Sum(x => x.TotalPrice);
                        }
                    }
                }
            }
            catch
            {
            }

            return statistic;
        }

        private static DashboardStatistic BuildFallbackDashboardStatistic()
        {
            var rooms = new RoomService().GetAll();
            var items = new InventoryService().GetAll();

            return new DashboardStatistic
            {
                TodayRevenue = 0,
                MonthlyRevenue = 0,
                TotalRooms = rooms.Count,
                AvailableRooms = rooms.Count(x => x.Status == RoomStatus.Available),
                OccupiedRooms = rooms.Count(x => x.Status == RoomStatus.Occupied),
                ReservedRooms = rooms.Count(x => x.Status == RoomStatus.Reserved),
                MaintenanceRooms = rooms.Count(x => x.Status == RoomStatus.Maintenance),
                TodayCheckIns = 0,
                TodayCheckOuts = 0,
                PendingReservations = 0,
                TotalCustomers = 0,
                LowStockItems = items.Count(IsLowStock)
            };
        }

        private static DateTime GetRevenueDate(Invoice invoice)
        {
            return invoice.PaidAt ?? invoice.CreatedAt;
        }

        private static bool IsLowStock(InventoryItem item)
        {
            return item.Status == InventoryStatus.LowStock || (item.Quantity > 0 && item.Quantity <= item.MinimumQuantity);
        }
    }
}



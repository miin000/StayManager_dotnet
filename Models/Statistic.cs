using System;

namespace staymanager_pj.Models
{
    public class DashboardStatistic
    {
        public decimal TodayRevenue { get; set; }

        public decimal MonthlyRevenue { get; set; }

        public int TotalRooms { get; set; }

        public int AvailableRooms { get; set; }

        public int OccupiedRooms { get; set; }

        public int ReservedRooms { get; set; }

        public int MaintenanceRooms { get; set; }

        public int TodayCheckIns { get; set; }

        public int TodayCheckOuts { get; set; }

        public int PendingReservations { get; set; }

        public int TotalCustomers { get; set; }

        public int LowStockItems { get; set; }
    }

    public class RevenueStatistic
    {
        public DateTime Date { get; set; }

        public decimal TotalRevenue { get; set; }

        public int InvoiceCount { get; set; }
    }

    public class RoomOccupancyStatistic
    {
        public string RoomType { get; set; }

        public int TotalRooms { get; set; }

        public int OccupiedRooms { get; set; }

        public double OccupancyRate { get; set; }

        public RoomOccupancyStatistic()
        {
            RoomType = string.Empty;
        }
    }

    public class InventoryStatistic
    {
        public int TotalItems { get; set; }

        public int AvailableItems { get; set; }

        public int LowStockItems { get; set; }

        public int OutOfStockItems { get; set; }

        public decimal TotalImportCost { get; set; }
    }
}

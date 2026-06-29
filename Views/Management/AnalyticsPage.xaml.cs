using staymanager_pj.Models;
using staymanager_pj.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace staymanager_pj.Views.Management
{
    public partial class AnalyticsPage : Window
    {
        private readonly StatisticService _statisticService = new StatisticService();

        public AnalyticsPage()
        {
            InitializeComponent();
            Loaded += AnalyticsPage_Loaded;
        }

        private void AnalyticsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                var dashboard = _statisticService.GetDashboardStatistic();
                var revenues = _statisticService.GetRevenueStatistics();
                var roomStats = _statisticService.GetRoomOccupancyStatistics();
                var inventory = _statisticService.GetInventoryStatistic();

                icDashboardStats.ItemsSource = BuildDashboardCards(dashboard);
                icRevenueStats.ItemsSource = revenues.Select(x => new RevenueDisplayItem(x)).ToList();
                icRoomStats.ItemsSource = roomStats.Select(x => new RoomStatDisplayItem(x)).ToList();
                icInventoryStats.ItemsSource = BuildInventoryCards(inventory);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được dữ liệu thống kê.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static List<StatCardItem> BuildDashboardCards(DashboardStatistic dashboard)
        {
            return new List<StatCardItem>
            {
                new StatCardItem("Doanh thu hôm nay", FormatCurrency(dashboard.TodayRevenue), "Tổng tiền hóa đơn đã thanh toán trong ngày."),
                new StatCardItem("Doanh thu tháng", FormatCurrency(dashboard.MonthlyRevenue), "Tổng tiền hóa đơn đã thanh toán trong tháng hiện tại."),
                new StatCardItem("Tổng số phòng", dashboard.TotalRooms.ToString(), "Số phòng hiện có trong hệ thống."),
                new StatCardItem("Phòng trống", dashboard.AvailableRooms.ToString(), "Sẵn sàng phục vụ khách mới."),
                new StatCardItem("Phòng đang sử dụng", dashboard.OccupiedRooms.ToString(), "Đang có khách lưu trú."),
                new StatCardItem("Phòng đã đặt", dashboard.ReservedRooms.ToString(), "Đã giữ chỗ, chờ check-in."),
                new StatCardItem("Phòng bảo trì", dashboard.MaintenanceRooms.ToString(), "Tạm ngưng để sửa chữa/bảo trì."),
                new StatCardItem("Check-in hôm nay", dashboard.TodayCheckIns.ToString(), "Lượt khách dự kiến nhận phòng hôm nay."),
                new StatCardItem("Check-out hôm nay", dashboard.TodayCheckOuts.ToString(), "Lượt khách dự kiến trả phòng hôm nay."),
                new StatCardItem("Đặt phòng chờ xử lý", dashboard.PendingReservations.ToString(), "Booking đang ở trạng thái chờ xác nhận."),
                new StatCardItem("Khách hàng", dashboard.TotalCustomers.ToString(), "Tổng hồ sơ khách hàng đã lưu."),
                new StatCardItem("Vật tư sắp hết", dashboard.LowStockItems.ToString(), "Cần bổ sung kho sớm.")
            };
        }

        private static List<StatCardItem> BuildInventoryCards(InventoryStatistic inventory)
        {
            return new List<StatCardItem>
            {
                new StatCardItem("Tổng mặt hàng", inventory.TotalItems.ToString(), "Số vật tư đang được quản lý."),
                new StatCardItem("Đang khả dụng", inventory.AvailableItems.ToString(), "Vật tư còn đủ số lượng sử dụng."),
                new StatCardItem("Sắp hết", inventory.LowStockItems.ToString(), "Mặt hàng chạm ngưỡng tối thiểu."),
                new StatCardItem("Hết hàng", inventory.OutOfStockItems.ToString(), "Cần nhập kho ngay."),
                new StatCardItem("Tổng chi phí nhập", FormatCurrency(inventory.TotalImportCost), "Cộng dồn giá trị nhập kho hiện có.")
            };
        }

        private static string FormatCurrency(decimal amount)
        {
            return string.Format("{0:N0} đ", amount);
        }

        private void Sidebar_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private class StatCardItem
        {
            public StatCardItem(string title, string value, string description)
            {
                Title = title;
                Value = value;
                Description = description;
            }

            public string Title { get; private set; }

            public string Value { get; private set; }

            public string Description { get; private set; }
        }

        private class RevenueDisplayItem
        {
            public RevenueDisplayItem(RevenueStatistic statistic)
            {
                Label = statistic.Date.ToString("dd/MM/yyyy");
                RevenueText = FormatCurrency(statistic.TotalRevenue);
                InvoiceCountText = statistic.InvoiceCount + " hóa đơn";
            }

            public string Label { get; private set; }

            public string RevenueText { get; private set; }

            public string InvoiceCountText { get; private set; }
        }

        private class RoomStatDisplayItem
        {
            public RoomStatDisplayItem(RoomOccupancyStatistic statistic)
            {
                RoomType = statistic.RoomType;
                OccupancyRateText = statistic.OccupancyRate.ToString("0.#") + "%";
                Summary = string.Format("{0}/{1} phòng đang khai thác hoặc đã đặt", statistic.OccupiedRooms, statistic.TotalRooms);
            }

            public string RoomType { get; private set; }

            public string OccupancyRateText { get; private set; }

            public string Summary { get; private set; }
        }
    }
}




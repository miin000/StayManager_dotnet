using staymanager_pj.Models;
using staymanager_pj.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace staymanager_pj.Views.Customer
{
    public partial class RoomDetailPage : Window
    {
        private readonly RoomService _roomService = new RoomService();
        private readonly int _roomId;
        private Room _room;
        private List<RoomInventoryItem> _items = new List<RoomInventoryItem>();

        public RoomDetailPage()
            : this(0)
        {
        }

        public RoomDetailPage(int roomId)
        {
            _roomId = roomId;
            InitializeComponent();
            Loaded += (s, e) => LoadRoom();
            btnBookNow.Click += BtnBookNow_Click;
        }

        private void LoadRoom()
        {
            try
            {
                _room = _roomId > 0
                    ? _roomService.GetById(_roomId)
                    : _roomService.GetAll().FirstOrDefault();

                if (_room == null)
                {
                    MessageBox.Show("Không tìm thấy phòng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                    return;
                }

                _items = _roomService.GetItems(_room.Id);
                BindRoom();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được chi tiết phòng.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void BindRoom()
        {
            var basePrice = _room.BasePrice > 0
                ? _room.BasePrice
                : Math.Max(0, _room.PricePerNight - _items.Sum(x => x.TotalPrice));
            var itemCharge = _items.Sum(x => x.TotalPrice);

            txtTitle.Text = string.Format("Phòng {0} - {1}", _room.RoomNumber, _room.RoomType);
            txtSubtitle.Text = string.Format("{0} khách | {1} | {2:N0} VNĐ / đêm", _room.Capacity, _room.Status, _room.PricePerNight);
            txtRoomNumber.Text = _room.RoomNumber;
            txtDescription.Text = string.IsNullOrWhiteSpace(_room.Description)
                ? "Phòng đang sẵn sàng phục vụ. Thông tin mô tả chi tiết sẽ được cập nhật thêm bởi bộ phận quản lý."
                : _room.Description;
            txtPrice.Text = string.Format(CultureInfo.CurrentCulture, "{0:N0} VNĐ", _room.PricePerNight);
            txtPriceBreakdown.Text = string.Format(
                CultureInfo.CurrentCulture,
                "Giá cơ bản {0:N0} VNĐ + đồ/vật tư {1:N0} VNĐ.",
                basePrice,
                itemCharge);
            txtStatus.Text = _room.Status.ToString();
            txtCapacity.Text = string.Format("{0} khách", _room.Capacity);

            icRoomItems.ItemsSource = _items
                .Select(x => new RoomItemDetailViewModel(x))
                .ToList();
            txtNoItems.Visibility = _items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            dpCheckIn.SelectedDate = DateTime.Today;
            dpCheckOut.SelectedDate = DateTime.Today.AddDays(1);
            cbGuests.ItemsSource = Enumerable.Range(1, Math.Max(1, _room.Capacity))
                .Select(x => string.Format("{0} khách", x))
                .ToList();
            cbGuests.SelectedIndex = 0;
            btnBookNow.IsEnabled = _room.Status == RoomStatus.Available;
        }

        private void BtnBookNow_Click(object sender, RoutedEventArgs e)
        {
            if (_room == null)
            {
                return;
            }

            if (_room.Status != RoomStatus.Available)
            {
                MessageBox.Show("Phòng hiện không ở trạng thái có thể đặt.");
                return;
            }

            MessageBox.Show(
                "Bạn có thể đặt phòng này từ màn hình đặt phòng. Thông tin phòng đang xem đã lấy từ dữ liệu quản lý.",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public class RoomItemDetailViewModel
        {
            public RoomItemDetailViewModel(RoomInventoryItem item)
            {
                ItemName = item.ItemName;
                DetailText = string.Format(CultureInfo.CurrentCulture, "{0} x {1:N0} VNĐ = {2:N0} VNĐ", item.Quantity, item.UnitPrice, item.TotalPrice);
            }

            public string ItemName { get; private set; }

            public string DetailText { get; private set; }
        }
    }
}

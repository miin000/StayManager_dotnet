using staymanager_pj.Models;
using staymanager_pj.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace staymanager_pj.Views.Customer
{
    public partial class RoomListPage : Window
    {
        private readonly RoomService _roomService = new RoomService();
        private List<RoomCardViewModel> _rooms = new List<RoomCardViewModel>();

        public RoomListPage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadRooms();
            btnApplyFilter.Click += (s, e) => ApplyFilter();
            btnResetFilter.Click += (s, e) => ResetFilter();
            txtSearch.KeyDown += TxtSearch_KeyDown;
            slPrice.ValueChanged += (s, e) =>
            {
                UpdatePriceFilterText();
                ApplyFilter();
            };
            cmbRoomType.SelectionChanged += (s, e) => ApplyFilter();
        }

        private void LoadRooms()
        {
            try
            {
                _rooms = _roomService.GetAll()
                    .Select(x => new RoomCardViewModel(x, _roomService.GetItems(x.Id)))
                    .ToList();

                BindRoomTypes();
                ResetPriceRange();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được danh sách phòng.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BindRoomTypes()
        {
            var selected = cmbRoomType.SelectedItem as string;
            var types = new List<string> { "Tất cả" };
            types.AddRange(_rooms
                .Select(x => x.RoomType)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x));

            cmbRoomType.ItemsSource = types;
            cmbRoomType.SelectedItem = types.Contains(selected) ? selected : "Tất cả";
        }

        private void ResetPriceRange()
        {
            var maxPrice = _rooms.Count == 0 ? 10000000m : Math.Max(1, _rooms.Max(x => x.PricePerNight));
            slPrice.Maximum = (double)maxPrice;
            slPrice.Value = (double)maxPrice;
            UpdatePriceFilterText();
        }

        private void ApplyFilter()
        {
            if (icRooms == null || _rooms == null)
            {
                return;
            }

            var keyword = (txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();
            var selectedType = cmbRoomType.SelectedItem as string;
            var maxPrice = (decimal)slPrice.Value;

            var result = _rooms.Where(x => x.PricePerNight <= maxPrice);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                result = result.Where(x =>
                    (x.RoomNumber ?? string.Empty).ToLowerInvariant().Contains(keyword)
                    || (x.RoomType ?? string.Empty).ToLowerInvariant().Contains(keyword)
                    || (x.Description ?? string.Empty).ToLowerInvariant().Contains(keyword)
                    || (x.ItemSummary ?? string.Empty).ToLowerInvariant().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(selectedType) && selectedType != "Tất cả")
            {
                result = result.Where(x => x.RoomType == selectedType);
            }

            var filtered = result.ToList();
            icRooms.ItemsSource = filtered;
            txtEmpty.Visibility = filtered.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ResetFilter()
        {
            txtSearch.Clear();
            cmbRoomType.SelectedItem = "Tất cả";
            ResetPriceRange();
            ApplyFilter();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyFilter();
            }
        }

        private void BtnDetail_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (sender as FrameworkElement)?.DataContext as RoomCardViewModel;
            if (viewModel == null)
            {
                return;
            }

            var detail = new RoomDetailPage(viewModel.RoomId);
            detail.Owner = this;
            detail.ShowDialog();
        }

        private void UpdatePriceFilterText()
        {
            if (txtPriceFilter != null)
            {
                txtPriceFilter.Text = string.Format(CultureInfo.CurrentCulture, "Tối đa {0:N0} VNĐ", slPrice.Value);
            }
        }

        public class RoomCardViewModel
        {
            public RoomCardViewModel(Room room, List<RoomInventoryItem> items)
            {
                RoomId = room.Id;
                RoomNumber = room.RoomNumber;
                RoomType = room.RoomType;
                PricePerNight = room.PricePerNight;
                Description = string.IsNullOrWhiteSpace(room.Description) ? "Phòng đang sẵn sàng phục vụ." : room.Description;
                Summary = string.Format("{0} khách | {1}", room.Capacity, room.Status);
                StatusText = room.Status == RoomStatus.Available ? "Có thể đặt" : "Trạng thái: " + room.Status;
                ItemSummary = items.Count == 0
                    ? string.Empty
                    : string.Join(", ", items.Select(x => string.Format("{0} x{1}", x.ItemName, x.Quantity)));
            }

            public int RoomId { get; private set; }

            public string RoomNumber { get; private set; }

            public string RoomType { get; private set; }

            public decimal PricePerNight { get; private set; }

            public string PriceText
            {
                get { return string.Format(CultureInfo.CurrentCulture, "{0:N0} VNĐ / đêm", PricePerNight); }
            }

            public string Description { get; private set; }

            public string Summary { get; private set; }

            public string StatusText { get; private set; }

            public string ItemSummary { get; private set; }
        }
    }
}

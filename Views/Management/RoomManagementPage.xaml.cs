using staymanager_pj.Models;
using staymanager_pj.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace staymanager_pj.Views.Management
{
    public partial class RoomManagementPage : Window
    {
        private readonly RoomService _roomService = new RoomService();
        private readonly InventoryService _inventoryService = new InventoryService();
        private List<RoomListItemViewModel> _rooms = new List<RoomListItemViewModel>();
        private List<RoomItemSelectionViewModel> _itemSelections = new List<RoomItemSelectionViewModel>();
        private int _selectedId;

        public RoomManagementPage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadData();
            dgRooms.SelectionChanged += (s, e) => FillForm(dgRooms.SelectedItem as RoomListItemViewModel);
            txtSearch.KeyDown += TxtSearch_KeyDown;
            txtBasePrice.TextChanged += (s, e) => UpdateCalculatedPrice();
            btnRefresh.Click += (s, e) => LoadData();
            btnSearch.Click += (s, e) => ApplySearch();
            btnNew.Click += (s, e) => ClearForm();
            btnCalculate.Click += (s, e) => UpdateCalculatedPrice();
            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
            cmbStatus.ItemsSource = Enum.GetValues(typeof(RoomStatus)).Cast<RoomStatus>().ToList();
        }

        private void LoadData()
        {
            try
            {
                _rooms = _roomService.GetAll()
                    .Select(x => new RoomListItemViewModel(x, _roomService.GetItems(x.Id)))
                    .ToList();

                dgRooms.ItemsSource = _rooms;
                RestoreSelection();
                if (_selectedId == 0)
                {
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được danh sách phòng.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySearch()
        {
            var keyword = (txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();
            dgRooms.ItemsSource = string.IsNullOrWhiteSpace(keyword)
                ? _rooms
                : _rooms.Where(x =>
                    (x.Room.RoomNumber ?? string.Empty).ToLowerInvariant().Contains(keyword)
                    || (x.Room.RoomType ?? string.Empty).ToLowerInvariant().Contains(keyword)
                    || (x.ItemSummary ?? string.Empty).ToLowerInvariant().Contains(keyword))
                    .ToList();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplySearch();
            }
        }

        private void FillForm(RoomListItemViewModel viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            var room = viewModel.Room;
            _selectedId = room.Id;
            txtRoomNumber.Text = room.RoomNumber;
            txtRoomType.Text = room.RoomType;
            txtBasePrice.Text = GetBasePrice(room, viewModel.ItemCharge).ToString("0.##");
            txtCapacity.Text = room.Capacity.ToString();
            cmbStatus.SelectedItem = room.Status;
            txtImagePath.Text = room.ImagePath;
            txtDescription.Text = room.Description;
            LoadItemSelections(room.Id);
            UpdateCalculatedPrice();
        }

        private void ClearForm()
        {
            _selectedId = 0;
            txtRoomNumber.Clear();
            txtRoomType.Clear();
            txtBasePrice.Text = "0";
            txtCapacity.Text = "1";
            cmbStatus.SelectedItem = RoomStatus.Available;
            txtImagePath.Clear();
            txtDescription.Clear();
            dgRooms.SelectedItem = null;
            LoadItemSelections(0);
            UpdateCalculatedPrice();
        }

        private void LoadItemSelections(int roomId)
        {
            var assignedItems = roomId > 0 ? _roomService.GetItems(roomId) : new List<RoomInventoryItem>();
            var assignedByItemId = assignedItems.ToDictionary(x => x.InventoryItemId);
            var selections = new List<RoomItemSelectionViewModel>();

            foreach (var inventory in _inventoryService.GetAll())
            {
                RoomInventoryItem assigned;
                assignedByItemId.TryGetValue(inventory.Id, out assigned);

                selections.Add(new RoomItemSelectionViewModel
                {
                    InventoryItemId = inventory.Id,
                    ItemName = inventory.ItemName,
                    UnitPrice = assigned != null ? assigned.UnitPrice : inventory.SellingPrice,
                    Quantity = assigned != null ? assigned.Quantity : 1,
                    IsSelected = assigned != null
                });
            }

            _itemSelections = selections.OrderBy(x => x.ItemName).ToList();
            dgRoomItems.ItemsSource = _itemSelections;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!TryBuildRoom(out var room, out var items))
            {
                return;
            }

            try
            {
                _roomService.Save(room, items);
                _selectedId = room.Id;
                LoadData();
                MessageBox.Show("Lưu phòng thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không lưu được phòng.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == 0)
            {
                return;
            }

            if (MessageBox.Show("Xóa phòng đang chọn?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _roomService.Delete(_selectedId);
                _selectedId = 0;
                LoadData();
                MessageBox.Show("Đã xóa phòng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không xóa được phòng. Có thể phòng đã có đặt phòng/hóa đơn.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgRoomItems_CurrentCellChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(UpdateCalculatedPrice));
        }

        private bool TryBuildRoom(out Room room, out List<RoomInventoryItem> items)
        {
            room = null;
            items = null;

            if (string.IsNullOrWhiteSpace(txtRoomNumber.Text))
            {
                MessageBox.Show("Nhập số phòng.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtRoomType.Text))
            {
                MessageBox.Show("Nhập loại phòng.");
                return false;
            }

            var basePrice = ReadDecimal(txtBasePrice.Text);
            var capacity = ReadInt(txtCapacity.Text);
            if (basePrice < 0)
            {
                MessageBox.Show("Giá cơ bản không được âm.");
                return false;
            }

            if (capacity <= 0)
            {
                MessageBox.Show("Sức chứa phải lớn hơn 0.");
                return false;
            }

            items = _itemSelections
                .Where(x => x.IsSelected)
                .Select(x => new RoomInventoryItem
                {
                    InventoryItemId = x.InventoryItemId,
                    ItemName = x.ItemName,
                    Quantity = Math.Max(1, x.Quantity),
                    UnitPrice = x.UnitPrice
                })
                .ToList();

            room = new Room
            {
                Id = _selectedId,
                RoomNumber = txtRoomNumber.Text.Trim(),
                RoomType = txtRoomType.Text.Trim(),
                BasePrice = basePrice,
                PricePerNight = basePrice + items.Sum(x => Math.Max(1, x.Quantity) * x.UnitPrice),
                Capacity = capacity,
                Status = cmbStatus.SelectedItem is RoomStatus ? (RoomStatus)cmbStatus.SelectedItem : RoomStatus.Available,
                ImagePath = txtImagePath.Text.Trim(),
                Description = txtDescription.Text.Trim()
            };

            return true;
        }

        private void UpdateCalculatedPrice()
        {
            if (txtCalculatedPrice == null)
            {
                return;
            }

            var basePrice = ReadDecimal(txtBasePrice.Text);
            var itemCharge = _itemSelections
                .Where(x => x.IsSelected)
                .Sum(x => Math.Max(1, x.Quantity) * x.UnitPrice);

            txtCalculatedPrice.Text = (basePrice + itemCharge).ToString("N0");
        }

        private void RestoreSelection()
        {
            if (_selectedId == 0)
            {
                return;
            }

            var selected = _rooms.FirstOrDefault(x => x.Room.Id == _selectedId);
            if (selected != null)
            {
                dgRooms.SelectedItem = selected;
            }
        }

        private static decimal GetBasePrice(Room room, decimal itemCharge)
        {
            if (room.BasePrice > 0)
            {
                return room.BasePrice;
            }

            return Math.Max(0, room.PricePerNight - itemCharge);
        }

        private static int ReadInt(string text)
        {
            int value;
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value) ? value : 0;
        }

        private static decimal ReadDecimal(string text)
        {
            decimal value;
            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value) ? value : 0m;
        }

        public class RoomListItemViewModel
        {
            public RoomListItemViewModel(Room room, List<RoomInventoryItem> items)
            {
                Room = room;
                ItemCharge = items.Sum(x => x.TotalPrice);
                ItemSummary = items.Count == 0
                    ? string.Empty
                    : string.Join(", ", items.Select(x => string.Format("{0} x{1}", x.ItemName, x.Quantity)));
            }

            public Room Room { get; private set; }

            public decimal ItemCharge { get; private set; }

            public string ItemSummary { get; private set; }
        }

        public class RoomItemSelectionViewModel
        {
            public int InventoryItemId { get; set; }

            public string ItemName { get; set; }

            public decimal UnitPrice { get; set; }

            public int Quantity { get; set; }

            public bool IsSelected { get; set; }
        }
    }
}

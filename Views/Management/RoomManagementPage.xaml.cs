using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using staymanager_pj.Models;
using staymanager_pj.Services;

namespace staymanager_pj.Views.Management
{
    public partial class RoomManagementPage : Window
    {
        private readonly RoomService _service = new RoomService();
        private List<RoomStatusItemViewModel> _rooms = new List<RoomStatusItemViewModel>();

        public RoomManagementPage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadRooms();
            btnRefresh.Click += (s, e) => LoadRooms();
        }

        private void LoadRooms()
        {
            try
            {
                _rooms = _service.GetAll()
                    .Select(x => new RoomStatusItemViewModel(x))
                    .ToList();

                icRooms.ItemsSource = _rooms;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Không tải được danh sách phòng.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (sender as Button)?.DataContext as RoomStatusItemViewModel;
            if (viewModel == null)
            {
                return;
            }

            try
            {
                _service.UpdateStatus(viewModel.Room.Id, viewModel.SelectedStatus);
                MessageBox.Show("Cập nhật tình trạng phòng thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRooms();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Không cập nhật được tình trạng phòng.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class RoomStatusItemViewModel
        {
            public RoomStatusItemViewModel(Room room)
            {
                Room = room;
                SelectedStatus = room.Status;
                AvailableStatuses = System.Enum.GetValues(typeof(RoomStatus)).Cast<RoomStatus>().ToList();
            }

            public Room Room { get; private set; }

            public List<RoomStatus> AvailableStatuses { get; private set; }

            public RoomStatus SelectedStatus { get; set; }
        }
    }
}



using staymanager_pj.Models;
using staymanager_pj.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace staymanager_pj.Views.Management
{
    public partial class CheckOutPage : Window
    {
        private readonly ReservationService _reservationService = new ReservationService();
        private readonly InvoiceService _invoiceService = new InvoiceService();
        private List<FrontDeskReservation> _items = new List<FrontDeskReservation>();

        public CheckOutPage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadData();
            btnRefresh.Click += (s, e) => LoadData();
            btnSearch.Click += (s, e) => ApplySearch();
            txtSearch.KeyDown += TxtSearch_KeyDown;
            cmbPaymentMethod.ItemsSource = Enum.GetValues(typeof(PaymentMethod))
                .Cast<PaymentMethod>()
                .Where(x => x != PaymentMethod.None)
                .ToList();
            cmbPaymentMethod.SelectedItem = PaymentMethod.Cash;
        }

        private void LoadData()
        {
            try
            {
                _items = _reservationService.GetFrontDeskReservations();
                dgFrontDesk.ItemsSource = _items;
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tải được danh sách check-in/check-out.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplySearch()
        {
            var keyword = (txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();
            dgFrontDesk.ItemsSource = string.IsNullOrWhiteSpace(keyword)
                ? _items
                : _items.Where(x =>
                    (x.CustomerName ?? string.Empty).ToLowerInvariant().Contains(keyword)
                    || (x.PhoneNumber ?? string.Empty).ToLowerInvariant().Contains(keyword)
                    || (x.RoomNumber ?? string.Empty).ToLowerInvariant().Contains(keyword)
                    || (x.InvoiceCode ?? string.Empty).ToLowerInvariant().Contains(keyword))
                    .ToList();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplySearch();
            }
        }

        private void BtnCheckIn_Click(object sender, RoutedEventArgs e)
        {
            var item = GetRow(sender);
            if (item == null)
            {
                return;
            }

            if (!item.CanCheckIn)
            {
                MessageBox.Show("Chỉ có thể check-in đặt phòng đang chờ hoặc đã xác nhận.");
                return;
            }

            try
            {
                _reservationService.UpdateStatus(item.ReservationId, ReservationStatus.CheckedIn);
                LoadData();
                MessageBox.Show("Check-in thành công. Phòng đã chuyển sang trạng thái đang sử dụng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể check-in.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCheckOut_Click(object sender, RoutedEventArgs e)
        {
            var item = GetRow(sender);
            if (item == null)
            {
                return;
            }

            if (!item.CanCheckOut)
            {
                MessageBox.Show("Chỉ có thể checkout đặt phòng đã check-in.");
                return;
            }

            try
            {
                if (item.InvoiceId > 0 && item.InvoiceStatus == InvoiceStatus.Unpaid)
                {
                    var payResult = MessageBox.Show(
                        "Hóa đơn chưa thanh toán. Xác nhận thanh toán trước khi checkout?",
                        "Thanh toán",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);

                    if (payResult == MessageBoxResult.Cancel)
                    {
                        return;
                    }

                    if (payResult == MessageBoxResult.Yes)
                    {
                        _invoiceService.ConfirmPayment(item.InvoiceId, GetPaymentMethod());
                    }
                }

                _reservationService.UpdateStatus(item.ReservationId, ReservationStatus.CheckedOut);
                LoadData();
                MessageBox.Show("Checkout thành công. Phòng đã được trả về trạng thái trống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể checkout.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPaid_Click(object sender, RoutedEventArgs e)
        {
            var item = GetRow(sender);
            if (item == null)
            {
                return;
            }

            if (item.InvoiceId == 0)
            {
                MessageBox.Show("Đặt phòng này chưa có hóa đơn.");
                return;
            }

            if (item.InvoiceStatus == InvoiceStatus.Paid)
            {
                MessageBox.Show("Hóa đơn đã thanh toán.");
                return;
            }

            try
            {
                _invoiceService.ConfirmPayment(item.InvoiceId, GetPaymentMethod());
                LoadData();
                MessageBox.Show("Đã xác nhận thanh toán hóa đơn.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xác nhận thanh toán.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSummary()
        {
            var waiting = _items.Count(x => x.Status == ReservationStatus.Pending || x.Status == ReservationStatus.Confirmed);
            var checkedIn = _items.Count(x => x.Status == ReservationStatus.CheckedIn);
            txtSummary.Text = string.Format("Đang chờ nhận phòng: {0} | Đang ở: {1} | Tổng dòng hiển thị: {2}", waiting, checkedIn, _items.Count);
        }

        private PaymentMethod GetPaymentMethod()
        {
            return cmbPaymentMethod.SelectedItem is PaymentMethod
                ? (PaymentMethod)cmbPaymentMethod.SelectedItem
                : PaymentMethod.Cash;
        }

        private static FrontDeskReservation GetRow(object sender)
        {
            return (sender as FrameworkElement)?.DataContext as FrontDeskReservation;
        }
    }
}

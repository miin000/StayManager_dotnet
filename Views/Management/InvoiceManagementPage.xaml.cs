using staymanager_pj.Models;
using staymanager_pj.Services;
using System;
using System.Linq;
using System.Windows;

namespace staymanager_pj.Views.Management
{
    public partial class InvoiceManagementPage : Window
    {
        private readonly InvoiceService _service = new InvoiceService();
        private int _selectedId;

        public InvoiceManagementPage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadData();
            dgInvoices.SelectionChanged += (s, e) => FillForm(dgInvoices.SelectedItem as Invoice);
            btnRefresh.Click += (s, e) => LoadData();
            btnNew.Click += (s, e) => ClearForm();
            btnSave.Click += BtnSave_Click;
            btnAddItem.Click += BtnAddItem_Click;
            btnPaid.Click += BtnPaid_Click;
        }

        private void LoadData()
        {
            dgInvoices.ItemsSource = _service.GetAll();
        }

        private void FillForm(Invoice invoice)
        {
            if (invoice == null) return;
            _selectedId = invoice.Id;
            txtCode.Text = invoice.InvoiceCode;
            txtReservationId.Text = invoice.ReservationId.ToString();
            txtCustomerId.Text = invoice.CustomerId.ToString();
            txtRoomId.Text = invoice.RoomId.ToString();
            txtRoomCharge.Text = invoice.RoomCharge.ToString("0.##");
            txtServiceCharge.Text = invoice.ServiceCharge.ToString("0.##");
            txtDiscount.Text = invoice.Discount.ToString("0.##");
            LoadItems(invoice.Id);
        }

        private void LoadItems(int invoiceId)
        {
            lstItems.ItemsSource = _service.GetItems(invoiceId)
                .Select(x => string.Format("{0} x {1} = {2:0.##}", x.ItemName, x.Quantity, x.TotalPrice))
                .ToList();
        }

        private void ClearForm()
        {
            _selectedId = 0;
            txtCode.Clear(); txtReservationId.Clear(); txtCustomerId.Clear(); txtRoomId.Clear();
            txtRoomCharge.Text = "0"; txtServiceCharge.Text = "0"; txtDiscount.Text = "0";
            txtItemName.Clear(); txtItemQty.Text = "1"; txtItemPrice.Text = "0";
            lstItems.ItemsSource = null;
            dgInvoices.SelectedItem = null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var invoice = new Invoice
            {
                Id = _selectedId,
                InvoiceCode = txtCode.Text.Trim(),
                ReservationId = ReadInt(txtReservationId.Text),
                CustomerId = ReadInt(txtCustomerId.Text),
                RoomId = ReadInt(txtRoomId.Text),
                RoomCharge = ReadDecimal(txtRoomCharge.Text),
                ServiceCharge = ReadDecimal(txtServiceCharge.Text),
                Discount = ReadDecimal(txtDiscount.Text),
                Status = InvoiceStatus.Unpaid,
                PaymentMethod = PaymentMethod.None
            };

            if (invoice.ReservationId <= 0 || invoice.CustomerId <= 0 || invoice.RoomId <= 0)
            {
                MessageBox.Show("Nhập mã đặt phòng, mã khách, mã phòng hợp lệ.");
                return;
            }

            _service.Save(invoice);
            LoadData();
            ClearForm();
        }

        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == 0)
            {
                MessageBox.Show("Chọn hoặc lưu hóa đơn trước.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtItemName.Text))
            {
                MessageBox.Show("Nhập tên dịch vụ/vật tư.");
                return;
            }

            _service.AddItem(_selectedId, new InvoiceItem
            {
                ItemName = txtItemName.Text.Trim(),
                Quantity = Math.Max(1, ReadInt(txtItemQty.Text)),
                UnitPrice = ReadDecimal(txtItemPrice.Text)
            });

            LoadData();
            LoadItems(_selectedId);
            txtItemName.Clear(); txtItemQty.Text = "1"; txtItemPrice.Text = "0";
        }

        private void BtnPaid_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == 0) return;
            _service.ConfirmPayment(_selectedId, PaymentMethod.Cash);
            LoadData();
            MessageBox.Show("Đã xác nhận thanh toán.");
        }

        private static int ReadInt(string text)
        {
            int value;
            return int.TryParse(text, out value) ? value : 0;
        }

        private static decimal ReadDecimal(string text)
        {
            decimal value;
            return decimal.TryParse(text, out value) ? value : 0m;
        }
    }
}


using staymanager_pj.Models;
using staymanager_pj.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace staymanager_pj.Views.Management
{
    public partial class InventoryManagementPage : Window
    {
        private readonly InventoryService _service = new InventoryService();
        private List<InventoryItem> _items = new List<InventoryItem>();
        private int _selectedId;

        public InventoryManagementPage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoadData();
            dgInventory.SelectionChanged += (s, e) => FillForm(dgInventory.SelectedItem as InventoryItem);
            btnRefresh.Click += (s, e) => LoadData();
            btnSearch.Click += (s, e) => ApplySearch();
            txtSearch.KeyDown += TxtSearch_KeyDown;
            btnNew.Click += (s, e) => ClearForm();
            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
            btnImport.Click += BtnImport_Click;
        }

        private void LoadData()
        {
            _items = _service.GetAll();
            dgInventory.ItemsSource = _items;
            RestoreSelection();
        }

        private void ApplySearch()
        {
            var key = (txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();
            dgInventory.ItemsSource = string.IsNullOrWhiteSpace(key) ? _items : _service.Search(key);
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplySearch();
            }
        }

        private void FillForm(InventoryItem item)
        {
            if (item == null) return;
            _selectedId = item.Id;
            txtCode.Text = item.ItemCode;
            txtName.Text = item.ItemName;
            txtCategory.Text = item.Category;
            txtQuantity.Text = item.Quantity.ToString();
            txtMinimum.Text = item.MinimumQuantity.ToString();
            txtUnit.Text = item.Unit;
            txtImportPrice.Text = item.ImportPrice.ToString("0.##");
            txtSellingPrice.Text = item.SellingPrice.ToString("0.##");
            txtNote.Text = item.Note;
            txtImportUnitPrice.Text = item.ImportPrice.ToString("0.##");
            txtImportQty.Clear();
            txtSupplier.Clear();
        }

        private void ClearForm()
        {
            _selectedId = 0;
            txtCode.Clear(); txtName.Clear(); txtCategory.Clear(); txtQuantity.Text = "0"; txtMinimum.Text = "0";
            txtUnit.Clear(); txtImportPrice.Text = "0"; txtSellingPrice.Text = "0"; txtNote.Clear();
            txtImportQty.Clear(); txtImportUnitPrice.Clear(); txtSupplier.Clear();
            dgInventory.SelectedItem = null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!TryBuildItem(out var item))
            {
                return;
            }

            _service.Save(item);
            LoadData();
            ClearForm();
            MessageBox.Show("Lưu vật tư thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == 0) return;
            if (MessageBox.Show("Xóa vật tư đang chọn?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try
            {
                _service.Delete(_selectedId);
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không xóa được vật tư. Có thể vật tư đã phát sinh dữ liệu nhập kho.\n" + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == 0)
            {
                MessageBox.Show("Chọn vật tư trước khi nhập kho.");
                return;
            }

            var qty = ReadInt(txtImportQty.Text);
            if (qty <= 0)
            {
                MessageBox.Show("Số lượng nhập phải lớn hơn 0.");
                return;
            }

            var unitPrice = ReadDecimal(txtImportUnitPrice.Text);
            if (unitPrice < 0)
            {
                MessageBox.Show("Đơn giá nhập không hợp lệ.");
                return;
            }

            _service.ImportStock(_selectedId, qty, unitPrice, txtSupplier.Text.Trim(), txtNote.Text.Trim());
            LoadData();
            txtImportQty.Clear(); txtImportUnitPrice.Clear(); txtSupplier.Clear();
            MessageBox.Show("Nhập kho thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RestoreSelection()
        {
            if (_selectedId == 0)
            {
                return;
            }

            var selected = _items.FirstOrDefault(x => x.Id == _selectedId);
            if (selected != null)
            {
                dgInventory.SelectedItem = selected;
            }
        }

        private bool TryBuildItem(out InventoryItem item)
        {
            item = null;

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Tên vật tư là bắt buộc.");
                return false;
            }

            var quantity = ReadInt(txtQuantity.Text);
            var minimum = ReadInt(txtMinimum.Text);
            var importPrice = ReadDecimal(txtImportPrice.Text);
            var sellingPrice = ReadDecimal(txtSellingPrice.Text);

            if (quantity < 0 || minimum < 0)
            {
                MessageBox.Show("Số lượng và mức tối thiểu không được âm.");
                return false;
            }

            if (importPrice < 0 || sellingPrice < 0)
            {
                MessageBox.Show("Giá nhập/giá bán không được âm.");
                return false;
            }

            item = new InventoryItem
            {
                Id = _selectedId,
                ItemCode = txtCode.Text.Trim(),
                ItemName = txtName.Text.Trim(),
                Category = txtCategory.Text.Trim(),
                Quantity = quantity,
                MinimumQuantity = minimum,
                Unit = txtUnit.Text.Trim(),
                ImportPrice = importPrice,
                SellingPrice = sellingPrice,
                Note = txtNote.Text.Trim()
            };

            return true;
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



using staymanager_pj.Models;
using staymanager_pj.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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
            btnNew.Click += (s, e) => ClearForm();
            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
            btnImport.Click += BtnImport_Click;
        }

        private void LoadData()
        {
            _items = _service.GetAll();
            dgInventory.ItemsSource = _items;
        }

        private void ApplySearch()
        {
            var key = (txtSearch.Text ?? string.Empty).Trim().ToLowerInvariant();
            dgInventory.ItemsSource = string.IsNullOrWhiteSpace(key)
                ? _items
                : _items.Where(x => (x.ItemCode ?? string.Empty).ToLowerInvariant().Contains(key)
                                 || (x.ItemName ?? string.Empty).ToLowerInvariant().Contains(key)
                                 || (x.Category ?? string.Empty).ToLowerInvariant().Contains(key)).ToList();
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
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Nhập tên vật tư.");
                return;
            }

            var item = new InventoryItem
            {
                Id = _selectedId,
                ItemCode = txtCode.Text.Trim(),
                ItemName = txtName.Text.Trim(),
                Category = txtCategory.Text.Trim(),
                Quantity = ReadInt(txtQuantity.Text),
                MinimumQuantity = ReadInt(txtMinimum.Text),
                Unit = txtUnit.Text.Trim(),
                ImportPrice = ReadDecimal(txtImportPrice.Text),
                SellingPrice = ReadDecimal(txtSellingPrice.Text),
                Note = txtNote.Text.Trim()
            };

            _service.Save(item);
            LoadData();
            ClearForm();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == 0) return;
            if (MessageBox.Show("Xóa vật tư đang chọn?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            _service.Delete(_selectedId);
            LoadData();
            ClearForm();
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

            _service.ImportStock(_selectedId, qty, ReadDecimal(txtImportUnitPrice.Text), txtSupplier.Text.Trim(), txtNote.Text.Trim());
            LoadData();
            txtImportQty.Clear(); txtSupplier.Clear();
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


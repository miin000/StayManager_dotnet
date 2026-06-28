using staymanager_pj.Services;
using System;
using System.Windows;

namespace staymanager_pj.Views.Management
{
    public partial class CustomerManagementPage : Window
    {
        private readonly CustomerService customerService = new CustomerService();

        public CustomerManagementPage()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            try
            {
                dgCustomers.ItemsSource = customerService.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thêm khách hàng đang phát triển.");
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadCustomers();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng xuất dữ liệu sẽ được bổ sung.");
        }
    }
}
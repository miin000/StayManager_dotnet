using staymanager_pj.Services;
using System;
using System.Windows;

namespace staymanager_pj.Views.Management
{
    public partial class EmployeeManagementPage : Window
    {
        public EmployeeManagementPage()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                EmployeeService service = new EmployeeService();
                dgEmployees.ItemsSource = service.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng thêm nhân viên đang phát triển.");
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng xuất dữ liệu sẽ được bổ sung.");
        }
    }
}
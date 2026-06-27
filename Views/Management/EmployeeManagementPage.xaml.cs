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
    }
}
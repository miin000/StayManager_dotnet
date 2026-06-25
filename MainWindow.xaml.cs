using staymanager_pj.Views.Management;
using staymanager_pj.Views.Shared;
using System.Windows;

namespace staymanager_pj
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            btnInventory.Click += (s, e) => Open(new InventoryManagementPage());
            btnInvoices.Click += (s, e) => Open(new InvoiceManagementPage());
            btnRooms.Click += (s, e) => Open(new RoomManagementPage());
            btnCustomers.Click += (s, e) => Open(new CustomerManagementPage());
            btnEmployees.Click += (s, e) => Open(new EmployeeManagementPage());
            btnLogin.Click += (s, e) => Open(new LoginPage());
        }

        private void Open(Window window)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Show();
        }
    }
}


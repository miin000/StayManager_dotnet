using staymanager_pj.Views.Management;
using System.Windows;
using System.Windows.Controls;

namespace staymanager_pj.Views.Shared
{
    public partial class Sidebar : UserControl
    {
        public Sidebar()
        {
            InitializeComponent();
            Loaded += Sidebar_Loaded;
        }

        private void Sidebar_Loaded(object sender, RoutedEventArgs e)
        {
            btnDashboard.Click -= BtnDashboard_Click;
            btnRoomStatus.Click -= BtnRoomStatus_Click;
            btnInventory.Click -= BtnInventory_Click;
            btnBillingInvoices.Click -= BtnBillingInvoices_Click;
            btnAnalytics.Click -= BtnAnalytics_Click;
            btnLogout.Click -= BtnLogout_Click;

            btnDashboard.Click += BtnDashboard_Click;
            btnRoomStatus.Click += BtnRoomStatus_Click;
            btnInventory.Click += BtnInventory_Click;
            btnBillingInvoices.Click += BtnBillingInvoices_Click;
            btnAnalytics.Click += BtnAnalytics_Click;
            btnLogout.Click += BtnLogout_Click;
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            Open(new MainWindow());
        }

        private void BtnRoomStatus_Click(object sender, RoutedEventArgs e)
        {
            Open(new RoomManagementPage());
        }

        private void BtnInventory_Click(object sender, RoutedEventArgs e)
        {
            Open(new InventoryManagementPage());
        }

        private void BtnBillingInvoices_Click(object sender, RoutedEventArgs e)
        {
            Open(new InvoiceManagementPage());
        }

        private void BtnAnalytics_Click(object sender, RoutedEventArgs e)
        {
            Open(new AnalyticsPage());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Open(new LoginPage());
        }

        private void Open(Window window)
        {
            var currentWindow = Window.GetWindow(this);
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Show();
            if (currentWindow != null && currentWindow.GetType() != window.GetType())
            {
                currentWindow.Close();
            }
        }
    }
}



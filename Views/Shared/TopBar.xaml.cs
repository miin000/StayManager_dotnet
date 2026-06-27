using staymanager_pj.Services;
using staymanager_pj.Views.Customer;
using staymanager_pj.Views.Management;
using System;
using System.Windows;
using System.Windows.Controls;

namespace staymanager_pj.Views.Shared
{
    public partial class TopBar : UserControl
    {
        public TopBar()
        {
            InitializeComponent();

            Loaded += (s, e) => RefreshState();
            btnHome.Click += (s, e) => NavigateHome();
            btnRoomList.Click += (s, e) => NavigateTo(() => new RoomListPage());
            btnLogin.Click += (s, e) => NavigateTo(() => new LoginPage());
            btnRegister.Click += (s, e) => NavigateTo(() => new RegisterPage());
            btnLogout.Click += BtnLogout_Click;
        }

        private void RefreshState()
        {
            var isLoggedIn = AuthService.IsLoggedIn;
            pnlGuest.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
            pnlAuthenticated.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;

            if (!isLoggedIn)
            {
                txtWelcome.Text = string.Empty;
                return;
            }

            if (AuthService.IsCustomerLoggedIn)
            {
                txtWelcome.Text = $"Xin chào, {AuthService.CurrentCustomer.FullName}";
                return;
            }

            txtWelcome.Text = $"Xin chào, {AuthService.CurrentEmployee.FullName}";
        }

        private void NavigateHome()
        {
            if (AuthService.IsCustomerLoggedIn)
            {
                NavigateTo(() => new HomePage());
                return;
            }

            if (AuthService.IsEmployeeLoggedIn)
            {
                NavigateTo(() => new MainWindow());
                return;
            }

            NavigateTo(() => new Dashboard());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AuthService.Logout();
            NavigateTo(() => new Dashboard());
        }

        private void NavigateTo(Func<Window> factory)
        {
            var currentWindow = Window.GetWindow(this);
            var nextWindow = factory();

            if (currentWindow != null && currentWindow.GetType() == nextWindow.GetType())
            {
                return;
            }

            nextWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            nextWindow.Show();
            currentWindow?.Close();
        }
    }
}

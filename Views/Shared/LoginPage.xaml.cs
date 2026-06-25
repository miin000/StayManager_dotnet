using staymanager_pj.Services;
using staymanager_pj.Views.Customer;
using staymanager_pj.Views.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace staymanager_pj.Views.Shared
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Window
    {
        private readonly EmployeeService _employeeService = new EmployeeService();
        private readonly CustomerService _customerService = new CustomerService();

        public LoginPage()
        {
            InitializeComponent();
            btnLogin.Click += BtnLogin_Click;
            btnGoToRegister.Click += (s, e) => { new RegisterPage().Show(); Close(); };
            btnForgotPassword.Click += (s, e) => MessageBox.Show("Vui lòng liên hệ quản trị viên để cấp lại mật khẩu.");
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Password;
            var employee = _employeeService.Login(username, password);
            if (employee != null)
            {
                new Dashboard().Show();
                Close();
                return;
            }

            var customer = _customerService.Login(username, password);
            if (customer != null)
            {
                new HomePage().Show();
                Close();
                return;
            }

            MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng.", "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}


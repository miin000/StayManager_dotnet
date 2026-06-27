using staymanager_pj.Models;
using staymanager_pj.Services;
using staymanager_pj.Views.Customer;
using System.Windows;

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
            btnForgotPassword.Click += BtnForgotPassword_Click;
            rbCustomer.Checked += (s, e) => UpdateRoleHint();
            rbAdmin.Checked += (s, e) => UpdateRoleHint();
            rbStaff.Checked += (s, e) => UpdateRoleHint();

            UpdateRoleHint();
        }

        private void UpdateRoleHint()
        {
            if (rbAdmin.IsChecked == true)
            {
                txtRoleHint.Text = "Admin sẽ vào khu vực quản trị sau khi đăng nhập.";
                return;
            }

            if (rbStaff.IsChecked == true)
            {
                txtRoleHint.Text = "Staff sẽ vào khu vực quản lý vận hành sau khi đăng nhập.";
                return;
            }

            txtRoleHint.Text = "Khách hàng sẽ vào trang chủ đặt phòng sau khi đăng nhập.";
        }

        private void BtnForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            if (rbCustomer.IsChecked == true)
            {
                MessageBox.Show("Vui lòng liên hệ lễ tân hoặc quản trị viên để được hỗ trợ tài khoản khách hàng.");
                return;
            }

            MessageBox.Show("Tài khoản admin/staff vui lòng liên hệ quản trị viên để cấp lại mật khẩu.");
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (rbCustomer.IsChecked == true)
            {
                LoginCustomer(username, password);
                return;
            }

            var expectedRole = rbAdmin.IsChecked == true ? UserRole.Admin : UserRole.Staff;
            LoginEmployee(username, password, expectedRole);
        }

        private void LoginCustomer(string username, string password)
        {
            var customer = _customerService.Login(username, password);
            if (customer == null)
            {
                MessageBox.Show("Tài khoản khách hàng không đúng hoặc chưa tồn tại.", "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AuthService.Login(customer);
            new HomePage().Show();
            Close();
        }

        private void LoginEmployee(string username, string password, UserRole expectedRole)
        {
            var employee = _employeeService.Login(username, password);
            if (employee == null)
            {
                MessageBox.Show("Tài khoản nhân viên không đúng hoặc đã bị khóa.", "Đăng nhập thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (employee.Role != expectedRole)
            {
                MessageBox.Show($"Tài khoản này không thuộc nhóm {expectedRole}.", "Sai loại tài khoản", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AuthService.Login(employee);
            new MainWindow().Show();
            Close();
        }
    }
}

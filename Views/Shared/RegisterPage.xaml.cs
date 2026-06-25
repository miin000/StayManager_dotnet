using staymanager_pj.Services;
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
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Window
    {
        private readonly CustomerService _service = new CustomerService();

        public RegisterPage()
        {
            InitializeComponent();
            btnRegister.Click += BtnRegister_Click;
            btnGoToLogin.Click += (s, e) => { new LoginPage().Show(); Close(); };
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtPhoneNumber.Text) || string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ họ tên, số điện thoại, tên đăng nhập và mật khẩu.");
                return;
            }

            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("Mật khẩu nhập lại không khớp.");
                return;
            }

            if (chkAgree.IsChecked != true)
            {
                MessageBox.Show("Bạn cần đồng ý điều khoản sử dụng.");
                return;
            }

            _service.Save(new staymanager_pj.Models.Customer
            {
                FullName = txtFullName.Text.Trim(),
                PhoneNumber = txtPhoneNumber.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Username = txtUsername.Text.Trim(),
                PasswordHash = txtPassword.Password,
                IdentityNumber = txtIdentityNumber.Text.Trim(),
                Address = txtAddress.Text.Trim()
            });

            MessageBox.Show("Đăng ký thành công. Vui lòng đăng nhập.");
            new LoginPage().Show();
            Close();
        }
    }
}


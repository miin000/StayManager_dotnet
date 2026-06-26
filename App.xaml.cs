using MySql.Data.MySqlClient;
using staymanager_pj.Data;
using System;
using System.Configuration;
using System.Text;
using System.Windows;

namespace staymanager_pj
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                EnsureDatabaseExists();

                using (var db = new AppDbContext())
                {
                    db.Database.Initialize(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Kết nối MySQL thất bại hoặc chưa tạo được bảng.\n\n" + BuildErrorDetails(ex) +
                    "\n\nKiểm tra lại server, port, user, password trong App.config.",
                    "Lỗi khởi tạo cơ sở dữ liệu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(-1);
            }
        }

        private static void EnsureDatabaseExists()
        {
            var connection = ConfigurationManager.ConnectionStrings["StayManagerDbContext"];
            if (connection == null || string.IsNullOrWhiteSpace(connection.ConnectionString))
            {
                throw new InvalidOperationException("Thiếu connection string StayManagerDbContext trong App.config.");
            }

            var builder = new MySqlConnectionStringBuilder(connection.ConnectionString);
            var databaseName = builder.Database;
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("Connection string chưa khai báo tên database.");
            }

            builder.Database = string.Empty;

            using (var mysql = new MySqlConnection(builder.ConnectionString))
            {
                mysql.Open();
                using (var command = mysql.CreateCommand())
                {
                    command.CommandText = $"CREATE DATABASE IF NOT EXISTS `{databaseName.Replace("`", "``")}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
                    command.ExecuteNonQuery();
                }
            }
        }

        private static string BuildErrorDetails(Exception ex)
        {
            var builder = new StringBuilder();
            var current = ex;

            while (current != null)
            {
                if (!string.IsNullOrWhiteSpace(current.Message))
                {
                    if (builder.Length > 0)
                    {
                        builder.AppendLine();
                        builder.AppendLine("---");
                    }

                    builder.Append(current.Message);
                }

                current = current.InnerException;
            }

            return builder.Length == 0 ? "Không lấy được chi tiết lỗi." : builder.ToString();
        }
    }
}

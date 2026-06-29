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
        public static bool IsDatabaseAvailable { get; private set; }

        public static string DatabaseError { get; private set; }

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

                IsDatabaseAvailable = true;
                DatabaseError = string.Empty;
            }
            catch (Exception ex)
            {
                IsDatabaseAvailable = false;
                DatabaseError = BuildErrorDetails(ex);

                MessageBox.Show(
                    "Không kết nối được MySQL. Ứng dụng sẽ chạy chế độ dữ liệu mẫu tạm thời cho các màn đã hỗ trợ.\n\n" + DatabaseError +
                    "\n\nMuốn lưu dữ liệu thật: bật MySQL server, kiểm tra port/user/password trong App.config rồi chạy lại.",
                    "Cảnh báo cơ sở dữ liệu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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

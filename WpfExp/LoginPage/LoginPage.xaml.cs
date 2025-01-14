using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
using WpfExp.LogRecords;
using WpfExp.ModeratorPage;

namespace WpfExp.LoginPage
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Window
    {

        private string connStr;

            public LoginPage()
        {
            InitializeComponent();
            connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUserName.Text;
            string password = pssbowPassword.Password;

            using(SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string loginQuery = "SELECT u.UserName, r.RoleName, u.Id FROM USERS u " +
                    "JOIN ROLES r ON u.rolesId = r.Id " +
                    "WHERE u.UserName = @UserName AND u.Password = @Password ";

                using (SqlCommand cmd = new SqlCommand(loginQuery,conn))
                {
                    cmd.Parameters.AddWithValue("@UserName", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    

                    using(SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if(reader.Read())
                        {
                            string user = reader["UserName"].ToString();
                            int userId = Convert.ToInt32(reader["Id"]);
                            string role = reader["RoleName"].ToString();

                            FindUserId.CurrentUserId = userId;
                            FindUserId.CurrentUserRole = role;

                            try
                            {
                                LogUserProcess.AddLog(userId ,$"Giriş işlemi yapıldı. Kullanıcı: {user}, Rolü: {role}");
                            }
                            catch (Exception ex)
                            {

                                MessageBox.Show($"Loglama sırasında bir hata gerçekleşti. {ex.Message}", "Hata",MessageBoxButton.OK, MessageBoxImage.Error);
                            }

                            MessageBox.Show($"Giriş başarılı! Hoşgeldin {user}", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);

                            

                            if (role == "admin")
                            {
                                AdminLoginPanel loginPanel = new AdminLoginPanel();
                                loginPanel.Show();
                                //MainWindow mainWindow = new MainWindow();
                                //mainWindow.Show();
                                this.Hide();

                                
                            }

                            else if(role == "moderator")
                            {
                                WpfExp.ModeratorPage.ModeratorPage modpage = new ModeratorPage.ModeratorPage();
                                modpage.Show();
                                this.Hide();
                            }

                            else if (role == "visitor")
                            {
                                WpfExp.VisitorPage.VisitorPage visitpage = new VisitorPage.VisitorPage();
                                visitpage.Show();
                                this.Hide();
                            }
                        }
                        else
                        {
                            ErrorTextBlock.Text = "Bir şeyleri yanlış girdin.";
                            ErrorTextBlock.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }
    }
}

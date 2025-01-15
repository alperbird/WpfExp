using System;
using System.Collections.Generic;
using System.Data;
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

namespace WpfExp.LogRecords
{
    /// <summary>
    /// Interaction logic for LogRecords.xaml
    /// </summary>
    public partial class LogRecords : Window
    {
        public LogRecords()
        {
            InitializeComponent();
            LoadLogs();
        }

        private void LoadLogs()
        {
            var connection = new Connection(); // Connection sınıfından instance alınıyor.
            using (SqlConnection conn = connection.GetConnection())
            {
                try
                {
                    string logQuery = "SELECT l.Id, l.Action, l.Timestamp, u.UserName FROM LOGS l " +
                                      "JOIN USERS u ON l.UserId = u.Id ";

                    SqlDataAdapter adapt = new SqlDataAdapter(logQuery, conn);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);
                    dgLogs.ItemsSource = dt.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Logları yüklerken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


    }
}

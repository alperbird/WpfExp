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
            using (SqlConnection conn = new SqlConnection(@"Server=192.168.5.68,1433;Database=OZGENC;User Id=sa; Password=ozgencmachine16;Trusted_Connection=True;"))
            {
                conn.Open();
                string logQuery = "SELECT l.Id, l.Action, l.Timestamp, u.UserName FROM LOGS l " +
                    "JOIN USERS u ON l.UserId = u.Id ";

                SqlDataAdapter adapt = new SqlDataAdapter(logQuery,conn);
                DataTable dt = new DataTable();
                adapt.Fill(dt);
                dgLogs.ItemsSource = dt.DefaultView;
            }
        }

    }
}

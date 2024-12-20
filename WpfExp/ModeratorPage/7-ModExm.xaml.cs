using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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

namespace WpfExp.ModeratorPage
{
    /// <summary>
    /// Interaction logic for _7_ModExm.xaml
    /// </summary>
    public partial class _7_ModExm : Window
    {
        public string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        public _7_ModExm()
        {
            InitializeComponent();
            TableRead();
        }

        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string exmQuery = "SELECT Id, name AS 'Ad', description AS 'Açıklama' FROM PROFILE_EXAMPLE";
                using (SqlCommand cmd = new SqlCommand(exmQuery, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgExamples.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO PROFILE_EXAMPLE (name, description) " +
                    "VALUES(@name, @description)";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);

                cmd.Parameters.AddWithValue("@name", txtExmName.Text);
                cmd.Parameters.AddWithValue("@description", txtExmDesc.Text);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                LogUserProcess.AddLog(currentUserId, $"Örnek eklendi: {txtExmName.Text}");
                Clean();
                TableRead();
            }
        }

        private void Clean()
        {
            txtExmName.Text = "";
            txtExmDesc.Text = "";
        }
    }
}

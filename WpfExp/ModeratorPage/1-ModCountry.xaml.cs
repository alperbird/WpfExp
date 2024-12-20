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
    /// Interaction logic for _1_ModCountry.xaml
    /// </summary>
    public partial class _1_ModCountry : Window
    {
        public string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        public _1_ModCountry()
        {
            InitializeComponent();
            TableRead();
        }
        public void TableRead() // tablo okuma  
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT Id, name AS 'Ülke Adı', description AS 'Açıklama' FROM COUNTRIES";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgCountries.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;


            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO COUNTRIES (name, description)" +
                    "VALUES(@name, @description)";
                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@name", txtCountryName.Text);
                cmd.Parameters.AddWithValue("@description", txtCountryDesc.Text);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                LogUserProcess.AddLog(currentUserId, $"Ülke Kaydı Eklendi: {txtCountryName.Text}");

                Clean();
                TableRead();
            }

        }


        private void Clean()
        {
            txtCountryName.Text = "";
            txtCountryDesc.Text = "";
        }
        
    }
}

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
    /// Interaction logic for _2_ModCity.xaml
    /// </summary>
    public partial class _2_ModCity : Window
    {
        public string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        public _2_ModCity()
        {
            InitializeComponent();
            TableRead();
            cmbxFull();
        }
        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string readQueary = "SELECT ci.Id, ci.name AS 'Şehir Adı', ci.description AS 'Açıklama', co.name AS 'Ülke' FROM CITIES ci " +
                    "INNER JOIN COUNTRIES co ON " +
                    "ci.countryId = co.Id";
                using (SqlCommand cmd = new SqlCommand(readQueary, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgCities.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void cmbxFull() 
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string queryCountry = "SELECT Id,name FROM COUNTRIES";
                SqlCommand cocmd = new SqlCommand(queryCountry, conn);
                SqlDataAdapter daCountry = new SqlDataAdapter(cocmd);
                DataTable dtco = new DataTable();
                daCountry.Fill(dtco);

                cmbCountry.ItemsSource = dtco.DefaultView;
                cmbCountry.DisplayMemberPath = "name";
                cmbCountry.SelectedValuePath = "Id";

                cmbCountry.UpdateLayout();
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO CITIES (name, description, countryId) " +
                    "VALUES(@name, @description, @countryId)";
                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@name", txtCityName.Text);
                cmd.Parameters.AddWithValue("@description", txtCityDesc.Text);
                cmd.Parameters.AddWithValue("@countryId", cmbCountry.SelectedValue);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                LogUserProcess.AddLog(currentUserId, $"Şehir Kaydı Eklendi: {txtCityName.Text}");

                Clean();
                TableRead();
            }
        }
        private void Clean()
        {
            txtCityName.Text = "";
            txtCityDesc.Text = "";
            cmbCountry.SelectedIndex = -1;
        }
    }
}

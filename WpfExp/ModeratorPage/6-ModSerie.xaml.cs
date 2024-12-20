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
    /// Interaction logic for _6_ModSerie.xaml
    /// </summary>
    public partial class _6_ModSerie : Window
    {
        public string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        public _6_ModSerie()
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
                string serieQuery = "SELECT ps.Id, ps.name AS 'Seri Adı', ps.description AS 'Açıklama', pb.name AS 'Marka' FROM PROFILE_SERIES ps " +
                    "JOIN PROFILE_BRANDS pb ON " +
                    "ps.brandId = pb.Id";
                using (SqlCommand cmd = new SqlCommand(serieQuery, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgSeries.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void cmbxFull()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT Id, name FROM PROFILE_BRANDS";

                SqlCommand pbcmd = new SqlCommand(query, conn);
                SqlDataAdapter adpb = new SqlDataAdapter(pbcmd);
                DataTable dt = new DataTable();
                adpb.Fill(dt);

                cmbBrand.ItemsSource = dt.DefaultView;
                cmbBrand.DisplayMemberPath = "name";
                cmbBrand.SelectedValuePath = "Id";
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO PROFILE_SERIES (name, description, brandId) " +
                    "VALUES(@name, @description, @brandId)";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);

                cmd.Parameters.AddWithValue("@name", txtSeriesName.Text);
                cmd.Parameters.AddWithValue("@description", txtSeriesDesc.Text);
                cmd.Parameters.AddWithValue("@brandId", cmbBrand.SelectedValue);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                LogUserProcess.AddLog(currentUserId, $"Marka Serisi Eklendi: {txtSeriesName.Text}");

                Clean();
                TableRead();
            }
        }

        private void Clean()
        {
            txtSeriesName.Text = "";
            txtSeriesDesc.Text = "";
            cmbBrand.SelectedIndex = -1;
        }
    }
}

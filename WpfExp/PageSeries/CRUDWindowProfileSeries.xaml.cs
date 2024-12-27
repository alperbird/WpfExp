using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
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

namespace WpfExp.PageSeries
{
    /// <summary>
    /// Interaction logic for CRUDWindowProfileSeries.xaml
    /// </summary>
    public partial class CRUDWindowProfileSeries : Window
    {
        private string connStr = @"Server=192.168.5.68,1433;Database=OZGENC;User Id=sa; Password=ozgencmachine16;Trusted_Connection=True;";
        public CRUDWindowProfileSeries()
        {
            InitializeComponent();
            TableRead();
            cmbxFull();
        }

        public void TableRead()
        {
            using(SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string serieQuery = "SELECT ps.Id, ps.name AS 'Seri Adı', ps.description AS 'Açıklama', pb.name AS 'Marka' FROM PROFILE_SERIES ps " +
                    "JOIN PROFILE_BRANDS pb ON " +
                    "ps.brandId = pb.Id";
                using(SqlCommand cmd = new SqlCommand(serieQuery, conn))
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
            using(SqlConnection conn = new SqlConnection(connStr))
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

        private void dgSerie(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgSeries.SelectedItem is DataRowView row)
                {
                    txtSeriesName.Text = row["Seri Adı"].ToString();
                    txtSeriesDesc.Text = row["Açıklama"].ToString();

                    var selectedBrand = cmbBrand.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == row["Marka"].ToString());
                    cmbBrand.SelectedItem = selectedBrand;

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("HATA!\n" + ex.Message);
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;
            using(SqlConnection conn = new SqlConnection(connStr))
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

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;
            if(dgSeries.SelectedItem is DataRowView row)
            {
                string oldName = "";
                string oldDesc = "";
                int oldbrandId = 0;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT Id,name,description,brandId FROM PROFILE_SERIES WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", row["Id"]);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldName = reader["name"].ToString();
                            oldDesc = reader["description"].ToString();
                            oldbrandId = Convert.ToInt32(reader["brandId"]);
                        }
                    }
                }


                string newName = txtSeriesName.Text;
                string newDesc = txtSeriesDesc.Text;
                int newBrandId = Convert.ToInt32(cmbBrand.SelectedValue);
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE PROFILE_SERIES SET " +
                        "name = @name," +
                        "description = @description," +
                        "brandId = @brandId " +
                        "WHERE Id = @Id";

                    SqlCommand cmd = new SqlCommand(updateQuery, conn);

                    cmd.Parameters.AddWithValue("@name", txtSeriesName.Text);
                    cmd.Parameters.AddWithValue("@description", txtSeriesDesc.Text);
                    cmd.Parameters.AddWithValue("@brandId", Convert.ToInt32(cmbBrand.SelectedValue));
                    cmd.Parameters.AddWithValue("@Id", row["Id"]);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.");
                    Clean();
                    TableRead();
                }
                string action = $"Seçilen Marka Serisi Güncellendi: " +
                                $"Makine Adı: {oldName} -> {newName},\n " +
                                $"Açıklama: {oldDesc} -> {newDesc},\n " +
                                $"Makine Tipi: {oldbrandId} -> {newBrandId}";
                LogUserProcess.AddLog(currentUserId, action);
            }
        }

        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Silmek istediğine emin misin?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
                if (dgSeries.SelectedItem is DataRowView row)
                {
                    int currentUserId = LoginPage.FindUserId.CurrentUserId;
                    string deletedName = "";
                    string deletedDesc = "";
                    int deletedBrandId = 0;
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        string query = "SELECT Id,name,description,brandId FROM PROFILE_SERIES WHERE Id = @Id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", row["Id"]);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                deletedName = reader["name"].ToString();
                                deletedDesc = reader["description"].ToString();
                                deletedBrandId = Convert.ToInt32(reader["brandId"]);
                            }
                        }
                    }

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM PROFILE_SERIES " +
                            "WHERE Id = @Id";

                        SqlCommand cmd = new SqlCommand(deleteQuery, conn);

                        cmd.Parameters.AddWithValue("@Id", row["Id"]);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Seçilen profil serisi silindi.");
                        string logMessage = $"Kayıt Silindi: " +
                                        $"Marka Serisi Adı: {deletedName},\n " +
                                        $"Açıklama: {deletedDesc},\n " +
                                        $"Ülke: {deletedBrandId}";
                        LogUserProcess.AddLog(currentUserId, logMessage);

                        Clean();
                        TableRead();

                    }
                }
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

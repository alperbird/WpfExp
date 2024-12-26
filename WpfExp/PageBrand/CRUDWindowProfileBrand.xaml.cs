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

namespace WpfExp.PageBrand
{
    /// <summary>
    /// Interaction logic for CRUDWindowProfileBrand.xaml
    /// </summary>
    public partial class CRUDWindowProfileBrand : Window
    {
        private string connStr = @"Server=DESKTOP-F5QQ2O8;Database=OZGENC;Trusted_Connection=True;";
        public CRUDWindowProfileBrand()
        {
            InitializeComponent();
            TableRead();
        }

        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string brandQuery = "SELECT Id, name AS 'Marka Adı', description AS 'Açıklama' FROM PROFILE_BRANDS";

                using (SqlCommand cmd = new SqlCommand(brandQuery, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgBrands.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void dgBrand(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgBrands.SelectedItem is DataRowView row)
                {
                    txtBrandName.Text = row["Marka Adı"].ToString();
                    txtBrandDesc.Text = row["Açıklama"].ToString();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("HATA\n" + ex.Message);
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO PROFILE_BRANDS(name, description) " +
                    "VALUES(@name, @description)";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);

                cmd.Parameters.AddWithValue("@name", txtBrandName.Text);
                cmd.Parameters.AddWithValue("@description", txtBrandDesc);


                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                LogUserProcess.AddLog(currentUserId, $"Marka Kaydı Eklendi: {txtBrandName.Text}");
                Clean();
                TableRead();
            }
        }

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;
            if (dgBrands.SelectedItem is DataRowView row)
            {
                string oldName = "";
                string oldDesc = "";
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT Id, name, description FROM PROFILE_BRANDS WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@Id", row["Id"]);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldName = reader["name"].ToString();
                            oldDesc = reader["description"].ToString();
                        }
                    }
                }


                string newName = txtBrandName.Text;
                string newDesc = txtBrandDesc.Text;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE PROFİLE_BRANDS SET " +
                        "name = @name," +
                        "description = @description," +
                        "WHERE Id = @Id";

                    SqlCommand cmd = new SqlCommand(updateQuery);

                    cmd.Parameters.AddWithValue("@name", txtBrandName.Text);
                    cmd.Parameters.AddWithValue("@description", txtBrandDesc.Text);
                    cmd.Parameters.AddWithValue("@Id", row["Id"]);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.");
                    Clean();
                    TableRead();
                }
                string logMessage = $"Seçili Marka Güncellendi: " +
                                    $"Marka Adı: {oldName} -> {newName},\n " +
                                    $"Açıklama: {oldDesc} -> {newDesc}";
                LogUserProcess.AddLog(currentUserId, logMessage);
            }
        }

        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Silmek istediğine emin misin?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (dgBrands.SelectedItem is DataRowView row)
                {
                    int currentUserId = LoginPage.FindUserId.CurrentUserId;
                    string deletedName = "";
                    string deletedDesc = "";
                    using (SqlConnection conn = new SqlConnection(connStr)) 
                    {
                        conn.Open();
                        string query = "SELECT Id, name, description FROM PROFILE_BRANDS WHERE Id = @Id";
                        SqlCommand cmd = new SqlCommand(query, conn);

                        cmd.Parameters.AddWithValue("@Id", row["Id"]);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                deletedName = reader["name"].ToString();
                                deletedDesc = reader["description"].ToString();
                            }
                        }
                    }

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM PROFILE_BRANDS " +
                            "WHERE Id = @Id";

                        SqlCommand cmd = new SqlCommand(deleteQuery, conn);

                        cmd.Parameters.AddWithValue("@Id", row["Id"]);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Seçilen marka silindi.");
                        string logMessage = $"Kayıt Silindi. " +
                                            $"Ad: {deletedName},\n " +
                                            $"Açıklama: {deletedDesc}";
                        LogUserProcess.AddLog(currentUserId, logMessage);

                        Clean();
                        TableRead();
                    }
                }

            }
        }

        private void Clean()
        {
            txtBrandName.Text = "";
            txtBrandDesc.Text = "";

        }
    }
}

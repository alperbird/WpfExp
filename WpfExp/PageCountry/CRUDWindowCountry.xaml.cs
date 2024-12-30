using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfExp.LogRecords;

namespace WpfExp.PageCountry
{
    /// <summary>
    /// Interaction logic for CRUDWindowCountry.xaml
    /// </summary>
    public partial class CRUDWindowCountry : Window
    {
        private string connStr;
        public CRUDWindowCountry()
        {
            InitializeComponent();
            connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
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

        private void dgCountry(object sender, SelectionChangedEventArgs e) //tabloya tıklıyorsun verileri dolduruyor falan)
        {
            try
            {
                if (dgCountries.SelectedItem is DataRowView row)
                {
                    txtCountryName.Text = row["Ülke Adı"].ToString();
                    txtCountryDesc.Text = row["Açıklama"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata patlatmayacak ama deneyelim yine de\n" + ex.Message);
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

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;

            if (dgCountries.SelectedItem is DataRowView row)
            {
                string oldName = "";
                string oldDesc = "";

                // Eski veriyi almak için veritabanından sorgulama yapıyoruz
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string selectQuery = "SELECT name, description FROM COUNTRIES WHERE Id = @Id";
                    SqlCommand selectCmd = new SqlCommand(selectQuery, conn);
                    selectCmd.Parameters.AddWithValue("@Id", row["Id"]);

                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldName = reader["name"].ToString();
                            oldDesc = reader["description"].ToString();
                        }
                    }
                }

                string newName = txtCountryName.Text;
                string newDesc = txtCountryDesc.Text;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE COUNTRIES SET " +
                                         "name = @name, " +
                                         "description = @description " +
                                         "WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(updateQuery, conn);
                    cmd.Parameters.AddWithValue("@name", txtCountryName.Text);
                    cmd.Parameters.AddWithValue("@description", txtCountryDesc.Text);
                    cmd.Parameters.AddWithValue("@Id", row["Id"]);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.");
                    Clean();
                    TableRead();
                }

                string logMessage = $"Seçili Ülke Güncellendi. " +
                                 $"Ülke adı: {oldName} -> {newName},\n " +
                                 $"Açıklama: {oldDesc} -> {newDesc}, ";

                LogUserProcess.AddLog(currentUserId, logMessage);
            }
        }


        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Silmek istediğine emin misin?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (dgCountries.SelectedItem is DataRowView row)
                {
                    int currentUserId = LoginPage.FindUserId.CurrentUserId;

                    string deletedName = "";
                    string deletedDesc = "";

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string selectQuery = "SELECT name, description FROM COUNTRIES WHERE Id = @Id";
                        SqlCommand selectCmd = new SqlCommand(selectQuery, conn);
                        selectCmd.Parameters.AddWithValue("@Id", row["Id"]);

                        using (SqlDataReader reader = selectCmd.ExecuteReader())
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
                        string deleteQuery = "DELETE FROM COUNTRIES WHERE Id = @Id";
                        SqlCommand cmd = new SqlCommand(deleteQuery, conn);
                        cmd.Parameters.AddWithValue("@Id", row["Id"]);
                        cmd.ExecuteNonQuery();
                    }


                    MessageBox.Show("Seçilen ülke silindi.");
                    string logMessage = $"Kayıt Silindi. " +
                                        $"Ad: {deletedName},\n " +
                                        $"Açıklama: {deletedDesc}";
                    LogUserProcess.AddLog(currentUserId, logMessage);

                    Clean();
                    TableRead();
                }
            }
        }


        private void Clean()
        {
            txtCountryName.Text = "";
            txtCountryDesc.Text = "";
        }
    }
}
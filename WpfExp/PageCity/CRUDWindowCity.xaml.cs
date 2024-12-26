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
using WpfExp.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WpfExp.PageCity
{
    /// <summary>
    /// Interaction logic for CRUDWindowCity.xaml
    /// </summary>
    public partial class CRUDWindowCity : Window
    {
        private string connStr = @"Server=DESKTOP-F5QQ2O8;Database=OZGENC;Trusted_Connection=True;";
        public CRUDWindowCity()
        {
            InitializeComponent();
            TableRead();
            cmbxFull();
        }
        private void cmbxFull() //combobox içini doldurduk
        {
            using(SqlConnection conn = new SqlConnection(connStr))
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
        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string readQueary = "SELECT ci.Id, ci.name AS 'Şehir Adı', ci.description AS 'Açıklama', co.name AS 'Ülke' FROM CITIES ci " +
                    "INNER JOIN COUNTRIES co ON " +
                    "ci.countryId = co.Id";
                using(SqlCommand cmd = new SqlCommand(readQueary, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgCities.ItemsSource = dt.DefaultView;
                }
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

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;
            if (dgCities.SelectedItem is DataRowView row)
            {
                string oldName = "";
                string oldDesc = "";
                int oldCountryId = 0;

                // Eski verileri al
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string selectQuery = "SELECT name, description, countryId FROM CITIES WHERE Id = @Id";
                    SqlCommand selectCmd = new SqlCommand(selectQuery, conn);
                    selectCmd.Parameters.AddWithValue("@Id", row["Id"]);

                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldName = reader["name"].ToString();
                            oldDesc = reader["description"].ToString();
                            oldCountryId = Convert.ToInt32(reader["countryId"]);
                        }
                    }
                }

                // Yeni verileri al
                string newName = txtCityName.Text;
                string newDesc = txtCityDesc.Text;
                int newCountryId = Convert.ToInt32(cmbCountry.SelectedValue);

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE CITIES SET " +
                        "name = @name," +
                        "description = @description," +
                        "countryId = @countryId " +
                        "WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(updateQuery, conn);

                    cmd.Parameters.AddWithValue("@name", newName);
                    cmd.Parameters.AddWithValue("@description", newDesc);
                    cmd.Parameters.AddWithValue("@countryId", newCountryId);
                    cmd.Parameters.AddWithValue("@Id", row["Id"]);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.");
                    Clean();
                    TableRead();
                }


                string action = $"Seçilen şehir güncellendi. "+
                                 $"Şehir adı: {oldName} -> {newName},\n " +
                                 $"Açıklama: {oldDesc} -> {newDesc},\n " +
                                 $"Ülke ID: {oldCountryId} -> {newCountryId}";

               LogUserProcess.AddLog(currentUserId, action); // Log ekle
            }
        }


        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Silmek istediğine emin misin?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
                if(dgCities.SelectedItem is DataRowView row)
                {
                    int currentUserId = LoginPage.FindUserId.CurrentUserId; // kullanıcıyı bul

                    string deletedName = "";
                    string deletedDesc = "";
                    int deletedCountryId = 0;

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string selectQuery = "SELECT name, description, countryId FROM CITIES WHERE Id = @Id";
                        SqlCommand selectCmd = new SqlCommand(selectQuery, conn);
                        selectCmd.Parameters.AddWithValue("@Id", row["Id"]);

                        using (SqlDataReader reader = selectCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                deletedName = reader["name"].ToString();
                                deletedDesc = reader["description"].ToString();
                                deletedCountryId = Convert.ToInt32(reader["countryId"]);
                            }
                        }
                    }

                    using (SqlConnection conn = new SqlConnection( connStr))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM CITIES " +
                            "WHERE Id = @Id";
                        SqlCommand cmd = new SqlCommand(deleteQuery, conn);
                        cmd.Parameters.AddWithValue("@Id", row["Id"]);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Kayıt silindi!");
                    string logMessage = $"Kayıt Silindi: " +
                                        $"Şehir Adı: {deletedName},\n " +
                                        $"Açıklama: {deletedDesc},\n " +
                                        $"Ülke: {deletedCountryId}";

                    Clean();
                    TableRead();
                }
            }
        }

        private void Clean()
        {
            txtCityName.Text = "";
            txtCityDesc.Text = "";
            cmbCountry.SelectedIndex = -1;
        }

        private void dgCity(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if(dgCities.SelectedItem is DataRowView row) 
                {
                    txtCityName.Text = row["Şehir Adı"].ToString();
                    txtCityDesc.Text = row["Açıklama"].ToString();

                    //Bu LINQ şeklinde yazım daha büyük veri setleri için geçerlidir.
                    var selectedCountry = cmbCountry.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == row["Ülke"].ToString());
                    cmbCountry.SelectedItem = selectedCountry;

                    /*var countryname = row["Ülke"].ToString(); Küçük veri setleri için kullanılabilir.
                   foreach (var item in cmbCountry.Items)
                   {
                       if ((item as DataRowView)?["name"].ToString() == countryname)
                       {
                           cmbCountry.SelectedItem = item;
                           break;
                       }
                   }*/ 
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("HATA!\n"+ ex.Message);
            }
        }
    }
}

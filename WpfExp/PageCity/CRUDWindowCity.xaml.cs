using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
using WpfExp.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WpfExp.PageCity
{
    /// <summary>
    /// Interaction logic for CRUDWindowCity.xaml
    /// </summary>
    public partial class CRUDWindowCity : Window
    {
        public string connStr = "Data Source= Turbocut-400-500.db; Version=3;";
        public CRUDWindowCity()
        {
            InitializeComponent();
            TableRead();
            cmbxFull();
        }
        private void cmbxFull() //combobox içini doldurduk
        {
            using(SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string queryCountry = "SELECT CountriesID,name FROM COUNTRIES";
                SQLiteCommand cocmd = new SQLiteCommand(queryCountry, conn);
                SQLiteDataAdapter daCountry = new SQLiteDataAdapter(cocmd);
                DataTable dtco = new DataTable();
                daCountry.Fill(dtco);

                cmbCountry.ItemsSource = dtco.DefaultView;
                cmbCountry.DisplayMemberPath = "name";
                cmbCountry.SelectedValuePath = "CountriesID";

                cmbCountry.UpdateLayout();
            }
        }
        public void TableRead()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string readQueary = "SELECT ci.CitiesID, ci.name AS 'Şehir Adı', ci.description AS 'Açıklama', co.name AS 'Ülke' FROM CITIES ci " +
                    "INNER JOIN COUNTRIES co ON " +
                    "ci.CountryID = co.CountriesID";
                using(SQLiteCommand cmd = new SQLiteCommand(readQueary, conn))
                {
                    SQLiteDataAdapter adapt = new SQLiteDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgCities.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            using(SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO CITIES (name, description, CountryID) " +
                    "VALUES(@name, @description, @CountryID)";
                SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@name", txtCityName.Text);
                cmd.Parameters.AddWithValue("@description", txtCityDesc.Text);
                cmd.Parameters.AddWithValue("@CountryID", cmbCountry.SelectedValue);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                Clean();
                TableRead();
            }
        }

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            if(dgCities.SelectedItem is DataRowView row)
            {
                using (SQLiteConnection conn = new SQLiteConnection(connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE CITIES SET " +
                        "name = @name," +
                        "description = @description," +
                        "CountryID = @CountryID " +
                        "WHERE CitiesID = @CitiesID";
                    SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn);

                    cmd.Parameters.AddWithValue("@name", txtCityName.Text);
                    cmd.Parameters.AddWithValue("@description", txtCityDesc.Text);
                    cmd.Parameters.AddWithValue("@CountryID", Convert.ToInt32(cmbCountry.SelectedValue));
                    cmd.Parameters.AddWithValue("@CitiesID", row["CitiesID"]);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.");
                    Clean();
                    TableRead();
                }
            }
        }

        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Silmek istediğine emin misin?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
                if(dgCities.SelectedItem is DataRowView row)
                {
                    using(SQLiteConnection conn = new SQLiteConnection( connStr))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM CITIES " +
                            "WHERE CitiesID = @CitiesID";
                        SQLiteCommand cmd = new SQLiteCommand(deleteQuery, conn);
                        cmd.Parameters.AddWithValue("@CountriesID", row["CountriesID"]);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Seçilen şehir silindi!");
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

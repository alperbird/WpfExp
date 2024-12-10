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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfExp.PageCountry
{
    /// <summary>
    /// Interaction logic for CRUDWindowCountry.xaml
    /// </summary>
    public partial class CRUDWindowCountry : Window
    {
        public string connStr = "Data Source= Turbocut-400-500.db; Version=3;";
        public CRUDWindowCountry()
        {
            InitializeComponent();
            TableRead();
        }

        public void TableRead() // tablo okuma  
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string query = "SELECT CountriesID, name AS 'Ülke Adı', description AS 'Açıklama' FROM COUNTRIES";
                using (SQLiteCommand cmd = new SQLiteCommand(query, conn)) 
                {
                    SQLiteDataAdapter adapt = new SQLiteDataAdapter(cmd);
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
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO COUNTRIES (name, description)" +
                    "VALUES(@name, @description)";
                SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@name", txtCountryName.Text);
                cmd.Parameters.AddWithValue("@description", txtCountryDesc.Text);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                Clean();

                TableRead();
            }
        }

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            if (dgCountries.SelectedItem is DataRowView row)
            {
                using (SQLiteConnection conn = new SQLiteConnection(connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE COUNTRIES SET " +
                        "name = @name, " +
                        "description = @description " +
                        "WHERE CountriesID = @CountriesID";
                    SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn);
                    cmd.Parameters.AddWithValue("@name", txtCountryName.Text);
                    cmd.Parameters.AddWithValue("@description", txtCountryDesc.Text);
                    cmd.Parameters.AddWithValue("@CountriesID", row["CountriesID"]);

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
            if (result == MessageBoxResult.Yes)
            {
                if (dgCountries.SelectedItem is DataRowView row)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(connStr))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM COUNTRIES " +
                            "WHERE CountriesID = @CountriesID";
                        SQLiteCommand cmd = new SQLiteCommand(deleteQuery, conn);
                        cmd.Parameters.AddWithValue("@CountriesID", row["CountriesID"]);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Seçilen ülke silindi.");
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
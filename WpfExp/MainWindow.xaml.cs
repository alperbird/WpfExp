using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.Runtime.InteropServices.ComTypes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WpfExp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connectionString = @"Data Source=C:\\Users\\root\\source\\repos\\WpfExp\\WpfExp\\bin\\Debug\\Turbocut-400-500.db;Version=3;";
        private SQLiteConnection conn;
        public MainWindow() 
        {
            InitializeComponent();
            LoadCountry();
            conn = new SQLiteConnection(connectionString);
            conn.Open();
            
        }
        // Ülke seçimi
        private void LoadCountry()
        {
           
        
            List<string> countries = new List<string>();

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT name FROM COUNTRIES"; 
                using (var command = new SQLiteCommand(query, conn))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        countries.Add(reader["name"].ToString());
                    }
                }
                ulkeler.ItemsSource = countries;
            }
        }
        
        private void countries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ulkeler.SelectedItem != null)
            {
                string selectedCountry = ulkeler.SelectedItem.ToString(); 
                LoadCities(selectedCountry);
            }
        }
        // Ülke id'sine göre şehir seçimi
        private void LoadCities(string selectedCountry)
        {
            List<string> cities = new List<string>();

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string citiesQuery = "SELECT CITIES.name FROM CITIES " +
                                     "INNER JOIN COUNTRIES ON CITIES.CountryID = COUNTRIES.CountriesID " +
                                     "WHERE COUNTRIES.CountriesID = @CountryID";

                using (var command = new SQLiteCommand(citiesQuery, conn))
                {
                    
                    
                    command.Parameters.AddWithValue("@CountryID", selectedCountry);

                    using (var reader = command.ExecuteReader())
                    {
                            while (reader.Read())
                            {
                                cities.Add(reader["name"].ToString());
                            }
                    }
                }
                sehirler.ItemsSource = cities;
            }
        }
    }
}

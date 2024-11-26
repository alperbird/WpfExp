using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace WpfExp
{
    public class Connection
    {
        
        private string connectionString = "Data Source=C:\\Users\\root\\source\\repos\\WpfExp\\WpfExp\\bin\\Debug\\Turbocut-400-500.db;Version=3;";
        private SQLiteConnection conn;

        public Connection()
        {
            conn = new SQLiteConnection(connectionString);
        }

        
        
        public List<string> GetCountry()
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
                return countries;
            }
        }

        public List<string> GetCities(string selectedCountry)
        {
            List<string> cities = new List<string>();

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string citiesQuery = "SELECT CITIES.name FROM CITIES " +
                                     "INNER JOIN COUNTRIES ON CITIES.CountryID = COUNTRIES.CountriesID " +
                                     "WHERE COUNTRIES.name = @name";

                using (var command = new SQLiteCommand(citiesQuery, conn))
                {
                    command.Parameters.AddWithValue("@name", selectedCountry);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cities.Add(reader["name"].ToString());
                        }
                    }
                }
                return cities;
            }
        }

        public List<string> GetCustomers(string selectedCity)
        {
            List<string> customers = new List<string>();

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string customerQuery = "SELECT CUSTOMER.name FROM CUSTOMER " +
                    "INNER JOIN CITIES ON CUSTOMER.CityID = CITIES.CitiesID " +
                    "WHERE CITIES.name = @name";  // Parametre ile şehir adını alıyoruz

                using (var command = new SQLiteCommand(customerQuery, conn))
                {
                    // Şehir adını parametre olarak ekliyoruz
                    command.Parameters.AddWithValue("@name", selectedCity);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(reader["name"].ToString());
                        }
                    }
                }
            }
            return customers;
        }
    }
   
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;
using WpfExp.Models;

namespace WpfExp
{
    public class Connection
    {

        private string connectionString = "Data Source= Turbocut-400-500.db;Version=3;";
        private SQLiteConnection conn;

        public Connection()
        {
            conn = new SQLiteConnection(connectionString);
        }


        #region DataGridİşlemleri
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

        public List<Customers> GetCustomers(string selectedCities)
        {
            List<Customers> customers = new List<Customers>();

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string customerQuery = "SELECT CUSTOMER.CustomerID, CUSTOMER.name FROM CUSTOMER " +
                    "INNER JOIN CITIES ON CUSTOMER.CityID = CITIES.CitiesID " +
                    "WHERE CITIES.name = @name";

                using (var command = new SQLiteCommand(customerQuery, conn))
                {
                    command.Parameters.AddWithValue("@name", selectedCities);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new Customers
                            {
                                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                name = reader["name"].ToString()
                            });
                        }
                    }
                }
            }
            return customers;
        }

        public DataTable GetMachines(int selectedCustomerId)
        {
            DataTable machineTbl = new DataTable();
            string machineQuery = "SELECT MACHINES.machID, MACHINES.name, MACHINES.description, MACHINES.custID, MACHINES.tag " +
                                  "FROM MACHINES " +
                                  "INNER JOIN CUSTOMER ON MACHINES.custID = CUSTOMER.CustomerID " +
                                  "WHERE MACHINES.custID = @custID";

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var command = new SQLiteCommand(machineQuery, conn))
                {
                    command.Parameters.AddWithValue("@custID", selectedCustomerId); 
                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(machineTbl);
                    }
                }
            }
            return machineTbl;
        }

        #endregion

        #region Details
        public DataTable DetailBrands(int machineID) // dayı buradan markaları çekeceğiz
        {
            DataTable brandTbl = new DataTable();
            string brandQuery = "SELECT name, description FROM PROFILES_BRANDS " +
                "WHERE machineID = @machineID";

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var command = new SQLiteCommand(brandQuery, conn))
                {
                    command.Parameters.AddWithValue("@machineID",machineID);
                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(brandTbl);
                    }
                }
            }
            return brandTbl;
        }

        public DataTable DetailSeries(int brandId) //markanın serisini çekeceğiz (search bir işe yaramadı kaldırdım)
        {
            DataTable seriesTbl = new DataTable();
            string serieQuery = "SELECT name, description FROM PROFILE_SERIES " +
                "WHERE brandID = @brandID";
            using(var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using ( var command = new SQLiteCommand(serieQuery, conn))
                {
                    command.Parameters.AddWithValue("@brandID", brandId);
                    
          
                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(seriesTbl);
                    }
                }
            }
            return seriesTbl;
        }

        public DataTable DetailExamples(int brandId)//markanın serisinin örneğini çekeceğiz
        {
            DataTable exampleTbl = new DataTable();
            string exmQuery = "SELECT name, description FROM PROFILE_TBL " +
                "WHERE seriesID = @seriesID";
            using( var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using( var command = new SQLiteCommand(exmQuery, conn))
                {
                    command.Parameters.AddWithValue("@seriesID", brandId);
                    using( var adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(exampleTbl);
                    }
                }
            }
            return exampleTbl;
        }
        #endregion
    }

}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data;
using WpfExp.Models;
using System.Data.SqlClient;

namespace WpfExp
{
    public class Connection
    {

        private string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        private SqlConnection conn;

        public Connection()
        {
            conn = new SqlConnection(connectionString);
        }


        #region DataGridİşlemleri
        public List<string> GetCountry()
        {

            List<string> countries = new List<string>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT name FROM COUNTRIES";
                using (var command = new SqlCommand(query, conn))
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

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string citiesQuery = "SELECT CITIES.name FROM CITIES " +
                                     "INNER JOIN COUNTRIES ON CITIES.countryId = COUNTRIES.Id " +
                                     "WHERE COUNTRIES.name = @name";

                using (var command = new SqlCommand(citiesQuery, conn))
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

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string customerQuery = "SELECT CUSTOMER.Id, CUSTOMER.name FROM CUSTOMER " +
                    "INNER JOIN CITIES ON CUSTOMER.cityId = CITIES.Id " +
                    "WHERE CITIES.name = @name";

                using (var command = new SqlCommand(customerQuery, conn))
                {
                    command.Parameters.AddWithValue("@name", selectedCities);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new Customers
                            {
                                CustomerID = Convert.ToInt32(reader["Id"]),
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
            string machineQuery = "SELECT cu.Id, ma.name, ma.description, cu.name AS customerName, snm.serialNum " +
                          "FROM MACHINES ma " +
                          "JOIN CUSTOMER cu ON cu.machineId = ma.Id " +  // Müşteri ve makine ilişkisi
                          "JOIN SERIAL_NUMBERS snm ON cu.serialnumId = snm.Id " + // Seri numarası ile ilişki
                          "WHERE cu.Id = @Id";  // Müşteri id'ye göre filtreleme


            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var command = new SqlCommand(machineQuery, conn))
                {
                    command.Parameters.AddWithValue("@Id", selectedCustomerId);
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(machineTbl);
                    }
                }
            }
            return machineTbl;
        }

        #endregion

        #region Details
        public DataTable DetailBrands(int custId) // dayı buradan markaları çekeceğiz
        {
            DataTable brandTbl = new DataTable();
            string brandQuery = "SELECT pb.name, pb.description FROM PROFILE_BRANDS pb " +
                "JOIN CUSTOMER cu ON cu.pbrandId = pb.Id " +
                "WHERE cu.Id = @Id";

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var command = new SqlCommand(brandQuery, conn))
                {
                    command.Parameters.AddWithValue("@Id", custId);
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(brandTbl);
                    }
                }
            }
            return brandTbl;
        }

        public DataTable DetailSeries(int custId) //markanın serisini çekeceğiz (search bir işe yaramadı kaldırdım)
        {
            DataTable seriesTbl = new DataTable();
            string serieQuery = "SELECT ps.name, ps.description FROM PROFILE_SERIES ps " +
                "JOIN CUSTOMER cu ON cu.pserieId = ps.Id " +
                "WHERE cu.Id = @Id";
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var command = new SqlCommand(serieQuery, conn))
                {
                    command.Parameters.AddWithValue("@Id", custId);


                    using (var adapter = new SqlDataAdapter(command))
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
            string exmQuery = "SELECT pe.name, pe.description FROM PROFILE_EXAMPLE pe " +
                "JOIN CUSTOMER cu ON cu.pexmId = pe.Id " +
                "WHERE cu.Id = @Id ";
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var command = new SqlCommand(exmQuery, conn))
                {
                    command.Parameters.AddWithValue("@Id", brandId);
                    using (var adapter = new SqlDataAdapter(command))
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



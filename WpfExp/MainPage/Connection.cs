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
        #region DATABASE CONNECTION
         
        private SqlConnection conn;

        public Connection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string is not initialized.");
            }
        }

        // Bağlantıyı döndüren bir metot
        public SqlConnection GetConnection()
        {
            string connStr = @"Server=192.168.5.68,1433;Database=OZGENC;User Id=sa; Password=ozgencmachine16;Trusted_Connection=True;"; // Her seferinde private olarak tanımladık
            conn = new SqlConnection(connStr);

            if (conn.State != ConnectionState.Open)
            {
                conn.Open(); // Bağlantı açılmamışsa açıyoruz
            }
            return conn;
        }

        // Bağlantıyı kapatan bir metot
        public void CloseConnection()
        {
            if (conn != null && conn.State == ConnectionState.Open)
            {
                conn.Close(); // Bağlantıyı kapatıyoruz
            }
        }
        #endregion

        #region DataGridİşlemleri
        public List<string> GetCountry()
        {

            List<string> countries = new List<string>();

            using (var conn = this.GetConnection())
            {
                string query = "SELECT name FROM COUNTRIES";
                using (var command = new SqlCommand(query, conn))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        countries.Add(reader["name"].ToString());
                    }
                }
                conn.Close();
                return countries;
            }
        }

        public List<string> GetCities(string selectedCountry)
        {
            List<string> cities = new List<string>();

            using (var conn = this.GetConnection())
            {
                string citiesQuery = "SELECT ci.name FROM CITIES ci " +
                                     "INNER JOIN COUNTRIES co ON ci.countryId = co.Id " +
                                     "WHERE co.name = @name";

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
                conn.Close();
                return cities;
            }
        }

        public List<Customers> GetCustomers(string selectedCities)
        {
            List<Customers> customers = new List<Customers>();

            using (var conn = this.GetConnection())
            {
                string customerQuery = "SELECT cu.Id, cu.name FROM CUSTOMER cu " +
                    "INNER JOIN CITIES ci ON cu.cityId = ci.Id " +
                    "WHERE ci.name = @name";

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
                conn.Close();
            }
            return customers;
        }

        public DataTable GetMachines(int selectedCustomerId)
        {
            DataTable machineTbl = new DataTable();
            string machineQuery = @"
                                    SELECT 
                                    cm.customerId AS 'CustomerId',
                                    ma.name AS 'MachineName',
                                    ma.description AS 'description',
                                    snm.serialNum AS 'SerialNumber'
                                    FROM CUSTOMER_MACHINE cm
                                    JOIN MACHINES ma ON cm.machineId = ma.Id
                                    LEFT JOIN SERIAL_NUMBERS snm ON ma.Id = snm.machineId AND cm.customerId = snm.customerId
                                    WHERE cm.customerId = @CustomerId";


            using (var conn = this.GetConnection())
            {
                using (var command = new SqlCommand(machineQuery, conn))
                {
                    command.Parameters.AddWithValue("@CustomerId", selectedCustomerId);
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(machineTbl);
                    }
                }
                conn.Close();
            }
            return machineTbl;
        }

        #endregion

        #region Details
        public DataTable DetailBrands(int custId) // dayı buradan markaları çekeceğiz
        {
            DataTable brandTbl = new DataTable();
            string brandQuery = @"
                                SELECT DISTINCT pb.name, pb.description 
                                FROM CUSTOMER_BRANDS cb
                                JOIN PROFILE_BRANDS pb ON cb.brandId = pb.Id
                                WHERE cb.customerId = @customerId";

            using (var conn = this.GetConnection())
            {
                using (var command = new SqlCommand(brandQuery, conn))
                {
                    command.Parameters.AddWithValue("@customerId", custId);
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(brandTbl);
                    }
                }
                conn.Close();
            }
            return brandTbl;
        }

        public DataTable DetailSeries(int custId) //markanın serisini çekeceğiz (search bir işe yaramadı kaldırdım)
        {
            DataTable seriesTbl = new DataTable();
            string serieQuery = @"
                                SELECT DISTINCT ps.name, ps.description 
                                FROM CUSTOMER_BRANDS cb
                                JOIN BRAND_SERIES bs ON cb.brandId = bs.brandId
                                JOIN PROFILE_SERIES ps ON bs.seriesId = ps.Id
                                WHERE cb.customerId = @customerId
                                AND bs.customerId = @customerId;
                                ";
            using (var conn = this.GetConnection())
            {
                using (var command = new SqlCommand(serieQuery, conn))
                {
                    command.Parameters.AddWithValue("@customerId", custId);


                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(seriesTbl);
                    }
                }
                conn.Close();
            }
            return seriesTbl;
        }

        public DataTable DetailExamples(int custId)//markanın serisinin örneğini çekeceğiz
        {
            DataTable exampleTbl = new DataTable();
            string exmQuery = @"
                              SELECT 
                              pe.Id, 
                              pe.filetype, 
                              DATALENGTH(pe.filecontent) / 1024 AS ContentSize -- KB olarak
                              FROM CUSTOMER_PROFILE_EXAMPLES cpe
                              INNER JOIN PROFILE_EXAMPLE pe ON cpe.profileExampleId = pe.Id
                              WHERE cpe.customerId = @customerId";

            using (var conn = this.GetConnection())
            {
                using (var command = new SqlCommand(exmQuery, conn))
                {
                    command.Parameters.AddWithValue("@customerId", custId);
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(exampleTbl);
                    }
                }
                conn.Close();
            }
            return exampleTbl;
        }

        public byte[] GetFileContent(int fileId, out string fileType)
        {
            fileType = null;
            byte[] fileContent = null;

            using (var connection = this.GetConnection())
            {
                string query = "SELECT filetype, filecontent FROM PROFILE_EXAMPLE WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", fileId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fileType = reader["filetype"].ToString();
                            fileContent = reader["filecontent"] as byte[];
                        }
                    }
                }
            }

            return fileContent;
        }


        #endregion
    }

}



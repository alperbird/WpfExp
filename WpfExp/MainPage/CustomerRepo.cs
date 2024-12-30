using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfExp.Models;

namespace WpfExp.MainPage
{
    public class CustomerRepo
    {
        private readonly Connection _conn;

        public CustomerRepo(Connection conn)
        {
            _conn = conn;
        }
        public List<Customers> GetCustomers(string selectedCities)
        {
            List<Customers> customers = new List<Customers>();

            using (var conn = _conn.GetConnection())
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
            }
            return customers;
        }
    }
}

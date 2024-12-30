using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExp.MainPage
{
    public class CityRepo
    {
        private readonly Connection _conn;
        public CityRepo(Connection conn)
        {
            _conn = conn;
        }

        public List<string> GetCities(string selectedCountry)
        {
            List<string> cities = new List<string>();

            using (var conn = _conn.GetConnection())
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
                return cities;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace WpfExp.MainPage
{
    public class CountryRepo
    {
        private readonly Connection _conn;
        public CountryRepo(Connection conn)
        {
            _conn = conn;
        }
        public List<string> GetCountry()
        {
            List<string> countries = new List<string>();
            using (var conn = _conn.GetConnection())
            {
                string query = "SELECT name FROM COUNTRIES";
                using (var command = new SqlCommand(query, conn))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        countries.Add(reader.GetString(0));
                    }
                }
            }
            return countries;
        }
    }
}

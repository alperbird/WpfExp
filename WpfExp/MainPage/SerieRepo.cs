using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExp.MainPage
{
    public class SerieRepo
    {
        private readonly Connection _conn;
        public SerieRepo(Connection conn)
        {
            _conn = conn;
        }

        public DataTable DetailSeries(int custId) //markanın serisini çekeceğiz
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
            using (var conn = _conn.GetConnection())
            {
                using (var command = new SqlCommand(serieQuery, conn))
                {
                    command.Parameters.AddWithValue("@customerId", custId);


                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(seriesTbl);
                    }
                }
            }
            return seriesTbl;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExp.MainPage
{
    public class ExampleRepoNonCustomer // yeni repo ve sorgu
    {
        private readonly Connection _conn;

        public ExampleRepoNonCustomer(Connection conn)
        {
            _conn = conn;
        }

        public DataTable GetExamplesBySeries(int seriesId)
        {
            DataTable examplesTbl = new DataTable();
            string query = @"
                           SELECT 
                           Id, 
                           name, 
                           filetype, 
                           DATALENGTH(filecontent) / 1024 AS FileSize 
                           FROM PROFILE_EXAMPLE 
                           WHERE serieId = @seriesId";

            using (var conn = _conn.GetConnection())
            {
                using (var command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@seriesId", seriesId);
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(examplesTbl);
                    }
                }
            }
            return examplesTbl;
        }

    }
}

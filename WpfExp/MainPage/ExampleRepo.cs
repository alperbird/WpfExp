using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExp.MainPage
{
    public class ExampleRepo
    {
        private readonly Connection _conn;
        public ExampleRepo(Connection conn)
        {
            _conn = conn;
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

            using (var conn = _conn.GetConnection())
            {
                using (var command = new SqlCommand(exmQuery, conn))
                {
                    command.Parameters.AddWithValue("@customerId", custId);
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(exampleTbl);
                    }
                }
            }
            return exampleTbl;
        }
    }
}

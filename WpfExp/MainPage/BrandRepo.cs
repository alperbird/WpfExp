using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExp.MainPage
{
    public class BrandRepo
    {
        private readonly Connection _conn;
        public BrandRepo(Connection conn)
        {
            _conn = conn;
        }

        public DataTable DetailBrands(int custId) 
        {
            DataTable brandTbl = new DataTable();
            string brandQuery = @"
                                SELECT DISTINCT pb.name, pb.description 
                                FROM CUSTOMER_BRANDS cb
                                JOIN PROFILE_BRANDS pb ON cb.brandId = pb.Id
                                WHERE cb.customerId = @customerId";

            using (var conn = _conn.GetConnection())
            {
                using (var command = new SqlCommand(brandQuery, conn))
                {
                    command.Parameters.AddWithValue("@customerId", custId);
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(brandTbl);
                    }
                }
            }
            return brandTbl;
        }
    }
}

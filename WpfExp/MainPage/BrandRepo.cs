using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfExp.Models;

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

        public List<ProfileBrands> GetAllBrands()
        {
            var brands = new List<ProfileBrands>();
            using (var conn = _conn.GetConnection())
            {
                string query = "SELECT Id, name FROM PROFILE_BRANDS"; // Id ve name alınmalı
                using (var command = new SqlCommand(query, conn))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        brands.Add(new ProfileBrands
                        {
                            Id = reader.GetInt32(0), // Id
                            name = reader.GetString(1) // name
                        });
                    }
                }
            }
            return brands;
        }



    }
}

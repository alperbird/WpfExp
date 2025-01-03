using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExp.MainPage
{
    public class MachineRepo
    {
        private readonly Connection _conn;
        public MachineRepo(Connection conn)
        {
            _conn = conn;
        }
        public DataTable GetMachines(int selectedCustomerId)
        {
            DataTable machineTbl = new DataTable();
            string machineQuery = @"
                SELECT 
                cu.Id AS CustomerId,    
                cu.name AS CustomerName,
                ma.name AS MachineName,
                ma.description AS description,
                pb.name AS BrandName,
                sn.serialNum AS SerialNumber
                FROM CUSTOMER cu
                LEFT JOIN CUSTOMER_MACHINE cm ON cu.Id = cm.customerId
                LEFT JOIN MACHINES ma ON cm.machineId = ma.Id
                LEFT JOIN SERIAL_NUMBERS sn ON ma.Id = sn.machineId AND cu.Id = sn.customerId
                LEFT JOIN CUSTOMER_BRANDS cb ON cu.Id = cb.customerId
                LEFT JOIN PROFILE_BRANDS pb ON cb.brandId = pb.Id
                WHERE cu.Id = @CustomerId";


            using (var conn = _conn.GetConnection())
            {
                using (var command = new SqlCommand(machineQuery, conn))
                {
                    command.Parameters.AddWithValue("@CustomerId", selectedCustomerId);
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(machineTbl);
                    }
                }
            }
            return machineTbl;
        }
    }
}

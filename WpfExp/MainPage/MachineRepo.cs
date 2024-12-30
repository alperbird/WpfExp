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
                                    cm.customerId AS 'CustomerId',
                                    ma.name AS 'MachineName',
                                    ma.description AS 'description',
                                    snm.serialNum AS 'SerialNumber'
                                    FROM CUSTOMER_MACHINE cm
                                    JOIN MACHINES ma ON cm.machineId = ma.Id
                                    LEFT JOIN SERIAL_NUMBERS snm ON ma.Id = snm.machineId AND cm.customerId = snm.customerId
                                    WHERE cm.customerId = @CustomerId";


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

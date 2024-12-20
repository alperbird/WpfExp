using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfExp.Models;

namespace WpfExp.PageCustomer
{
    public class CRUDCustomer
    {
        private string connectionStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        private SQLiteConnection connection;

        public CRUDCustomer()
        {
            connection = new SQLiteConnection(connectionStr);
        }
    }
}

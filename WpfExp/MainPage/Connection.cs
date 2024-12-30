using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data;
using WpfExp.Models;
using System.Data.SqlClient;
using System.Configuration;

namespace WpfExp
{
    public class Connection
    {
        #region DATABASE CONNECTION

        private readonly string _connectionString;

        public Connection()
        {
            // Connection string App.config'den okunuyor.
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public SqlConnection GetConnection()
        {
            var conn = new SqlConnection(_connectionString);
            try
            {
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                throw new Exception("Database connection failed: " + ex.Message, ex);
            }
        }

        public void CloseConnection(SqlConnection conn)
        {
            if (conn != null && conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
            }
        }
        #endregion

    }

}



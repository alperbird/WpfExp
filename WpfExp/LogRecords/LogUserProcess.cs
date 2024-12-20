using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExp.LogRecords
{
    public static class LogUserProcess
    {
        private static  string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        private static string logFilePath = "C:\\Users\\root\\source\\repos\\WpfExp\\WpfExp\\LogRecords\\Logs.txt"; 

        public static void AddLog(int userId, string action) // datagrid içinde tutacağımız log kayıtları (uygulama içinde evet)
        {
            using(SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO LOGS(UserId, Action, TimeStamp) VALUES (@UserId, @Action, @TimeStamp)";

                using(SqlCommand cmd = new SqlCommand(insertQuery,conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@TimeStamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }

            AddLogToFile(userId, action);
        }

        public static void AddLogToFile(int userId, string action) // ne olur ne olmaz elimizde txt dosyası da olsun
        {
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | UserID: {userId} | Action: {action}";
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Log dosyasına yazılamadı: {ex.Message}");
            }
        }
    }
}


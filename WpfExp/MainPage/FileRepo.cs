using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExp.MainPage
{
    public class FileRepo
    {
        private readonly Connection _conn;

        public FileRepo(Connection conn)
        {
            _conn = conn;
        }

        public byte[] GetFileContent(int fileId, out string fileType)
        {
            fileType = null;
            byte[] fileContent = null;

            using (var connection = _conn.GetConnection())
            {
                string query = "SELECT filetype, filecontent FROM PROFILE_EXAMPLE WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", fileId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fileType = reader["filetype"].ToString();
                            fileContent = reader["filecontent"] as byte[];
                        }
                    }
                }
            }
            return fileContent;
        }
    }
}

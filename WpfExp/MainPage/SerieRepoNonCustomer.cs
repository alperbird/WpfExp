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
    public class SerieRepoNonCustomer //yeni repo ve sorgu
    {
        private readonly Connection _conn;

        public SerieRepoNonCustomer(Connection conn)
        {
            _conn = conn;
        }

        // müşteri bağlantısı olmadan markaları getiren SQL sorgusu
        public List<ProfileSeries> GetSeriesByBrand(int brandId)
        {
            var seriesList = new List<ProfileSeries>();
            using (var conn = _conn.GetConnection())
            {
                string query = "SELECT Id, name FROM PROFILE_SERIES WHERE brandId = @brandId";
                using (var command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@brandId", brandId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            seriesList.Add(new ProfileSeries
                            {
                                Id = reader.GetInt32(0), // Id
                                name = reader.GetString(1) // name
                            });
                        }
                    }
                }
            }
            return seriesList;
        }

        public List<FileModel> GetFilesBySeriesId(int seriesId)
        {
            var fileList = new List<FileModel>();

            using (var connection = _conn.GetConnection())
            {
                string query = @"
            SELECT 
                Id, 
                filetype, 
                filecontent 
            FROM PROFILE_EXAMPLE 
            WHERE serieId = @serieId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@serieId", seriesId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fileList.Add(new FileModel
                            {
                                Id = reader.GetInt32(0),
                                FileType = reader.GetString(1),
                                ContentSize = reader["filecontent"] as byte[]
                            });
                        }
                    }
                }
            }

            return fileList;
        }


    }
}

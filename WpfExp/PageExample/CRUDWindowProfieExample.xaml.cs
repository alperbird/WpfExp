using System;
using System.Collections.Generic;
using System.Data.Sql;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using WpfExp.LogRecords;

namespace WpfExp.PageExample
{
    /// <summary>
    /// Interaction logic for CRUDWindowProfieExample.xaml
    /// </summary>
    public partial class CRUDWindowProfieExample : Window
    {
        public string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        public CRUDWindowProfieExample()
        {
            InitializeComponent();
            TableRead();
        }
        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string exmQuery = "SELECT Id, name AS 'Ad', description AS 'Açıklama' FROM PROFILE_EXAMPLE";
                using (SqlCommand cmd = new SqlCommand(exmQuery, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgExamples.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void dgExample(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgExamples.SelectedItem is DataRowView row)
                {
                    txtExmName.Text = row["Ad"].ToString();
                    txtExmDesc.Text = row["Açıklama"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("HATA!\n" + ex.Message);
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO PROFILE_EXAMPLE (name, description) " +
                    "VALUES(@name, @description)";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);

                cmd.Parameters.AddWithValue("@name", txtExmName.Text);
                cmd.Parameters.AddWithValue("@description", txtExmDesc.Text);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                LogUserProcess.AddLog(currentUserId, $"Örnek eklendi: {txtExmName.Text}");
                Clean();
                TableRead();
            }
        }

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;
            if (dgExamples.SelectedItem is DataRowView row)
            {
                string oldName = "";
                string oldDesc = "";
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT Id, name, description FROM PROFILE_EXAMPLE WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldName = reader["name"].ToString();
                            oldDesc = reader["description"].ToString();
                        }
                    }
                }


                string newName = txtExmName.Text;
                string newDesc = txtExmDesc.Text;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE PROFILE_EXAMPLE SET " +
                        "name = @name," +
                        "description = @description," +
                        "fileType = @fileType " +
                        "WHERE Id = @Id";

                    SqlCommand cmd = new SqlCommand(updateQuery, conn);

                    cmd.Parameters.AddWithValue("@name", txtExmName.Text);
                    cmd.Parameters.AddWithValue("@description", txtExmDesc.Text);
                    cmd.Parameters.AddWithValue("@Id", row["Id"]);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.");
                    Clean();
                    TableRead();
                }
                string logMessage = $"Seçilen Örnek Güncellendi: " +
                                    $"Örnek Adı: {oldName} -> {newName}, " +
                                    $"Açıklama: {oldDesc} -> {newDesc}";
                LogUserProcess.AddLog(currentUserId, logMessage);
            }
        }

        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Silmek istediğine emin misin?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                int currentUserId = LoginPage.FindUserId.CurrentUserId;

                if (dgExamples.SelectedItem is DataRowView row)
                {
                    string deletedName = "";
                    string deletedDesc = "";
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string query = "SELECT Id, name, description FROM PROFILE_EXAMPLE WHERE Id = @Id";
                        SqlCommand cmd = new SqlCommand(query, conn);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                deletedName = reader["name"].ToString();
                                deletedDesc = reader["description"].ToString();
                            }
                        }
                    }

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM PROFILE_EXAMPLE " +
                            "WHERE Id = @Id";

                        SqlCommand cmd = new SqlCommand(deleteQuery, conn);

                        cmd.Parameters.AddWithValue("@Id", row["Id"]);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Seçilen örnek silindi.");
                        string logMessage = $"Kayıt Silindi: " +
                                        $"Şehir Adı: {deletedName}, " +
                                        $"Açıklama: {deletedDesc}";
                        LogUserProcess.AddLog(currentUserId, logMessage);
                        Clean();
                        TableRead();

                    }
                }
            }
        }

        private void Clean()
        {
            txtExmName.Text = "";
            txtExmDesc.Text = "";
        }
    }
}

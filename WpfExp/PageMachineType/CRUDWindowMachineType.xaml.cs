using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using WpfExp.LogRecords;
using WpfExp.Models;

namespace WpfExp.PageMachineType
{
    /// <summary>
    /// Interaction logic for CRUDWindowMachineType.xaml
    /// </summary>
    public partial class CRUDWindowMachineType : Window
    {
        public string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        public CRUDWindowMachineType()
        {
            InitializeComponent();
            TableRead();
        }

        public void TableRead()
        {
            using(SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string typeQuery = "SELECT Id, name AS 'İşlem Adı', description AS 'Açıklama' FROM MACHINE_TYPE";
                using(SqlCommand cmd = new SqlCommand(typeQuery, conn))
                {
                    SqlDataAdapter adap = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adap.Fill(dt);

                    dgMachineTypes.ItemsSource = dt.DefaultView;
                }

            }
        }

        private void dgMachineType(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgMachineTypes.SelectedItem is DataRowView row)
                {
                    txtTypeName.Text = row["İşlem Adı"].ToString();
                    txtTypeDesc.Text = row["Açıklama"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata patlatmayacak ama deneyelim yine de\n" + ex.Message);
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO MACHINE_TYPE(name, description) VALUES(@name, @description)";
                
                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@name", txtTypeName.Text);
                cmd.Parameters.AddWithValue("@description", txtTypeDesc.Text);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                LogUserProcess.AddLog(currentUserId, $"Makine Tipi Eklendi: {txtTypeName.Text}");

                Clean();

                TableRead();
            }
        }

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;
            if (dgMachineTypes.SelectedItem is DataRowView row)
            {
                string oldName = "";
                string oldDesc = "";
                
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT Id, name, description FROM MACHINE_TYPE WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", row["Id"]);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldName = reader["name"].ToString();
                            oldDesc = reader["description"].ToString();
                        }
                    }

                }

                string newName = txtTypeName.Text;
                string newDesc = txtTypeDesc.Text;
                using(SqlConnection conn = new SqlConnection( connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE MACHINE_TYPE SET " +
                        "name = @name," +
                        "description = @description " +
                        "WHERE Id = @Id";

                    SqlCommand cmd = new SqlCommand(updateQuery, conn);
                    cmd.Parameters.AddWithValue("@name", txtTypeName.Text);
                    cmd.Parameters.AddWithValue("@description", txtTypeDesc.Text);
                    cmd.Parameters.AddWithValue("@Id", row["Id"]);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Makine işlemi başarıyla güncellendi.");
                    Clean();

                    TableRead();
                }
                string logMessage = $"Seçili Ülke Güncellendi. " +
                                 $"Ülke adı: {oldName} -> {newName},\n " +
                                 $"Açıklama: {oldDesc} -> {newDesc}, ";

                LogUserProcess.AddLog(currentUserId, logMessage);
            }

        }

        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Silmek istediğine emin misin?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if(dgMachineTypes.SelectedItem is DataRowView row)
                {
                    int currentUserId = LoginPage.FindUserId.CurrentUserId;

                    string deletedName = "";
                    string deletedDesc = "";
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string query = "SELECT Id, name, description FROM MACHINE_TYPE WHERE Id = @Id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", row["Id"]);

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
                        string deleteQuery = "DELETE FROM MACHINE_TYPE " +
                            "WHERE Id = @Id";
                        SqlCommand cmd = new SqlCommand(deleteQuery, conn);

                        cmd.Parameters.AddWithValue("@Id", row["Id"]);
                        cmd.ExecuteNonQuery ();
                    }
                    MessageBox.Show("Seçilen işlem silindi.");
                    string logMessage = $"Kayıt Silindi. " +
                                        $"Ad: {deletedName},\n " +
                                        $"Açıklama: {deletedDesc}";

                    LogUserProcess.AddLog(currentUserId, logMessage);

                    Clean();
                    TableRead();
                }
            }
        }

        private void Clean()
        {
            txtTypeName.Text = "";
            txtTypeDesc.Text = "";
        }
    }
    
}

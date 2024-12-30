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
using WpfExp.Models;
using System.Data.SqlClient;
using WpfExp.LogRecords;
using System.Configuration;

namespace WpfExp.PageMachine
{
    /// <summary>
    /// Interaction logic for CRUDWindowMachine.xaml
    /// </summary>
    public partial class CRUDWindowMachine : Window
    {
        private string connStr;
        public CRUDWindowMachine()
        {
            InitializeComponent();
            connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            TableRead();
            cmbxFull();
        }

        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string machQuery = "SELECT ma.Id, ma.name AS 'Makine Adı', ma.description AS 'Açıklama', mt.name AS 'Makine Türü' FROM MACHINES ma " +
                    "JOIN MACHINE_TYPE mt ON ma.typeId = mt.Id";
                using (SqlCommand cmd = new SqlCommand(machQuery, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgMachines.ItemsSource = dt.DefaultView;
                }
            }
        }
        private void cmbxFull()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT name FROM MACHINE_TYPE";
                SqlCommand cucmd = new SqlCommand(query, conn);
                SqlDataAdapter daCust = new SqlDataAdapter(cucmd);
                DataTable dt = new DataTable();
                daCust.Fill(dt);

                cmbType.ItemsSource = dt.DefaultView;
                cmbType.DisplayMemberPath = "name";
                cmbType.SelectedValuePath = "Id";

            }
        }
        private void dgMachine(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgMachines.SelectedItem is DataRowView row)
                {
                    txtMachineName.Text = row["Makine Adı"].ToString();
                    txtMachDesc.Text = row["Açıklama"].ToString();
                    var selectedCustomer = cmbType.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == row["Makine Türü"].ToString());
                    cmbType.SelectedItem = selectedCustomer;

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
                string insertQuery = "INSERT INTO MACHINES(name, description, typeId) " +
                    "VALUES(@name, @description, @typeId)";
                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@name", txtMachineName.Text);
                cmd.Parameters.AddWithValue("@description", txtMachDesc.Text);
                cmd.Parameters.AddWithValue("@typeId", cmbType.SelectedValue);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi");
                LogUserProcess.AddLog(currentUserId, $"Makine Kaydı Eklendi: {txtMachineName}");

                Clean();
                TableRead();
            }
        }

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;

            if (dgMachines.SelectedItem is DataRowView row)
            {
                string oldName = "";
                string oldDesc = "";
                int oldTypeId = 0;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string selectedQuery = "SELECT name, description, typeId FROM MACHINES WHERE Id = @Id";

                    SqlCommand selectcmd = new SqlCommand(selectedQuery, conn);
                    selectcmd.Parameters.AddWithValue("@Id", row["Id"]);

                    using (SqlDataReader reader = selectcmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldName = reader["name"].ToString();
                            oldDesc = reader["description"].ToString();
                            oldTypeId = Convert.ToInt32(reader["typeId"]);
                        }
                    }
                }

                string newName = txtMachineName.Text;
                string newDesc = txtMachDesc.Text;
                int newTypeId = Convert.ToInt32(cmbType.SelectedValue);

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE MACHINES SET " +
                        "name = @name," +
                        "description = @description," +
                        "typeId = @typeId," +
                        "WHERE Id = @Id ";

                    SqlCommand cmd = new SqlCommand(updateQuery, conn);

                    cmd.Parameters.AddWithValue("@name", txtMachineName.Text);
                    cmd.Parameters.AddWithValue("@description", txtMachDesc.Text);
                    cmd.Parameters.AddWithValue("@typeId", Convert.ToInt32(cmbType.SelectedValue));
                    cmd.Parameters.AddWithValue("@Id", row["Id"]);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.");
                    Clean();
                    TableRead();
                }

                string action = $"Seçilen Makina Güncellendi: " +
                                $"Makine Adı: {oldName} -> {newName},\n " +
                                $"Açıklama: {oldDesc} -> {newDesc},\n " +
                                $"Makine Tipi: {oldTypeId} -> {newTypeId}";
                LogUserProcess.AddLog(currentUserId, action);

            }
        }

        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Silmek istediğine emin misin?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (dgMachines.SelectedItem is DataRowView row)
                {
                    int currentUserId = LoginPage.FindUserId.CurrentUserId;

                    string deletedName = "";
                    string deletedDesc = "";
                    int deletedTypeId = 0;
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string selectQuery = "SELECT name, description, typeId FROM CITIES WHERE Id = @Id";
                        SqlCommand selectCmd = new SqlCommand(selectQuery, conn);
                        selectCmd.Parameters.AddWithValue("@Id", row["Id"]);

                        using (SqlDataReader reader = selectCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                deletedName = reader["name"].ToString();
                                deletedDesc = reader["description"].ToString();
                                deletedTypeId = Convert.ToInt32(reader["countryId"]);
                            }
                        }
                    }

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM MACHINES " +
                            "WHERE Id = @Id";

                        SqlCommand cmd = new SqlCommand(deleteQuery, conn);
                        cmd.Parameters.AddWithValue("@Id", row["Id"]);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Seçilen makine silindi.");

                        string logMessage = $"Kayıt Silindi: " +
                                        $"Şehir Adı: {deletedName},\n " +
                                        $"Açıklama: {deletedDesc},\n " +
                                        $"Ülke: {deletedTypeId}";
                        LogUserProcess.AddLog(currentUserId, logMessage);
                        Clean();
                        TableRead();
                    }
                }

            }
        }

        private void Clean()
        {
            txtMachineName.Text = "";
            txtMachDesc.Text = "";
            cmbType.SelectedIndex = -1;
        }
    }
}

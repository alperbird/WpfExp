using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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

namespace WpfExp.PageBrand
{
    /// <summary>
    /// Interaction logic for CRUDWindowProfileBrand.xaml
    /// </summary>
    public partial class CRUDWindowProfileBrand : Window
    {
        public string connStr = "Data Source= Turbocut-400-500.db; Version=3;";
        public CRUDWindowProfileBrand()
        {
            InitializeComponent();
            TableRead();
            cmbxFull();
        }

        public void TableRead()
        {
            using(SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string brandQuery = "SELECT b.ProfileID, b.name AS 'Marka Adı', b.description AS 'Açıklama', m.name AS 'Makine' FROM PROFILES_BRANDS b " +
                    "INNER JOIN MACHINES m ON " +
                    "b.machineID = m.machID";

                using(SQLiteCommand cmd = new SQLiteCommand(brandQuery, conn))
                {
                    SQLiteDataAdapter adapt = new SQLiteDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgBrands.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void cmbxFull()
        {
            using(SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string query = "SELECT machID, name FROM MACHINES";

                SQLiteCommand mcmd = new SQLiteCommand(query, conn);
                SQLiteDataAdapter dam = new SQLiteDataAdapter(mcmd);
                DataTable dt = new DataTable();
                dam.Fill(dt);

                cmbMachine.ItemsSource = dt.DefaultView;
                cmbMachine.DisplayMemberPath = "name";
                cmbMachine.SelectedValuePath = "machID";
            }
        }

        private void dgBrand(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgBrands.SelectedItem is DataRowView row)
                {
                    txtBrandName.Text = row["Marka Adı"].ToString();
                    txtBrandDesc.Text = row["Açıklama"].ToString();

                    var selectedMach = cmbMachine.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == row["Makine"].ToString());
                    cmbMachine.SelectedItem = selectedMach;
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("HATA\n" + ex.Message);
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            using(SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO PROFILES_BRANDS(name, description, machineID) " +
                    "VALUES(@name, @description, @machineID)";

                SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn);

                cmd.Parameters.AddWithValue("@name", txtBrandName.Text);
                cmd.Parameters.AddWithValue("@description", txtBrandDesc);
                cmd.Parameters.AddWithValue("@machineID", cmbMachine.SelectedValue);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                Clean();
                TableRead();
            }
        }

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            if(dgBrands.SelectedItem is DataRowView row)
            {
                using(SQLiteConnection conn = new SQLiteConnection(connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE PROFİLES_BRANDS SET " +
                        "name = @name," +
                        "description = @description," +
                        "machineID = @machine_ID " +
                        "WHERE ProfileID = @ProfileID";

                    SQLiteCommand cmd = new SQLiteCommand(updateQuery);

                    cmd.Parameters.AddWithValue("@name", txtBrandName.Text);
                    cmd.Parameters.AddWithValue("@description", txtBrandDesc.Text);
                    cmd.Parameters.AddWithValue("@machineID", Convert.ToInt32(cmbMachine.SelectedValue));
                    cmd.Parameters.AddWithValue("@ProfileID", row["ProfileID"]);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.");
                    Clean();
                    TableRead();
                }
            }
        }

        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Silmek istediğine emin misin?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
            if (dgBrands.SelectedItem is DataRowView row)
            {
                using(SQLiteConnection conn = new SQLiteConnection(connStr))
                {
                    conn.Open();
                    string deleteQuery = "DELETE FROM PROFILES_BRANDS " +
                        "WHERE ProfileID = @ProfileID";

                    SQLiteCommand cmd = new SQLiteCommand(deleteQuery, conn);

                    cmd.Parameters.AddWithValue("@ProfileID", row["ProfileID"]);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Seçilen marka silindi.");
                    Clean();
                    TableRead();
                }
            }

            }
        }

        private void Clean()
        {
            txtBrandName.Text = "";
            txtBrandDesc.Text = "";
            cmbMachine.SelectedIndex = -1;
        }
    }
}

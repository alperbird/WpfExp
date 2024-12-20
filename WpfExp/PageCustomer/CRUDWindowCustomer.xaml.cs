using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfExp.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using WpfExp.PageCustomer;
using System.Data.SQLite;

namespace WpfExp
{
    /// <summary>
    /// Interaction logic for AddCustomerWindow.xaml
    /// </summary>
    public partial class CRUDWindowCustomer : Window
    {
        private DataRowView row; // güncelleme için seçiyoruz
        private string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        public CRUDWindowCustomer()
        {
            InitializeComponent();
            //datagridi doldurma
            TableRead();

            //comboboxları dolduruyor bu da
            ComboBoxFull();

        }
        
        
        public void TableRead()
        {

            try
            {
                using(SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT cu.Id, cu.name AS 'Müşteri Adı', cu.description AS 'Açıklama', co.name AS 'Ülke', ci.name AS 'Şehir', " +
                        "ma.name 'Makine', pb.name AS 'Marka', ps.name AS 'Profil Serisi', pe.name AS 'Profil Örnek', snm.serialNum AS 'Seri Numarası' " +
                        "FROM CUSTOMER cu JOIN COUNTRIES co ON cu.countryId = co.Id " +
                        "JOIN CITIES ci ON cu.cityId = ci.Id " +
                        "JOIN MACHINES ma ON cu.machineId = ma.Id " +
                        "JOIN PROFILE_BRANDS pb ON cu.pbrandId = pb.Id " +
                        "JOIN PROFILE_SERIES ps ON cu.pserieId = ps.Id " +
                        "JOIN PROFILE_EXAMPLE pe ON cu.pexmId = pe.Id " +
                        "JOIN SERIAL_NUMBERS snm ON cu.serialnumId = snm.Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn)) 
                    {
                        SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        dgCustomers.ItemsSource = dt.DefaultView;
                    }
                }  
            }
            catch (Exception ex)
            {
                MessageBox.Show("HATA!\n " +ex.Message);
            }
        }

        private void ComboBoxFull() //combobox doldurma
        {
            
            using (SqlConnection conn = new SqlConnection(connStr))
            {

                conn.Open();
                //int customerId = Convert.ToInt32(row["CustomerID"]);

                string queryCountry = "SELECT Id,name FROM COUNTRIES";
                SqlCommand cocommand = new SqlCommand(queryCountry, conn);
                SqlDataAdapter daCountry = new SqlDataAdapter(cocommand);
                DataTable dtCountry = new DataTable();
                daCountry.Fill(dtCountry);
                //---------------------------------------------------------------
                cmbCountry.ItemsSource = dtCountry.DefaultView;
                cmbCountry.DisplayMemberPath = "name";
                cmbCountry.SelectedValuePath = "Id";
                
                cmbCountry.UpdateLayout();

                string queryCity = "SELECT Id, name FROM CITIES";
                SqlCommand cicommdan = new SqlCommand(queryCity, conn);
                SqlDataAdapter daCity = new SqlDataAdapter(cicommdan);
                DataTable dtCity = new DataTable();
                daCity.Fill(dtCity);
                //---------------------------------------------------------------
                cmbCity.ItemsSource = dtCity.DefaultView;
                cmbCity.DisplayMemberPath = "name";
                cmbCity.SelectedValuePath = "Id";
                cmbCity.UpdateLayout();

                //combobox doğru veriyi gösteriyor mu doğrulaması

                //---------------------------------------------------------------

                string queryMachine = "SELECT Id, name FROM MACHINES";
                SqlCommand macommand = new SqlCommand(queryMachine, conn);
                SqlDataAdapter daMachine = new SqlDataAdapter(macommand);
                DataTable dtMachine = new DataTable();
                daMachine.Fill(dtMachine);
                //---------------------------------------------------------------
                lstMachine.ItemsSource = dtMachine.DefaultView;
                lstMachine.DisplayMemberPath = "name";
                lstMachine.SelectedValuePath = "Id";
                lstMachine.UpdateLayout();



                string queryBrand = "SELECT Id, name FROM PROFILE_BRANDS";
                SqlCommand brcommand = new SqlCommand(queryBrand, conn);
                SqlDataAdapter daBrand = new SqlDataAdapter(brcommand);
                DataTable dtBrand = new DataTable();
                daBrand.Fill(dtBrand);
                //---------------------------------------------------------------
                cmbProfileBrand.ItemsSource = dtBrand.DefaultView;
                cmbProfileBrand.DisplayMemberPath = "name";
                cmbProfileBrand.SelectedValuePath = "Id";
                cmbProfileBrand.UpdateLayout();

                string querySeries = "SELECT Id, name FROM PROFILE_SERIES";
                SqlCommand secommand = new SqlCommand(querySeries, conn);
                SqlDataAdapter daSeries = new SqlDataAdapter(secommand);
                DataTable dtSeries = new DataTable();
                daSeries.Fill(dtSeries);
                //---------------------------------------------------------------
                cmbProfileSeries.ItemsSource = dtSeries.DefaultView;
                cmbProfileSeries.DisplayMemberPath = "name";
                cmbProfileSeries.SelectedValuePath = "Id";
                cmbProfileSeries.UpdateLayout();

                conn.Close();
            }
        }

        private void insertButton(object sender, RoutedEventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO CUSTOMER (name, description, countryId, cityId, machineId, pbrandId, pserieId, pexmId, serialnumId) " +
                    "VALUES(@name, @description, @countryId, @cityId, @machineId, @pbrandId, @pserieId, @pexmId, @serialnumId)";
                SqlCommand cmd = new SqlCommand(insertQuery, conn);

                cmd.Parameters.AddWithValue("@name", txtName.Text);
                cmd.Parameters.AddWithValue("@description", txtDesc.Text);
                cmd.Parameters.AddWithValue("@countryId", cmbCountry.SelectedValue);
                cmd.Parameters.AddWithValue("@cityId", cmbCity.SelectedValue);
                cmd.Parameters.AddWithValue("@machineId", lstMachine.SelectedValue);
                cmd.Parameters.AddWithValue("@pbrandId", cmbProfileBrand.SelectedValue);
                cmd.Parameters.AddWithValue("@pserieId", cmbProfileSeries.SelectedValue);
                cmd.Parameters.AddWithValue("@pexmId", txtProfileExample.Text);
                cmd.Parameters.AddWithValue("@serialnumId", txtSerieNumber.Text);


                int customerId = Convert.ToInt32(cmd.ExecuteScalar()); // Müşteri ID'sini al

                // Müşteri için makine ve seri numarası ekleme
                foreach (var selectedMachine in lstMachine.SelectedItems) // lstMachines: Çoklu seçim için kullanılan ListBox
                {
                    // Seçilen makinenin bilgilerini al
                    int machineId = Convert.ToInt32(((DataRowView)selectedMachine)["Id"]); // Makine ID'si
                    int serialNumId = MachineSerialNum(machineId); // Makineye ait seri numarasını al

                    // Seri numarası ekle
                    string insertMachineQuery = "INSERT INTO SERIAL_NUMBERS (customerId, machineId, serialNum) " +
                                                "VALUES(@customerId, @machineId, @serialNum)";
                    SqlCommand cmdMachine = new SqlCommand(insertMachineQuery, conn);
                    cmdMachine.Parameters.AddWithValue("@customerId", customerId);
                    cmdMachine.Parameters.AddWithValue("@machineId", machineId);
                    cmdMachine.Parameters.AddWithValue("@serialNum", serialNumId);
                    cmdMachine.ExecuteNonQuery();
                }


                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                Clean();

                TableRead();
            }
        }

        private int MachineSerialNum(int machineId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string querySerialNum = "SELECT TOP 1 Id FROM SERIAL_NUMBERS WHERE MachineId = @machineId";
                SqlCommand cmd = new SqlCommand(querySerialNum, conn);
                cmd.Parameters.AddWithValue("@machineId", machineId);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void dgCustomer(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgCustomers.SelectedItem is DataRowView selectedRow)
                {
                    txtName.Text = selectedRow["Müşteri Adı"].ToString();
                    txtDesc.Text = selectedRow["Açıklama"].ToString();
                    txtProfileExample.Text = selectedRow["Profil Örnek"].ToString();
                    txtSerieNumber.Text = selectedRow["Seri Numarası"].ToString();

                    var selectedCountry = cmbCountry.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"]?.ToString() == selectedRow["Ülke"]?.ToString());
                    cmbCountry.SelectedItem = selectedCountry;
                    //------------------------------------------------------------------------------
                    var selectedCity = cmbCity.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == selectedRow["Şehir"].ToString());
                    cmbCity.SelectedItem = selectedCity;

                    var selectedMachine = lstMachine.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == selectedRow["Makine"].ToString());
                    lstMachine.SelectedItem = selectedMachine;

                    var selectedBrand = cmbProfileBrand.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == selectedRow["Marka"].ToString());
                    cmbProfileBrand.SelectedItem = selectedBrand;

                    var selectedSeries = cmbProfileSeries.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == selectedRow["Profil Serisi"].ToString());
                    cmbProfileSeries.SelectedItem = selectedSeries;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("HATA!\n"+ex.Message);
            }
            
        }


        private void updateBtn(object sender, RoutedEventArgs e)
        {
            if (dgCustomers.SelectedItem is DataRowView selectedRow)
            {
                using (SqlConnection connection = new SqlConnection(connStr))
                {
                    connection.Open();

                    string updateQuery = "UPDATE CUSTOMER SET name = @name, description = @description, " +
                                         "countryId = @countryId, cityId= @cityId, machineId = @machineId, " +
                                         "pbrandId = @pbrandId, pserieId = @pserieId, " +
                                         "pexmId = @pexmId, " +
                                         "serialnumId = @serialnumId " +
                                         "WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(updateQuery, connection);

                    cmd.Parameters.AddWithValue("@name", txtName.Text);
                    cmd.Parameters.AddWithValue("@description", txtDesc.Text);
                    cmd.Parameters.AddWithValue("@countryID", Convert.ToInt32(cmbCountry.SelectedValue));
                    cmd.Parameters.AddWithValue("@CityID", Convert.ToInt32(cmbCity.SelectedValue));
                    cmd.Parameters.AddWithValue("@machineID", Convert.ToInt32(lstMachine.SelectedValue));
                    cmd.Parameters.AddWithValue("@pbrandID", Convert.ToInt32(cmbProfileBrand.SelectedValue));
                    cmd.Parameters.AddWithValue("@pseriesID", Convert.ToInt32(cmbProfileSeries.SelectedValue));
                    cmd.Parameters.AddWithValue("@pExmID", txtProfileExample.Text);
                    cmd.Parameters.AddWithValue("@serialnumId", txtSerieNumber.Text);
                    cmd.Parameters.AddWithValue("@Id", selectedRow["Id"]);

                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.");
                Clean();

                TableRead(); // DataGrid'i yenile
            }
        }

        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Silmek istediğine emin misin?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (dgCustomers.SelectedItem is DataRowView selectedRow)
                {
                    using (SqlConnection connection = new SqlConnection(connStr))
                    {
                        connection.Open();

                        string deleteQuery = "DELETE FROM CUSTOMER WHERE Id = @Id";
                        SqlCommand cmd = new SqlCommand(deleteQuery, connection);
                        cmd.Parameters.AddWithValue("@Id", selectedRow["Id"]);
                        cmd.ExecuteNonQuery();

                    }
                    MessageBox.Show("Seçilen müşteri silindi.");
                    Clean();
                    TableRead(); // DataGrid'i yenile
                }
            }
        }

        private void Clean() //insert ve update işlemi yaptıktan sonra textbox ve combobox temizliğini yapacak.
        {
            txtName.Text = "";
            txtDesc.Text = "";
            txtProfileExample.Text = "";
            txtSerieNumber.Text = "";
            cmbCountry.SelectedIndex = -1;
            cmbCity.SelectedIndex = -1;
            lstMachine.SelectedIndex = -1;
            cmbProfileBrand.SelectedIndex = -1;
            cmbProfileSeries.SelectedIndex = -1;
        }

        private void cmbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e) // ülke seçimine göre gelen şehir combobox
        {
            if (cmbCountry.SelectedValue != null)
            {
                int countryId = (int)cmbCountry.SelectedValue;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string queryCity = "SELECT Id, name FROM CITIES WHERE countryId = @countryId";
                    SqlCommand cmd = new SqlCommand(queryCity, conn);
                    cmd.Parameters.AddWithValue("@countryId", countryId);

                    SqlDataAdapter daCity = new SqlDataAdapter(cmd);
                    DataTable dtCity = new DataTable();
                    daCity.Fill(dtCity);

                    cmbCity.ItemsSource = dtCity.DefaultView;
                    cmbCity.DisplayMemberPath = "name";
                    cmbCity.SelectedValuePath = "Id";
                    conn.Close();
                }
            }
        }

        private void cmbProfileBrand_SelectionChanged(object sender, SelectionChangedEventArgs e) // marka seçimine göre gelen profil serisi combobox
        {
            if (cmbProfileBrand.SelectedValue != null)
            {
                int brandId = (int)cmbProfileBrand.SelectedValue;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string querySeries = "SELECT Id, name FROM PROFILE_SERIES WHERE brandId = @brandId OR brandId =  @brandId";
                    SqlCommand cmd = new SqlCommand(querySeries, conn);
                    cmd.Parameters.AddWithValue("@brandId", brandId);

                    SqlDataAdapter daSeries = new SqlDataAdapter(cmd);
                    DataTable dtSeries = new DataTable();
                    daSeries.Fill(dtSeries);

                    cmbProfileSeries.ItemsSource = dtSeries.DefaultView;
                    cmbProfileSeries.DisplayMemberPath = "name";
                    cmbProfileSeries.SelectedValuePath = "Id";
                    conn.Close();
                }
            }
        }
    }

}

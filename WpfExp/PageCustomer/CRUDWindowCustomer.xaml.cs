using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
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

namespace WpfExp
{
    /// <summary>
    /// Interaction logic for AddCustomerWindow.xaml
    /// </summary>
    public partial class CRUDWindowCustomer : Window
    {
        private DataRowView row; // güncelleme için seçiyoruz
        private string connStr = "Data Source= Turbocut-400-500.db; Version=3;";
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
                using(SQLiteConnection conn = new SQLiteConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT "+
                            " C.CustomerID,"+
                            " C.name AS 'Müşteri Adı',"+
                            " C.description AS 'Açıklama',"+
                            " Co.name AS 'Ülke',"+
                            " Ci.name AS 'Şehir',"+
                            " M.name AS 'Makine',"+
                            " Pb.name AS 'Marka',"+
                            " Ps.name AS 'Profil Serisi',"+
                            " Pt.name AS 'Profil Örnek' "+
                                " FROM CUSTOMER C "+
                                    "LEFT JOIN COUNTRIES Co ON C.countryID = Co.CountriesID "+
                                    "LEFT JOIN CITIES Ci ON C.CityID = Ci.CitiesID "+
                                    "LEFT JOIN MACHINES M ON C.machineID = M.machID "+
                                    "LEFT JOIN PROFILES_BRANDS Pb ON C.pbrandID = Pb.ProfileID "+
                                    "LEFT JOIN PROFILE_SERIES Ps ON C.pseriesID = Ps.SeriesID "+
                                    "LEFT JOIN PROFILE_TBL Pt ON C.pExmID = Pt.ID ";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn)) 
                    {
                        SQLiteDataAdapter adapt = new SQLiteDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        dgCustomers.ItemsSource = dt.DefaultView;
                    }
                }  
            }
            catch (Exception ex)
            {
                MessageBox.Show("GÖKTE NE VAR?\n " +ex.Message);
            }
        }

        private void ComboBoxFull() //combobox doldurma
        {
            
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {

                conn.Open();
                //int customerId = Convert.ToInt32(row["CustomerID"]);

                string queryCountry = "SELECT CountriesID,name FROM COUNTRIES";
                SQLiteCommand cocommand = new SQLiteCommand(queryCountry, conn);
                SQLiteDataAdapter daCountry = new SQLiteDataAdapter(cocommand);
                DataTable dtCountry = new DataTable();
                daCountry.Fill(dtCountry);
                //---------------------------------------------------------------
                cmbCountry.ItemsSource = dtCountry.DefaultView;
                cmbCountry.DisplayMemberPath = "name";
                cmbCountry.SelectedValuePath = "CountriesID";
                
                cmbCountry.UpdateLayout();

                //combobox doğru veriyi gösteriyor mu doğrulaması



                string queryCity = "SELECT CitiesID, name FROM CITIES";
                SQLiteCommand cicommdan = new SQLiteCommand(queryCity, conn);
                SQLiteDataAdapter daCity = new SQLiteDataAdapter(cicommdan);
                DataTable dtCity = new DataTable();
                daCity.Fill(dtCity);
                //---------------------------------------------------------------
                cmbCity.ItemsSource = dtCity.DefaultView;
                cmbCity.DisplayMemberPath = "name";
                cmbCity.SelectedValuePath = "CitiesID";
                cmbCity.UpdateLayout();

                string queryMachine = "SELECT machID, name FROM MACHINES";
                SQLiteCommand macommand = new SQLiteCommand(queryMachine, conn);
                SQLiteDataAdapter daMachine = new SQLiteDataAdapter(macommand);
                DataTable dtMachine = new DataTable();
                daMachine.Fill(dtMachine);
                //---------------------------------------------------------------
                cmbMachine.ItemsSource = dtMachine.DefaultView;
                cmbMachine.DisplayMemberPath = "name";
                cmbMachine.SelectedValuePath = "machID";
                cmbMachine.UpdateLayout();

                string queryBrand = "SELECT ProfileID, name FROM PROFILES_BRANDS";
                SQLiteCommand brcommand = new SQLiteCommand(queryBrand, conn);
                SQLiteDataAdapter daBrand = new SQLiteDataAdapter(brcommand);
                DataTable dtBrand = new DataTable();
                daBrand.Fill(dtBrand);
                //---------------------------------------------------------------
                cmbProfileBrand.ItemsSource = dtBrand.DefaultView;
                cmbProfileBrand.DisplayMemberPath = "name";
                cmbProfileBrand.SelectedValuePath = "ProfileID";
                cmbProfileBrand.UpdateLayout();

                string querySeries = "SELECT SeriesID, name FROM PROFILE_SERIES";
                SQLiteCommand secommand = new SQLiteCommand(querySeries, conn);
                SQLiteDataAdapter daSeries = new SQLiteDataAdapter(secommand);
                DataTable dtSeries = new DataTable();
                daSeries.Fill(dtSeries);
                //---------------------------------------------------------------
                cmbProfileSeries.ItemsSource = dtSeries.DefaultView;
                cmbProfileSeries.DisplayMemberPath = "name";
                cmbProfileSeries.SelectedValuePath = "SeriesID";
                cmbProfileSeries.UpdateLayout();
                conn.Close();
            }
        }

        private void insertButton(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO CUSTOMER (name, description, countryID, CityID, machineID, pbrandID, pseriesID, pExmID) " +
                    "VALUES(@name, @description, @countryID, @CityID, @machineID, @pbrandID, @pseriesID, @pExmID)";
                SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn);

                cmd.Parameters.AddWithValue("@name", txtName.Text);
                cmd.Parameters.AddWithValue("@description", txtDesc.Text);
                cmd.Parameters.AddWithValue("@countryID", cmbCountry.SelectedValue);
                cmd.Parameters.AddWithValue("@CityID", cmbCity.SelectedValue);
                cmd.Parameters.AddWithValue("@machineID", cmbMachine.SelectedValue);
                cmd.Parameters.AddWithValue("@pbrandID", cmbProfileBrand.SelectedValue);
                cmd.Parameters.AddWithValue("@pseriesID", cmbProfileSeries.SelectedValue);
                cmd.Parameters.AddWithValue("@pExmID", txtProfileExample.Text);
                
                cmd.ExecuteNonQuery();

                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                Clean();

                TableRead();
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

                    var selectedCountry = cmbCountry.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"]?.ToString() == selectedRow["Ülke"]?.ToString());
                    cmbCountry.SelectedItem = selectedCountry;
                    //------------------------------------------------------------------------------
                    var selectedCity = cmbCity.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == selectedRow["Şehir"].ToString());
                    cmbCity.SelectedItem = selectedCity;

                    var selectedMachine = cmbMachine.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == selectedRow["Makine"].ToString());
                    cmbMachine.SelectedItem = selectedMachine;

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
                using (SQLiteConnection connection = new SQLiteConnection(connStr))
                {
                    connection.Open();

                    string updateQuery = "UPDATE CUSTOMER SET name = @name, description = @description, " +
                                         "countryID = @countryID, CityID = @CityID, machineID = @machineID, " +
                                         "pbrandID = @pbrandID, pseriesID = @pseriesID, " +
                                         "pExmID = @pExmID " +
                                         "WHERE CustomerID = @CustomerID";
                    SQLiteCommand cmd = new SQLiteCommand(updateQuery, connection);

                    cmd.Parameters.AddWithValue("@name", txtName.Text);
                    cmd.Parameters.AddWithValue("@description", txtDesc.Text);
                    cmd.Parameters.AddWithValue("@countryID", Convert.ToInt32(cmbCountry.SelectedValue));
                    cmd.Parameters.AddWithValue("@CityID", Convert.ToInt32(cmbCity.SelectedValue));
                    cmd.Parameters.AddWithValue("@machineID", Convert.ToInt32(cmbMachine.SelectedValue));
                    cmd.Parameters.AddWithValue("@pbrandID", Convert.ToInt32(cmbProfileBrand.SelectedValue));
                    cmd.Parameters.AddWithValue("@pseriesID", Convert.ToInt32(cmbProfileSeries.SelectedValue));
                    cmd.Parameters.AddWithValue("@pExmID", txtProfileExample.Text);
                    cmd.Parameters.AddWithValue("@CustomerID", selectedRow["CustomerID"]);

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
                    using (SQLiteConnection connection = new SQLiteConnection(connStr))
                    {
                        connection.Open();

                        string deleteQuery = "DELETE FROM CUSTOMER WHERE CustomerID = @CustomerID";
                        SQLiteCommand cmd = new SQLiteCommand(deleteQuery, connection);
                        cmd.Parameters.AddWithValue("@CustomerID", selectedRow["CustomerID"]);
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
            cmbCountry.SelectedIndex = -1;
            cmbCity.SelectedIndex = -1;
            cmbMachine.SelectedIndex = -1;
            cmbProfileBrand.SelectedIndex = -1;
            cmbProfileSeries.SelectedIndex = -1;
            
        }
    }

}

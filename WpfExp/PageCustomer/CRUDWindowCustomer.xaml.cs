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

using System.Data.SQLite;
using WpfExp.LogRecords;
using System.Configuration;

namespace WpfExp
{
    /// <summary>
    /// Interaction logic for AddCustomerWindow.xaml
    /// </summary>
    public partial class CRUDWindowCustomer : Window
    {
        private DataRowView row; // güncelleme için seçiyoruz
        private string connStr;
        public CRUDWindowCustomer()
        {
            InitializeComponent();
            connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //datagridi doldurma
            TableRead();

            //comboboxları dolduruyor bu da
            ComboBoxFull();

        }


        public void TableRead()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"
                                    SELECT 
                                    cu.Id AS 'Müşteri ID',
                                    cu.name AS 'Müşteri Adı',
                                    cu.description AS 'Açıklama',
                                    co.name AS 'Ülke',
                                    ci.name AS 'Şehir',
                                    -- Makineler
                                    (
                                        SELECT STRING_AGG(ma.name, ', ') 
                                        FROM CUSTOMER_MACHINE cm
                                        JOIN MACHINES ma ON cm.machineId = ma.Id
                                        WHERE cm.customerId = cu.Id
                                    ) AS 'Makineler',
                                    -- Markalar
                                    (
                                        SELECT STRING_AGG(pb.name, ', ') 
                                        FROM CUSTOMER_BRANDS cb
                                        JOIN PROFILE_BRANDS pb ON cb.brandId = pb.Id
                                        WHERE cb.customerId = cu.Id
                                    ) AS 'Markalar',
                                    -- Profil Serileri
                                    (
                                        SELECT STRING_AGG(ps.name, ', ') 
                                        FROM CUSTOMER_PROFILE_SERIES cps
                                        JOIN PROFILE_SERIES ps ON cps.seriesId = ps.Id
                                        WHERE cps.customerId = cu.Id
                                    ) AS 'Profil Serileri',
                                    -- Profil Örnekleri
                                    (
                                        SELECT STRING_AGG(pe.name, ', ') 
                                        FROM CUSTOMER_PROFILE_EXAMPLES cpe
                                        JOIN PROFILE_EXAMPLE pe ON cpe.ProfileExampleId = pe.Id
                                        WHERE cpe.CustomerId = cu.Id
                                    ) AS 'Profil Örnekleri',
                                    -- Seri Numaraları
                                    (
                                        SELECT STRING_AGG(snm.serialNum, ', ') 
                                        FROM SERIAL_NUMBERS snm
                                        WHERE snm.CustomerId = cu.Id
                                    ) AS 'Seri Numaraları'
                                    FROM CUSTOMER cu
                                    LEFT JOIN COUNTRIES co ON cu.countryId = co.Id
                                    LEFT JOIN CITIES ci ON cu.cityId = ci.Id;
";

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
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void LoadCustomerProfileSeries(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = @"
            SELECT ps.Id, ps.name
            FROM CUSTOMER_PROFILE_SERIES cps
            JOIN PROFILE_SERIES ps ON cps.seriesId = ps.Id
            WHERE cps.customerId = @customerId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@customerId", customerId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                lstpSeries.ItemsSource = dt.DefaultView;
                lstpSeries.DisplayMemberPath = "name";
                lstpSeries.SelectedValuePath = "Id";
            }
        }

        #region Customer Combobox
        private void ComboBoxFull() //combobox doldurma
        {
            var queriesAndControls = new Dictionary<string, dynamic>
            {
                { "SELECT Id, name FROM COUNTRIES", cmbCountry },
                { "SELECT Id, name FROM CITIES", cmbCity },
                { "SELECT Id, name FROM MACHINES", lstMachine },
                { "SELECT Id, name FROM PROFILE_BRANDS", lstpBrand },
                {"SELECT Id, name FROM PROFILE_EXAMPLE",lstpexm }
                // lstpSeries başlangıçta burada sorgu ile doldurulmaz
            };

            foreach (var queryAndControl in queriesAndControls)
            {
                FillControl(queryAndControl.Key, queryAndControl.Value);
            }

            // Şehir ComboBox'u başlangıçta boş
            cmbCity.ItemsSource = null;
            cmbCity.DisplayMemberPath = "name";
            cmbCity.SelectedValuePath = "Id";
            cmbCity.SelectedIndex = -1;

            // lstpSeries başlangıçta boş olmalı
            lstpSeries.ItemsSource = null;
            lstpSeries.DisplayMemberPath = "name";
            lstpSeries.SelectedValuePath = "Id";

            lstpexm.DisplayMemberPath= "name";
            lstpexm.SelectedValuePath = "Id";
        }


        private void FillControl(string query, dynamic control)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(new SqlCommand(query, conn));
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    control.ItemsSource = dt.DefaultView;
                    control.DisplayMemberPath = "name";
                    control.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Doldurmada HATA! {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Customer Insert
        private void insertBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // 1. CUSTOMER tablosuna ekleme
                    string insertCustomerQuery = @"
                INSERT INTO CUSTOMER (name, description, countryId, cityId) 
                VALUES (@name, @description, @countryId, @cityId);
                SELECT SCOPE_IDENTITY();"; // Eklenen müşterinin ID'sini alır

                    SqlCommand cmd = new SqlCommand(insertCustomerQuery, conn);
                    cmd.Parameters.AddWithValue("@name", txtName.Text);
                    cmd.Parameters.AddWithValue("@description", txtDesc.Text);
                    cmd.Parameters.AddWithValue("@countryId", cmbCountry.SelectedValue);
                    cmd.Parameters.AddWithValue("@cityId", cmbCity.SelectedValue);

                    int customerId = Convert.ToInt32(cmd.ExecuteScalar()); // Eklenen müşterinin ID'sini al

                    // 2. Ara tablolara ekleme
                    InsertCustomerMachines(customerId, conn);
                    InsertCustomerBrands(customerId);
                    InsertCustomerProfileSeries(customerId);
                    InsertCustomerProfileExamples(customerId);
                    InsertCustomerSerialNumbers(customerId);

                    MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    LogUserProcess.AddLog(currentUserId, $"Müşteri Kaydı Eklendi: {txtName.Text}");
                    Clean();

                    TableRead();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eklemede HATA: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InsertCustomerMachines(int customerId, SqlConnection conn)
        {
            foreach (var selectedMachine in lstMachine.SelectedItems)
            {
                int machineId = Convert.ToInt32(((DataRowView)selectedMachine)["Id"]);

                string insertQuery = @"
            INSERT INTO CUSTOMER_MACHINE (customerId, machineId) 
            VALUES (@customerId, @machineId);";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@customerId", customerId);
                cmd.Parameters.AddWithValue("@machineId", machineId);
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertCustomerBrands(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                foreach (var selectedBrand in lstpBrand.SelectedItems)
                {
                    var brandRow = (DataRowView)selectedBrand;
                    int brandId = Convert.ToInt32(brandRow["Id"]);

                    string query = @"
                INSERT INTO CUSTOMER_BRANDS (customerId, brandId) 
                VALUES (@customerId, @brandId)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@customerId", customerId);
                    cmd.Parameters.AddWithValue("@brandId", brandId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void InsertCustomerProfileSeries(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                foreach (var selectedSeries in lstpSeries.SelectedItems)
                {
                    var seriesRow = (DataRowView)selectedSeries;
                    int seriesId = Convert.ToInt32(seriesRow["Id"]);
                    int brandId = GetBrandIdBySeries(seriesId, conn); // Series'e bağlı brandId'yi bul

                    // CUSTOMER_PROFILE_SERIES tablosuna ekle
                    string insertCustomerSeriesQuery = @"
                IF NOT EXISTS (
                    SELECT 1 FROM CUSTOMER_PROFILE_SERIES 
                    WHERE customerId = @customerId AND seriesId = @seriesId
                )
                INSERT INTO CUSTOMER_PROFILE_SERIES (customerId, seriesId)
                VALUES (@customerId, @seriesId)";
                    SqlCommand insertCustomerSeriesCmd = new SqlCommand(insertCustomerSeriesQuery, conn);
                    insertCustomerSeriesCmd.Parameters.AddWithValue("@customerId", customerId);
                    insertCustomerSeriesCmd.Parameters.AddWithValue("@seriesId", seriesId);
                    insertCustomerSeriesCmd.ExecuteNonQuery();

                    // BRAND_SERIES tablosuna ekle
                    string insertBrandSeriesQuery = @"
                IF NOT EXISTS (
                    SELECT 1 FROM BRAND_SERIES 
                    WHERE customerId = @customerId AND brandId = @brandId AND seriesId = @seriesId
                )
                INSERT INTO BRAND_SERIES (customerId, brandId, seriesId)
                VALUES (@customerId, @brandId, @seriesId)";
                    SqlCommand insertBrandSeriesCmd = new SqlCommand(insertBrandSeriesQuery, conn);
                    insertBrandSeriesCmd.Parameters.AddWithValue("@customerId", customerId);
                    insertBrandSeriesCmd.Parameters.AddWithValue("@brandId", brandId);
                    insertBrandSeriesCmd.Parameters.AddWithValue("@seriesId", seriesId);
                    insertBrandSeriesCmd.ExecuteNonQuery();
                }
            }
        }

        // BRAND_SERIES için brandId bulma metodu
        private int GetBrandIdBySeries(int seriesId, SqlConnection conn)
        {
            string query = "SELECT brandId FROM PROFILE_SERIES WHERE Id = @seriesId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@seriesId", seriesId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private void InsertCustomerSerialNumbers(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                foreach (var selectedMachine in lstMachine.SelectedItems)
                {
                    var machineRow = (DataRowView)selectedMachine;
                    int machineId = Convert.ToInt32(machineRow["Id"]);
                    string serialNumber = txtSerieNumber.Text; // Kullanıcıdan alınan seri numarası

                    string query = @"
            INSERT INTO SERIAL_NUMBERS (machineId, customerId, serialNum) 
            VALUES (@machineId, @customerId, @serialNum)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@machineId", machineId);
                    cmd.Parameters.AddWithValue("@customerId", customerId);
                    cmd.Parameters.AddWithValue("@serialNum", serialNumber);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void InsertCustomerProfileExamples(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                foreach (var selectedProfileExample in lstpexm.SelectedItems)
                {
                    var profileExampleRow = (DataRowView)selectedProfileExample;
                    int profileExampleId = Convert.ToInt32(profileExampleRow["Id"]);

                    string query = @"
                INSERT INTO CUSTOMER_PROFILE_EXAMPLES (customerId, profileExampleId) 
                VALUES (@customerId, @profileExampleId)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@customerId", customerId);
                    cmd.Parameters.AddWithValue("@profileExampleId", profileExampleId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion



        private void dgCustomer(object sender, SelectionChangedEventArgs e)
        {
            if (dgCustomers.SelectedItem is DataRowView selectedRow)
            {
                txtName.Text = selectedRow["Müşteri Adı"].ToString();
                txtDesc.Text = selectedRow["Açıklama"].ToString();

                // Ülke ve Şehir
                cmbCountry.SelectedItem = cmbCountry.Items
                    .Cast<DataRowView>()
                    .FirstOrDefault(item => item["name"].ToString() == selectedRow["Ülke"].ToString());
                cmbCity.SelectedItem = cmbCity.Items
                    .Cast<DataRowView>()
                    .FirstOrDefault(item => item["name"].ToString() == selectedRow["Şehir"].ToString());

                // Makineler 
                var selectedMachine = selectedRow["Makineler"].ToString().Split(',').FirstOrDefault();
                if (selectedMachine != null)
                {
                    var machine = lstMachine.Items.Cast<DataRowView>()
                                   .FirstOrDefault(m => m["name"].ToString() == selectedMachine);

                    if (machine != null)
                    {
                        lstMachine.SelectedItem = machine; // Tek seçim modu için
                    }
                }


                // Markalar
                var selectedBrands = selectedRow["Markalar"].ToString().Split(',').FirstOrDefault();
                if (selectedBrands != null)
                {
                    var brand = lstpBrand.Items.Cast<DataRowView>()
                                   .FirstOrDefault(b => b["name"].ToString() == selectedBrands);
                    if (brand != null)
                    {
                        lstpBrand.SelectedItem = brand; // Tek seçim modu için
                    }


                    // Profil Serileri
                    var selectedSeries = selectedRow["Profil Serileri"].ToString().Split(',').FirstOrDefault();
                    if (selectedSeries != null)
                    {
                        var series = lstpSeries.Items.Cast<DataRowView>()
                                       .FirstOrDefault(s => s["name"].ToString() == selectedSeries);
                        if (series != null)
                        {
                            lstpSeries.SelectedItem = series; // Tek seçim modu için
                        }

                        // Profil Örnekleri
                        var selectedExamples = selectedRow["Profil Örnekleri"].ToString().Split(',').FirstOrDefault();
                        if (selectedExamples != null)
                        {
                            var example = lstpexm.Items.Cast<DataRowView>()
                                           .FirstOrDefault(ex => ex["name"].ToString() == selectedExamples);
                            if (example != null)
                            {
                                lstpexm.SelectedItem = example; // Tek seçim modu için
                            }
                        }
                        // Seri Numaraları
                        txtSerieNumber.Text = selectedRow["Seri Numaraları"].ToString();
                    }

                }
            }
        }

        #region Customer Update
        private void updateBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;

            if (dgCustomers.SelectedItem is DataRowView row)
            {
                // Eski değerleri tutacak değişkenler
                string oldName = "", oldDesc = "";
                int oldCountryId = 0, oldCityId = 0;
                string oldMachines = "", oldBrands = "", oldProfileSeries = "", oldExamples = "", oldSerialNumbers = "";

                // Eski değerleri almak için sorgular
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Müşteri bilgileri
                    string selectedQuery = @"
                SELECT name, description, countryId, cityId 
                FROM CUSTOMER  
                WHERE Id = @Id";
                    SqlCommand selectCmd = new SqlCommand(selectedQuery, conn);
                    selectCmd.Parameters.AddWithValue("@Id", row["Müşteri ID"]);

                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldName = reader["name"].ToString();
                            oldDesc = reader["description"].ToString();
                            oldCountryId = Convert.ToInt32(reader["countryId"]);
                            oldCityId = Convert.ToInt32(reader["cityId"]);
                        }
                    }

                    // Eski makineler
                    oldMachines = GetRelatedData(conn, row["Müşteri ID"], "CUSTOMER_MACHINE", "MACHINES", "machineId", "name");

                    // Eski markalar
                    oldBrands = GetRelatedData(conn, row["Müşteri ID"], "CUSTOMER_BRANDS", "PROFILE_BRANDS", "brandId", "name");

                    // Eski profil serileri
                    oldProfileSeries = GetRelatedData(conn, row["Müşteri ID"], "CUSTOMER_PROFILE_SERIES", "PROFILE_SERIES", "seriesId", "name");

                    // Eski profil örnekleri
                    oldExamples = GetRelatedData(conn, row["Müşteri ID"], "CUSTOMER_PROFILE_EXAMPLES", "PROFILE_EXAMPLE", "profileExampleId", "name");

                    // Eski seri numaraları
                    oldSerialNumbers = GetSerialNumbers(conn, row["Müşteri ID"]);
                }

                // Yeni değerler
                string newName = txtName.Text;
                string newDesc = txtDesc.Text;
                int newCountryId = Convert.ToInt32(cmbCountry.SelectedValue);
                int newCityId = Convert.ToInt32(cmbCity.SelectedValue);

                // Yeni makineler, markalar, profil serileri ve profil örnekleri için UI'den değerleri topla
                string newMachines = string.Join(", ", lstMachine.SelectedItems.Cast<DataRowView>().Select(x => x["name"]));
                string newBrands = string.Join(", ", lstpBrand.SelectedItems.Cast<DataRowView>().Select(x => x["name"]));
                string newProfileSeries = string.Join(", ", lstpSeries.SelectedItems.Cast<DataRowView>().Select(x => x["name"]));
                string newExamples = string.Join(",", lstpexm.SelectedItems.Cast<DataRowView>().Select(x => x["name"]));
                string newSerialNumbers = txtSerieNumber.Text;

                // Müşteri bilgilerini güncelle
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // CUSTOMER tablosunu güncelle
                    string updateQuery = @"
                UPDATE CUSTOMER 
                SET name = @name, description = @description, countryId = @countryId, cityId = @cityId 
                WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(updateQuery, conn);
                    cmd.Parameters.AddWithValue("@name", newName);
                    cmd.Parameters.AddWithValue("@description", newDesc);
                    cmd.Parameters.AddWithValue("@countryId", newCountryId);
                    cmd.Parameters.AddWithValue("@cityId", newCityId);
                    cmd.Parameters.AddWithValue("@Id", row["Müşteri ID"]);
                    cmd.ExecuteNonQuery();

                    // İlişkili tabloları güncelle
                    int customerId = Convert.ToInt32(row["Müşteri ID"]);
                    UpdateCustomerMachines(customerId, conn);
                    UpdateCustomerBrands(customerId);
                    UpdateCustomerProfileSeries(customerId);
                    UpdateCustomerProfileExamples(customerId);
                    UpdateCustomerSerialNumbers(customerId);

                    MessageBox.Show("Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.");
                    Clean();
                    TableRead();
                }

                // Log kaydını oluştur
                string action = $"Müşteri Güncellendi:\n" +
                                $"Ad: {oldName} -> {newName}\n" +
                                $"Açıklama: {oldDesc} -> {newDesc}\n" +
                                $"Ülke: {oldCountryId} -> {newCountryId}\n" +
                                $"Şehir: {oldCityId} -> {newCityId}\n" +
                                $"Makineler: {oldMachines} -> {newMachines}\n" +
                                $"Markalar: {oldBrands} -> {newBrands}\n" +
                                $"Profil Serileri: {oldProfileSeries} -> {newProfileSeries}\n" +
                                $"Profil Örnekleri: {oldExamples} -> {newExamples}\n" +
                                $"Seri Numaraları: {oldSerialNumbers} -> {newSerialNumbers}";
                LogUserProcess.AddLog(currentUserId, action);
            }
        }

        // Ara tablolardaki güncelleme işleminde seçili olan kısmı silip, yeni veriyi ekliyoruz.
        private void UpdateCustomerMachines(int customerId, SqlConnection conn)
        {

            string deleteQuery = "DELETE FROM CUSTOMER_MACHINE WHERE CustomerId = @customerId";
            SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
            deleteCmd.Parameters.AddWithValue("@customerId", customerId);
            deleteCmd.ExecuteNonQuery();


            foreach (var selectedMachine in lstMachine.SelectedItems)
            {
                int machineId = Convert.ToInt32(((DataRowView)selectedMachine)["Id"]);

                string insertQuery = @"
            INSERT INTO CUSTOMER_MACHINE (CustomerId, MachineId) 
            VALUES (@customerId, @machineId);";

                SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                insertCmd.Parameters.AddWithValue("@customerId", customerId);
                insertCmd.Parameters.AddWithValue("@machineId", machineId);
                insertCmd.ExecuteNonQuery();
            }
        }

        private void UpdateCustomerBrands(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Eski markaları sil
                string deleteQuery = "DELETE FROM CUSTOMER_BRANDS WHERE customerId = @customerId";
                SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@customerId", customerId);
                deleteCmd.ExecuteNonQuery();

                // Yeni markaları ekle
                foreach (var selectedBrand in lstpBrand.SelectedItems)
                {
                    var brandRow = (DataRowView)selectedBrand;
                    int brandId = Convert.ToInt32(brandRow["Id"]);

                    string insertQuery = @"
                INSERT INTO CUSTOMER_BRANDS (customerId, brandId)
                VALUES (@customerId, @brandId)";
                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@customerId", customerId);
                    insertCmd.Parameters.AddWithValue("@brandId", brandId);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        private void UpdateCustomerProfileSeries(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Önce eski serileri temizle
                string deleteQuery = "DELETE FROM CUSTOMER_PROFILE_SERIES WHERE customerId = @customerId";
                SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@customerId", customerId);
                deleteCmd.ExecuteNonQuery();

                // Brand_Series tablosunu temizle
                string deleteBSQuery = "DELETE FROM BRAND_SERIES WHERE customerId = @customerId";
                SqlCommand deleteBSCmd = new SqlCommand(deleteBSQuery, conn);
                deleteBSCmd.Parameters.AddWithValue("@customerId", customerId);
                deleteBSCmd.ExecuteNonQuery();

                // Yeni serileri ekle
                InsertCustomerProfileSeries(customerId);
            }
        }

        private void UpdateCustomerSerialNumbers(int customerId)
        {

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Eski seri numaralarını sil
                string deleteQuery = "DELETE FROM SERIAL_NUMBERS WHERE customerId = @customerId";
                SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@customerId", customerId);
                deleteCmd.ExecuteNonQuery();

                // Yeni seri numaralarını ekle
                InsertCustomerSerialNumbers(customerId);
            }
        }

        private void UpdateCustomerProfileExamples(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Önce mevcut kayıtları sil
                string deleteQuery = "DELETE FROM CUSTOMER_PROFILE_EXAMPLES WHERE customerId = @customerId";
                SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@customerId", customerId);
                deleteCmd.ExecuteNonQuery();

                // Yeni kayıtları ekle
                foreach (var selectedProfileExample in lstpexm.SelectedItems)
                {
                    var profileExampleRow = (DataRowView)selectedProfileExample;
                    int profileExampleId = Convert.ToInt32(profileExampleRow["Id"]);

                    string insertQuery = @"
                INSERT INTO CUSTOMER_PROFILE_EXAMPLES (customerId, profileExampleId) 
                VALUES (@customerId, @profileExampleId)";
                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@customerId", customerId);
                    insertCmd.Parameters.AddWithValue("@profileExampleId", profileExampleId);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        // İlgili tablolar için veri çekme işlemi
        private string GetRelatedData(SqlConnection conn, object customerId, string customerTable, string relatedTable, string relatedId, string relatedName)
        {
            if (!int.TryParse(customerId?.ToString(), out int parsedCustomerId))
            {
                throw new FormatException($"customerId is not in a correct format: {customerId}");
            }
            string query = $@"
        SELECT STRING_AGG(rt.{relatedName}, ', ') 
        FROM {customerTable} ct
        JOIN {relatedTable} rt ON ct.{relatedId} = rt.Id
        WHERE ct.customerId = @customerId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@customerId", customerId);
            return Convert.ToString(cmd.ExecuteScalar());
        }

        // Seri numaraları için özel işlem
        private string GetSerialNumbers(SqlConnection conn, object customerId)
        {
            string query = "SELECT STRING_AGG(serialNum, ', ') FROM SERIAL_NUMBERS WHERE customerId = @customerId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@customerId", customerId);
            return Convert.ToString(cmd.ExecuteScalar());
        }
        #endregion

        #region Customer Delete
        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            if (dgCustomers.SelectedItem is DataRowView selectedRow)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Bu müşteriyi silmek istediğinizden emin misiniz?",
                    "Silme Onayı",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        int currentUserId = LoginPage.FindUserId.CurrentUserId;
                        int customerId = Convert.ToInt32(selectedRow["Müşteri ID"]);

                        // Eski müşteri bilgilerini al
                        string customerName = selectedRow["Müşteri Adı"].ToString();
                        string customerDescription = selectedRow["Açıklama"].ToString();
                        string country = selectedRow["Ülke"].ToString();
                        string city = selectedRow["Şehir"].ToString();
                        string machines = selectedRow["Makineler"]?.ToString() ?? "Yok";
                        string brands = selectedRow["Markalar"]?.ToString() ?? "Yok";
                        string profileSeries = selectedRow["Profil Serileri"]?.ToString() ?? "Yok";
                        string examples = selectedRow["Profil Örnekleri"]?.ToString() ?? "Yok";
                        string serialNumbers = selectedRow["Seri Numaraları"]?.ToString() ?? "Yok";

                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            conn.Open();

                            // Ara tablolardan ilişkileri sil
                            DeleteCustomerMachines(customerId, conn);
                            DeleteCustomerBrands(customerId, conn);
                            DeleteCustomerProfileSeries(customerId, conn);
                            DeleteBrandSeries(customerId, conn);
                            DeleteCustomerSerialNumbers(customerId, conn);
                            DeleteCustomerProfileExamples(customerId, conn);

                            // CUSTOMER tablosundan müşteri kaydını sil
                            string deleteCustomerQuery = "DELETE FROM CUSTOMER WHERE Id = @customerId";
                            SqlCommand cmd = new SqlCommand(deleteCustomerQuery, conn);
                            cmd.Parameters.AddWithValue("@customerId", customerId);
                            cmd.ExecuteNonQuery();
                        }
                        string logMessage = $"Müşteri Silindi:\n" +
                                $"Ad: {customerName},\nAçıklama: {customerDescription},\nÜlke: {country},\nŞehir: {city},\n" +
                                $"Makineler: {machines},\nMarkalar: {brands},\nProfil Serileri: {profileSeries},\n" +
                                $"Profil Örnekleri: {examples},\nSeri Numaraları: {serialNumbers}";
                        LogUserProcess.AddLog(currentUserId, logMessage);

                        MessageBox.Show("Müşteri başarıyla silindi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        Clean();
                        TableRead();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"HATA: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir müşteri seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Ara tablolar silme işlemi
        private void DeleteCustomerMachines(int customerId, SqlConnection conn) //
        {
            string deleteQuery = "DELETE FROM CUSTOMER_MACHINE WHERE CustomerId = @customerId";
            SqlCommand cmd = new SqlCommand(deleteQuery, conn);
            cmd.Parameters.AddWithValue("@customerId", customerId);
            cmd.ExecuteNonQuery();
        }

        private void DeleteCustomerBrands(int customerId, SqlConnection conn)
        {
            string deleteQuery = "DELETE FROM CUSTOMER_BRANDS WHERE CustomerId = @customerId";
            SqlCommand cmd = new SqlCommand(deleteQuery, conn);
            cmd.Parameters.AddWithValue("@customerId", customerId);
            cmd.ExecuteNonQuery();
        }

        private void DeleteCustomerProfileSeries(int customerId, SqlConnection conn)
        {
            // Customer_Profile_Series tablosundan sil
            string deleteCPSQuery = @"
                DELETE FROM CUSTOMER_PROFILE_SERIES 
                WHERE customerId = @customerId";
            SqlCommand deleteCPSCmd = new SqlCommand(deleteCPSQuery, conn);
            deleteCPSCmd.Parameters.AddWithValue("@customerId", customerId);
            deleteCPSCmd.ExecuteNonQuery();

            // Brand_Series tablosundan sil
            string deleteBSQuery = @"
                DELETE FROM BRAND_SERIES 
                WHERE customerId = @customerId";
            SqlCommand deleteBSCmd = new SqlCommand(deleteBSQuery, conn);
            deleteBSCmd.Parameters.AddWithValue("@customerId", customerId);
            deleteBSCmd.ExecuteNonQuery();
        }

        private void DeleteBrandSeries(int customerId, SqlConnection conn) // profile_brands ve profile_series alt tablosunun içindeki verileri siler
        {
            string deleteQuery = "DELETE FROM BRAND_SERIES WHERE CustomerId = @customerId";
            SqlCommand cmd = new SqlCommand(deleteQuery, conn);
            cmd.Parameters.AddWithValue("@customerId", customerId);
            cmd.ExecuteNonQuery();
        }

        private void DeleteCustomerSerialNumbers(int customerId, SqlConnection conn)
        {
            string deleteQuery = "DELETE FROM SERIAL_NUMBERS WHERE CustomerId = @customerId";
            SqlCommand cmd = new SqlCommand(deleteQuery, conn);
            cmd.Parameters.AddWithValue("@customerId", customerId);
            cmd.ExecuteNonQuery();
        }

        private void DeleteCustomerProfileExamples(int customerId, SqlConnection conn)
        {
            string query = "DELETE FROM CUSTOMER_PROFILE_EXAMPLES WHERE customerId = @customerId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@customerId", customerId);
            cmd.ExecuteNonQuery();
        }
        #endregion

        private void Clean() //insert ve update işlemi yaptıktan sonra textbox ve combobox temizliğini yapacak.
        {
            txtName.Text = string.Empty;
            txtDesc.Text = string.Empty;
            cmbCountry.SelectedIndex = -1;
            cmbCity.SelectedIndex = -1;
            lstMachine.SelectedIndex = -1;
            lstpBrand.SelectedIndex = -1;
            lstpSeries.SelectedIndex = -1;
            lstpexm.SelectedIndex = -1;
            txtSerieNumber.Text = string.Empty;
        }

        private void cmbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e) // ülke seçimine göre gelen şehir combobox
        {
            if (cmbCountry.SelectedValue != null)
            {
                int countryId = (int)cmbCountry.SelectedValue;


                string queryCity = "SELECT Id, name FROM CITIES WHERE countryId = @countryId";

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(queryCity, conn);
                    cmd.Parameters.AddWithValue("@countryId", countryId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dtCity = new DataTable();
                    adapter.Fill(dtCity);

                    cmbCity.ItemsSource = dtCity.DefaultView;
                    cmbCity.DisplayMemberPath = "name";
                    cmbCity.SelectedValuePath = "Id";
                    cmbCity.SelectedIndex = -1;
                }
            }
            else
            {
                // Ülke seçilmezse şehirleri temizle
                cmbCity.ItemsSource = null;
                cmbCity.SelectedIndex = -1;
            }
        }

        #region Select Multiple Brand and Profile Series
        private void lstpBrand_SelectionChanged(object sender, SelectionChangedEventArgs e) // marka seçimine göre gelen profil serisi listbox
        {
            if (lstpBrand.SelectedItems.Count > 0)
            {
                var brandIds = lstpBrand.SelectedItems
                    .Cast<DataRowView>()
                    .Select(item => Convert.ToInt32(item["Id"]))
                    .ToList();

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"
                SELECT DISTINCT ps.Id, ps.name
                FROM PROFILE_SERIES ps
                LEFT JOIN BRAND_SERIES bs ON ps.Id = bs.SeriesId
                WHERE ps.brandId IN (" + string.Join(",", brandIds) + ")";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    lstpSeries.ItemsSource = dt.DefaultView;
                    lstpSeries.DisplayMemberPath = "name";
                    lstpSeries.SelectedValuePath = "Id";
                }
            }
            else
            {
                lstpSeries.ItemsSource = null;
            }
        }


        private void LoadProfileSeries(List<int> brandIds)
        {
            if (brandIds == null || brandIds.Count == 0) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Dinamik SQL sorgusu oluştur
                    string querySeries = $@"
                SELECT DISTINCT Id, name 
                FROM PROFILE_SERIES
                WHERE brandId IN ({string.Join(",", brandIds)});";

                    SqlCommand cmd = new SqlCommand(querySeries, conn);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dtSeries = new DataTable();
                    adapter.Fill(dtSeries);

                    // Profil serileri ListBox'unu doldur
                    lstpSeries.ItemsSource = dtSeries.DefaultView;
                    lstpSeries.DisplayMemberPath = "name";
                    lstpSeries.SelectedValuePath = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"HATA: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }

}

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

namespace WpfExp.ModeratorPage
{
    /// <summary>
    /// Interaction logic for _3_ModCustomer.xaml
    /// </summary>
    public partial class _3_ModCustomer : Window
    {
        private DataRowView row;
        private string connStr = @"Server=192.168.5.68,1433;Database=OZGENC;User Id=sa; Password=ozgencmachine16;Trusted_Connection=True;";

        public _3_ModCustomer()
        {
            InitializeComponent();
            TableRead();
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

            lstpexm.DisplayMemberPath = "name";
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

        #region CUSTOMER INSERT
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
                    InsertCustomerProfileExamples(customerId, conn);
                    InsertCustomerSerialNumbers(customerId);

                    MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    LogUserProcess.AddLog(currentUserId, $"Müşteri Kaydı Eklendi:\n Müşteri Adı: {txtName.Text}");

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

        private void InsertCustomerProfileExamples(int customerId, SqlConnection conn)
        {
            foreach (var selectedProfileExample in lstpexm.SelectedItems)
            {
                int exampleId = Convert.ToInt32(((DataRowView)selectedProfileExample)["Id"]);

                string insertQuery = @"
            INSERT INTO CUSTOMER_PROFILE_EXAMPLES (customerId, profileExampleId) 
            VALUES (@customerId, @profileExampleId);";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@customerId", customerId);
                cmd.Parameters.AddWithValue("@profileExampleId", exampleId);
                cmd.ExecuteNonQuery();
            }
        }
        #endregion


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
            if (lstpBrand.SelectedValue != null)
            {
                int brandId = (int)lstpBrand.SelectedValue;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string querySeries = "SELECT Id, name FROM PROFILE_SERIES WHERE brandId = @brandId OR brandId =  @brandId";
                    SqlCommand cmd = new SqlCommand(querySeries, conn);
                    cmd.Parameters.AddWithValue("@brandId", brandId);

                    SqlDataAdapter daSeries = new SqlDataAdapter(cmd);
                    DataTable dtSeries = new DataTable();
                    daSeries.Fill(dtSeries);

                    lstpSeries.ItemsSource = dtSeries.DefaultView;
                    lstpSeries.DisplayMemberPath = "name";
                    lstpSeries.SelectedValuePath = "Id";
                    conn.Close();
                }
            }
        }

        private void Clean()
        {
            txtName.Text = "";
            txtDesc.Text = "";
            lstpexm.SelectedIndex = -1;
            txtSerieNumber.Text = "";
            cmbCountry.SelectedIndex = -1;
            cmbCity.SelectedIndex = -1;
            lstMachine.SelectedIndex = -1;
            lstpBrand.SelectedIndex = -1;
            lstpSeries.SelectedIndex = -1;
        }

        private void lstpBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
    }
}

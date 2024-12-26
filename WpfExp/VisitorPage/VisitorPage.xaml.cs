using System;
using System.Collections.Generic;
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
using WpfExp.ModeratorPage;

namespace WpfExp.VisitorPage
{
    /// <summary>
    /// Interaction logic for VisitorPage.xaml
    /// </summary>
    public partial class VisitorPage : Window
    {
        public VisitorPage()
        {
            InitializeComponent();
            conn = new Connection(@"Server=DESKTOP-F5QQ2O8;Database=OZGENC;Trusted_Connection=True;");
            LoadCountry();
        }

        public int selectedIndex { get; set; }
        private Connection conn;
        //Ülkeleri çeken fonksiyon
        private void LoadCountry()
        {
            List<string> countries = conn.GetCountry();
            ulkeler.ItemsSource = countries;

        }

        private void DataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            var rowData = e.Row.Item as DataRowView;
            if (rowData == null) return; // Eğer satır verisi boşsa geri dön

            try
            {
                // Müşteri ID'sini al
                int custId = Convert.ToInt32(rowData["CustomerId"]);

                // Detayları al
                var brandDetails = conn.DetailBrands(custId);
                var seriesDetails = conn.DetailSeries(custId);
                var exampleDetails = conn.DetailExamples(custId);

                // Detay verilerini bağlamak için bir bağlam oluştur
                var detailsContext = new
                {
                    DetailBrands = brandDetails.DefaultView,
                    DetailSeries = seriesDetails.DefaultView,
                    DetailExamples = exampleDetails.DefaultView
                };

                // Detay elemanına bağlamı ata
                e.DetailsElement.DataContext = detailsContext;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"HATA! Detaylar yüklenirken bir hata oluştu.\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void selectCities(string selectedCountry)
        {
            List<string> cities = conn.GetCities(selectedCountry);
            sehirler.ItemsSource = cities;

        }

        private void countries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbx = sender as ComboBox;
            selectedIndex = cmbx.SelectedIndex;
            if (ulkeler.SelectedItem != null)
            {
                string selectedCountry = ulkeler.SelectedItem.ToString();
                selectCities(selectedCountry);

            }

        }

        private void selectCustomers(string selectedCities)
        {
            List<Customers> customer = conn.GetCustomers(selectedCities);
            musteri.ItemsSource = customer;

            musteri.DisplayMemberPath = "name";
            musteri.SelectedValuePath = "CustomerID";
        }
        private void sehirler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbx = sender as ComboBox;
            selectedIndex = cmbx.SelectedIndex;
            if (cmbx.SelectedItem != null)
            {
                string selectedCities = sehirler.SelectedItem.ToString();
                List<Customers> customerList = conn.GetCustomers(selectedCities);
                musteri.ItemsSource = customerList;


            }

        }

        private void musteri_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = sender as ComboBox;
            if (a.SelectedValue != null)
            {
                int selectedCustomerId = (int)musteri.SelectedValue;
                LoadMachines(selectedCustomerId);
            }
        }

        private void LoadMachines(int selectedCustomerId)
        {
            var machineData = conn.GetMachines(selectedCustomerId);
            dgMakina.ItemsSource = machineData.DefaultView;
        }

        private void dgMakina_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            var DataRow = grid.SelectedItem;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VisitorButtonPage visitorPage = new VisitorButtonPage();
            visitorPage.Show();
        }

        private void ClosedWindow(object sender, EventArgs e)
        {
            WpfExp.LoginPage.LoginPage loginPage = new LoginPage.LoginPage();
            loginPage.Show();
        }
    }
}

using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.Runtime.InteropServices.ComTypes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data;
using WpfExp.Models;
using Syncfusion.Windows.Tools.Controls;

namespace WpfExp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int selectedIndex { get; set; }
        private Connection conn;

        public MainWindow()
        {
            InitializeComponent();
            conn = new Connection();
            LoadCountry();
        }
        //Ülkeleri çeken fonksiyon
        private void LoadCountry()
        {
            List<string> countries = conn.GetCountry();
            ulkeler.ItemsSource = countries;

        }

        private void DataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            var rowData = e.Row.Item as DataRowView;
            if (rowData == null) return;

            int brandId = Convert.ToInt32(rowData["machID"]);
            //string sname = rowData["name"].ToString();
            
            var brandDetails = conn.DetailBrands(brandId);
            var seriesDetails = conn.DetailSeries(brandId);
            var exampleDetails = conn.DetailExamples(brandId);
 
            var detailsContext = new
            {
                DetailBrands = brandDetails.DefaultView,
                DetailSeries = seriesDetails.DefaultView,
                DetailExamples = exampleDetails.DefaultView
            };

            e.DetailsElement.DataContext = detailsContext;
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
            GoButtonsWindow buttonWindow = new GoButtonsWindow();
            buttonWindow.Show();
        }


      /*private void details(object sender, RoutedEventArgs e) //tablo içindeki sütunlardan daha ayrıntılı bilgi
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility =
                    row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }*/

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }
    }

    }

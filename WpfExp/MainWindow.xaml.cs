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

        private void LoadCountry()
        {
            List<string> countries = conn.GetCountry();
            ulkeler.ItemsSource = countries;

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
                
            } }
        
        private void selectCustomers(string selectedCities)
        {
            List<string> customer = conn.GetCustomers(selectedCities);
            musteri.ItemsSource = customer;

        }


        private void sehirler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbx = sender as ComboBox;
            selectedIndex = cmbx.SelectedIndex;
            if (sehirler.SelectedItem != null)
            {
                string selectedCities = sehirler.SelectedItem.ToString();
                List<string> customerList = conn.GetCustomers(selectedCities);
                musteri.ItemsSource = customerList;
                

            }
            
        }


    }

    }

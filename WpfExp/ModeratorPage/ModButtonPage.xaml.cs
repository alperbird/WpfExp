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
using System.Windows.Shapes;

namespace WpfExp.ModeratorPage
{
    /// <summary>
    /// Interaction logic for ModButtonPage.xaml
    /// </summary>
    public partial class ModButtonPage : Window
    {
        public ModButtonPage()
        {
            InitializeComponent();
        }

        public void NewOpenWindow(Window newWindow)
        {
            this.Hide();
            newWindow.ShowDialog();
            this.Show();
        }

        private void countryCreate(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _1_ModCountry());
        }

        private void cityCreate(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _2_ModCity());
        }

        private void brandCreate(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _5_ModBrand());
        }

        private void customerCreate(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _3_ModCustomer());
        }

        private void machineCreate(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _4_ModMachine());
        }

        private void pseriesCreate(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _6_ModSerie());
        }

        private void pexmCreate(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _7_ModExm());
        }

        private void machineTypeCreate(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _8_ModType());
        }
    }
}

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

namespace WpfExp.VisitorPage
{
    /// <summary>
    /// Interaction logic for VisitorButtonPage.xaml
    /// </summary>
    public partial class VisitorButtonPage : Window
    {
        public VisitorButtonPage()
        {
            InitializeComponent();
        }

        public void NewOpenWindow(Window newWindow)
        {
            this.Hide();
            newWindow.ShowDialog();
            this.Show();
        }

        private void countryRead(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _1_VisitCountry());
        }

        private void cityRead(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _2_VisitCity());
        }

        private void brandRead(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _5_VisitBrand());
        }

        private void customerRead(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _3_VisitCustomer());
        }

        private void machineRead(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _4_VisitMachine());
        }

        private void pseriesRead(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _6_VisitSerie());
        }

        private void pexmRead(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _7_VisitExm());
        }

        private void machineTypeRead(object sender, RoutedEventArgs e)
        {
            NewOpenWindow(new _8_VisitType());
        }
    }
}

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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfExp.PageBrand;
using WpfExp.PageCity;
using WpfExp.PageCountry;
using WpfExp.PageExample;
using WpfExp.PageMachine;
using WpfExp.PageSeries;


namespace WpfExp
{
    /// <summary>
    /// Interaction logic for GoButtonsWindow.xaml
    /// </summary>
    public partial class GoButtonsWindow : Window
    {
        public GoButtonsWindow()
        {
            InitializeComponent();
        }

        public void OpenNewWindow(Window newWindow) // Bazı çiçekler bazı topraklarda yetişmiyor...
        {
            this.Hide();
            newWindow.ShowDialog();
            this.Show();
        }

        private void countryCRUD(object sender, RoutedEventArgs e) // Ülke butonu. Tıklayınca tablo, Insert, Update ve Delete işlemleri gözükecek (inş)
        {
            OpenNewWindow(new CRUDWindowCountry());
        }

        private void cityCRUD(object sender, RoutedEventArgs e) // Şehir butonu. Tıklayınca tablo, Insert, Update ve Delete işlemleri gözükecek (inş)
        {
            OpenNewWindow(new CRUDWindowCity());
        }

        private void brandCRUD(object sender, RoutedEventArgs e) // Marka butonu. Tıklayınca tablo, Insert, Update ve Delete işlemleri gözükecek (inş)
        {
            OpenNewWindow(new CRUDWindowProfileBrand());
        }

        private void customerCRUD(object sender, RoutedEventArgs e) // Aynısının müşteri versiyonu
        {
            OpenNewWindow(new CRUDWindowCustomer());
        }

        private void machineCRUD(object sender, RoutedEventArgs e) // Aynısının makine versiyonu
        {
            OpenNewWindow(new CRUDWindowMachine());
        }

        private void pseriesCRUD(object sender, RoutedEventArgs e) // Aynısının profil serisi versiyonu
        {
            OpenNewWindow(new CRUDWindowProfileSeries());
        }

        private void pexmCRUD(object sender, RoutedEventArgs e) // dıdıdımın dıdısının dıdısının dısıdısının dıdısı
        {
            OpenNewWindow(new CRUDWindowProfieExample());
        }
    }
}

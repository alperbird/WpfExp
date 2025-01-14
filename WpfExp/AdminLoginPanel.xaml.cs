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

namespace WpfExp
{
    /// <summary>
    /// Interaction logic for AdminLoginPanel.xaml
    /// </summary>
    public partial class AdminLoginPanel : Window
    {
        public AdminLoginPanel()
        {
            InitializeComponent();  
        }

        private void btnAdminPanel(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Hide();
        }

        private void btnLogPanel(object sender, RoutedEventArgs e)
        {
            LogRecords.LogRecords log = new LogRecords.LogRecords();
            log.Show();
        }

        private void ClosedWindow(object sender, EventArgs e)
        {
            WpfExp.LoginPage.LoginPage loginPage = new LoginPage.LoginPage();
            loginPage.Show();
        }
    }
}

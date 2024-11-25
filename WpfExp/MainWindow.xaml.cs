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

namespace WpfExp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string connectionString = @"Data Source=C:\\Users\\root\\source\\repos\\WpfExp\\WpfExp\\bin\\DebugTurbocut-400-500.db;Version=3;";
        private SQLiteConnection conn;
        public MainWindow() 
        {
            InitializeComponent();
            conn = new SQLiteConnection(connectionString);
            conn.Open();
            
        }

        private void countries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            conn.Open();
            string countriesQuery = "SELECT * FROM COUNTRIES";
            SQLiteCommand command = new SQLiteCommand(countriesQuery, conn);
            SQLiteDataAdapter adapt = new SQLiteDataAdapter(command);
            conn.Close();
        }

        private void cities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            conn.Open();
            string countriesQuery = "SELECT * FROM CITIES";
            SQLiteCommand command = new SQLiteCommand(countriesQuery, conn);
            SQLiteDataAdapter adapt = new SQLiteDataAdapter(command);
            conn.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace WpfExp.VisitorPage
{
    /// <summary>
    /// Interaction logic for _1_VisitCountry.xaml
    /// </summary>
    public partial class _1_VisitCountry : Window
    {
        public string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        public _1_VisitCountry()
        {
            InitializeComponent();
            TableRead();
        }

        public void TableRead() // tablo okuma  
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT Id, name AS 'Ülke Adı', description AS 'Açıklama' FROM COUNTRIES";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgCountries.ItemsSource = dt.DefaultView;
                }
            }
        }
    }
}

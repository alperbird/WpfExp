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

namespace WpfExp.VisitorPage
{
    /// <summary>
    /// Interaction logic for _2_VisitCity.xaml
    /// </summary>
    public partial class _2_VisitCity : Window
    {
        public string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        public _2_VisitCity()
        {
            InitializeComponent();
            TableRead();
        }

        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string readQueary = "SELECT ci.Id, ci.name AS 'Şehir Adı', ci.description AS 'Açıklama', co.name AS 'Ülke' FROM CITIES ci " +
                    "INNER JOIN COUNTRIES co ON " +
                    "ci.countryId = co.Id";
                using (SqlCommand cmd = new SqlCommand(readQueary, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgCities.ItemsSource = dt.DefaultView;
                }
            }
        }
    }
}

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

namespace WpfExp.VisitorPage
{
    /// <summary>
    /// Interaction logic for _5_VisitBrand.xaml
    /// </summary>
    public partial class _5_VisitBrand : Window
    {
        public string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        public _5_VisitBrand()
        {
            InitializeComponent();
            TableRead();
        }

        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string brandQuery = "SELECT Id, name AS 'Marka Adı', description AS 'Açıklama' FROM PROFILE_BRANDS";

                using (SqlCommand cmd = new SqlCommand(brandQuery, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgBrands.ItemsSource = dt.DefaultView;
                }
            }
        }
    }
}

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
    /// Interaction logic for _6_VisitSerie.xaml
    /// </summary>
    public partial class _6_VisitSerie : Window
    {
        private string connStr = @"Server=192.168.5.68,1433;Database=OZGENC;User Id=sa; Password=ozgencmachine16;Trusted_Connection=True;";
        public _6_VisitSerie()
        {
            InitializeComponent();
            TableRead();
        }

        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string serieQuery = "SELECT ps.Id, ps.name AS 'Seri Adı', ps.description AS 'Açıklama', pb.name AS 'Marka' FROM PROFILE_SERIES ps " +
                    "JOIN PROFILE_BRANDS pb ON " +
                    "ps.brandId = pb.Id";
                using (SqlCommand cmd = new SqlCommand(serieQuery, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgSeries.ItemsSource = dt.DefaultView;
                }
            }
        }
    }
}

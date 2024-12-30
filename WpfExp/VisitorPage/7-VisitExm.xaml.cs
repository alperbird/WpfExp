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
using System.Configuration;

namespace WpfExp.VisitorPage
{
    /// <summary>
    /// Interaction logic for _7_VisitExm.xaml
    /// </summary>
    public partial class _7_VisitExm : Window
    {
        private string connStr;
        public _7_VisitExm()
        {
            InitializeComponent();
            connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            TableRead();
        }

        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string exmQuery = "SELECT Id, name AS 'Ad', description AS 'Açıklama' FROM PROFILE_EXAMPLE";
                using (SqlCommand cmd = new SqlCommand(exmQuery, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgExamples.ItemsSource = dt.DefaultView;
                }
            }
        }
    }
}

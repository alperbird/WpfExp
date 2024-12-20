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
    /// Interaction logic for _4_VisitMachine.xaml
    /// </summary>
    public partial class _4_VisitMachine : Window
    {
        public string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        public _4_VisitMachine()
        {
            InitializeComponent();
            TableRead();
        }

        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string machQuery = "SELECT ma.Id, ma.name AS 'Makine Adı', ma.description AS 'Açıklama', mt.name AS 'Makine Türü' FROM MACHINES ma " +
                    "JOIN MACHINE_TYPE mt ON ma.typeId = mt.Id";
                using (SqlCommand cmd = new SqlCommand(machQuery, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgMachines.ItemsSource = dt.DefaultView;
                }
            }
        }
    }
}

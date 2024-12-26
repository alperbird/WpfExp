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
    /// Interaction logic for _8_VisitType.xaml
    /// </summary>
    public partial class _8_VisitType : Window
    {
        private string connStr = @"Server=DESKTOP-F5QQ2O8;Database=OZGENC;Trusted_Connection=True;";
        public _8_VisitType()
        {
            InitializeComponent();
            TableRead();
        }

        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string typeQuery = "SELECT Id, name AS 'İşlem Adı', description AS 'Açıklama' FROM MACHINE_TYPE";
                using (SqlCommand cmd = new SqlCommand(typeQuery, conn))
                {
                    SqlDataAdapter adap = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adap.Fill(dt);

                    dgMachineTypes.ItemsSource = dt.DefaultView;
                }

            }
        }
    }
}

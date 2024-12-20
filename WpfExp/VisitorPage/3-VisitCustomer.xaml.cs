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
    /// Interaction logic for _3_VisitCustomer.xaml
    /// </summary>
    public partial class _3_VisitCustomer : Window
    {
        private string connStr = @"Server=(localdb)\MSSQLLocalDB;Database=OZGENC;Trusted_Connection=True;";
        public _3_VisitCustomer()
        {
            InitializeComponent();
            TableRead();
        }

        public void TableRead()
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT cu.Id, cu.name AS 'Müşteri Adı', cu.description AS 'Açıklama', co.name AS 'Ülke', ci.name AS 'Şehir', " +
                        "ma.name 'Makine', pb.name AS 'Marka', ps.name AS 'Profil Serisi', pe.name AS 'Profil Örnek', snm.serialNum AS 'Seri Numarası' " +
                        "FROM CUSTOMER cu JOIN COUNTRIES co ON cu.countryId = co.Id " +
                        "JOIN CITIES ci ON cu.cityId = ci.Id " +
                        "JOIN MACHINES ma ON cu.machineId = ma.Id " +
                        "JOIN PROFILE_BRANDS pb ON cu.pbrandId = pb.Id " +
                        "JOIN PROFILE_SERIES ps ON cu.pserieId = ps.Id " +
                        "JOIN PROFILE_EXAMPLE pe ON cu.pexmId = pe.Id " +
                        "JOIN SERIAL_NUMBERS snm ON cu.serialnumId = snm.Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        dgCustomers.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("HATA!\n " + ex.Message);
            }
        }
    }
}

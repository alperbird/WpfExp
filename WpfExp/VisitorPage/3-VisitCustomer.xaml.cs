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
        private string connStr = @"Server=DESKTOP-F5QQ2O8;Database=OZGENC;Trusted_Connection=True;";
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

                    string query = @"
                                    SELECT 
                                    cu.Id AS 'Müşteri ID',
                                    cu.name AS 'Müşteri Adı',
                                    cu.description AS 'Açıklama',
                                    co.name AS 'Ülke',
                                    ci.name AS 'Şehir',
                                    -- Makineler
                                    (
                                        SELECT STRING_AGG(ma.name, ', ') 
                                        FROM CUSTOMER_MACHINE cm
                                        JOIN MACHINES ma ON cm.machineId = ma.Id
                                        WHERE cm.customerId = cu.Id
                                    ) AS 'Makineler',
                                    -- Markalar
                                    (
                                        SELECT STRING_AGG(pb.name, ', ') 
                                        FROM CUSTOMER_BRANDS cb
                                        JOIN PROFILE_BRANDS pb ON cb.brandId = pb.Id
                                        WHERE cb.customerId = cu.Id
                                    ) AS 'Markalar',
                                    -- Profil Serileri
                                    (
                                        SELECT STRING_AGG(ps.name, ', ') 
                                        FROM CUSTOMER_PROFILE_SERIES cps
                                        JOIN PROFILE_SERIES ps ON cps.seriesId = ps.Id
                                        WHERE cps.customerId = cu.Id
                                    ) AS 'Profil Serileri',
                                    -- Profil Örnekleri
                                    (
                                        SELECT STRING_AGG(pe.name, ', ') 
                                        FROM CUSTOMER_PROFILE_EXAMPLES cpe
                                        JOIN PROFILE_EXAMPLE pe ON cpe.ProfileExampleId = pe.Id
                                        WHERE cpe.CustomerId = cu.Id
                                    ) AS 'Profil Örnekleri',
                                    -- Seri Numaraları
                                    (
                                        SELECT STRING_AGG(snm.serialNum, ', ') 
                                        FROM SERIAL_NUMBERS snm
                                        WHERE snm.CustomerId = cu.Id
                                    ) AS 'Seri Numaraları'
                                    FROM CUSTOMER cu
                                    LEFT JOIN COUNTRIES co ON cu.countryId = co.Id
                                    LEFT JOIN CITIES ci ON cu.cityId = ci.Id;
";

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
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
    }
}

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
using WpfExp.LogRecords;

namespace WpfExp.ModeratorPage
{
    /// <summary>
    /// Interaction logic for _8_ModType.xaml
    /// </summary>
    public partial class _8_ModType : Window
    {
        private string connStr = @"Server=192.168.5.68,1433;Database=OZGENC;User Id=sa; Password=ozgencmachine16;Trusted_Connection=True;";
        public _8_ModType()
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

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO MACHINE_TYPE(name, description) VALUES(@name, @description)";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@name", txtTypeName.Text);
                cmd.Parameters.AddWithValue("@description", txtTypeDesc.Text);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                LogUserProcess.AddLog(currentUserId, $"Makine Tipi Eklendi: {txtTypeName.Text}");

                Clean();

                TableRead();
            }
        }

        private void Clean()
        {
            txtTypeName.Text = "";
            txtTypeDesc.Text = "";
        }
    }
}

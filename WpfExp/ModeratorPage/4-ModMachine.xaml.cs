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
    /// Interaction logic for _4_ModMachine.xaml
    /// </summary>
    public partial class _4_ModMachine : Window
    {
        private string connStr = @"Server=192.168.5.68,1433;Database=OZGENC;User Id=sa; Password=ozgencmachine16;Trusted_Connection=True;";
        public _4_ModMachine()
        {
            InitializeComponent();
            TableRead();
            cmbxFull();
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

        private void cmbxFull()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT name FROM MACHINE_TYPE";
                SqlCommand cucmd = new SqlCommand(query, conn);
                SqlDataAdapter daCust = new SqlDataAdapter(cucmd);
                DataTable dt = new DataTable();
                daCust.Fill(dt);

                cmbType.ItemsSource = dt.DefaultView;
                cmbType.DisplayMemberPath = "name";
                cmbType.SelectedValuePath = "Id";

            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            int currentUserId = LoginPage.FindUserId.CurrentUserId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO MACHINES(name, description, typeId) " +
                    "VALUES(@name, @description, @typeId)";
                SqlCommand cmd = new SqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@name", txtMachineName.Text);
                cmd.Parameters.AddWithValue("@description", txtMachDesc.Text);
                cmd.Parameters.AddWithValue("@typeId", cmbType.SelectedValue);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi");
                LogUserProcess.AddLog(currentUserId, $"Makine Kaydı Eklendi: {txtMachineName}");

                Clean();
                TableRead();
            }
        }

        private void Clean()
        {
            txtMachineName.Text = "";
            txtMachDesc.Text = "";
            cmbType.SelectedIndex = -1;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

namespace WpfExp.PageMachine
{
    /// <summary>
    /// Interaction logic for CRUDWindowMachine.xaml
    /// </summary>
    public partial class CRUDWindowMachine : Window
    {
        public string connStr = "Data Source= Turbocut-400-500.db; Version=3;";
        public CRUDWindowMachine()
        {
            InitializeComponent();
            TableRead();
            cmbxFull();
        }

        public void TableRead()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string machQuery = "SELECT m.machID,m.name AS 'Makine Adı', m.description AS 'Açıklama', c.name AS 'Müşteri Adı'," +
                    "m.tag AS 'Etiket No' FROM MACHINES m " +
                    "INNER JOIN CUSTOMER c ON " +
                    "m.custID = c.CustomerID";
                using (SQLiteCommand cmd = new SQLiteCommand(machQuery, conn))
                {
                    SQLiteDataAdapter adapt = new SQLiteDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgMachines.ItemsSource = dt.DefaultView;
                }
            }
        }
        private void cmbxFull()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string query = "SELECT CustomerID, name FROM CUSTOMER";
                SQLiteCommand cucmd = new SQLiteCommand(query, conn);
                SQLiteDataAdapter daCust = new SQLiteDataAdapter(cucmd);
                DataTable dt = new DataTable();
                daCust.Fill(dt);

                cmbCustomer.ItemsSource = dt.DefaultView;
                cmbCustomer.DisplayMemberPath = "name";
                cmbCustomer.SelectedValuePath = "CustomerID";

            }
        }
        private void dgMachine(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if(dgMachines.SelectedItem is DataRowView row)
                {
                    txtMachineName.Text = row["Makine Adı"].ToString();
                    txtMachDesc.Text = row["Açıklama"].ToString();
                    var selectedCustomer = cmbCustomer.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == row["Müşteri Adı"].ToString());
                    cmbCustomer.SelectedItem = selectedCustomer;
                    txtTag.Text = row["Etiket No"].ToString();
                }
            }

            catch(Exception ex) 
            {
                MessageBox.Show("HATA!\n" + ex.Message);
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            using(SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO MACHINES(name, description, custID, tag) " +
                    "VALUES(@name, @description, @custID, @tag)";
                SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@name", txtMachineName.Text);
                cmd.Parameters.AddWithValue("@description", txtMachDesc.Text);
                cmd.Parameters.AddWithValue("@custID", cmbCustomer.SelectedValue);
                cmd.Parameters.AddWithValue("@tag", txtTag.Text);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi");
                Clean();
                TableRead();
            }
        }

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            if(dgMachines.SelectedItem is DataRowView row)
            {
                using (SQLiteConnection conn = new SQLiteConnection(connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE MACHINES SET " +
                        "name = @name," +
                        "description = @description," +
                        "custID = @custID," +
                        "tag = @tag " +
                        "WHERE machID = @machID ";

                    SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn);

                    cmd.Parameters.AddWithValue("@name", txtMachineName.Text);
                    cmd.Parameters.AddWithValue("@description", txtMachDesc.Text);
                    cmd.Parameters.AddWithValue("@custID", Convert.ToInt32(cmbCustomer.SelectedValue));
                    cmd.Parameters.AddWithValue("@tag", txtTag.Text);
                    cmd.Parameters.AddWithValue("@machID", row["machID"]);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.");
                    Clean();
                    TableRead();
                }
            
                
            }
        }

        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Silmek istediğine emin misin?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
            {
            if (dgMachines.SelectedItem is DataRowView row)
            {
                using(SQLiteConnection conn = new SQLiteConnection( connStr))
                {
                    conn.Open();
                    string deleteQuery = "DELETE FROM MACHINES " +
                        "WHERE machID = @machID";

                    SQLiteCommand cmd = new SQLiteCommand(deleteQuery, conn);
                    cmd.Parameters.AddWithValue("@machID", row["machID"]);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Seçilen makine silindi.");
                    Clean();
                    TableRead();
                }
            }

            }
        }

        private void Clean()
        {
            txtMachineName.Text = "";
            txtMachDesc.Text = "";
            cmbCustomer.SelectedIndex = -1;
            txtTag.Text = "";
        }
    }
}

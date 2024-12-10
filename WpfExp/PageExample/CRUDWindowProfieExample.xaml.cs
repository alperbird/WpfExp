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

namespace WpfExp.PageExample
{
    /// <summary>
    /// Interaction logic for CRUDWindowProfieExample.xaml
    /// </summary>
    public partial class CRUDWindowProfieExample : Window
    {
        public string connStr = "Data Source= Turbocut-400-500.db; Version=3;";
        public CRUDWindowProfieExample()
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
                string exmQuery = "SELECT pe.ID, pe.name AS 'Ad', pe.description AS 'Açıklama', ps.name 'Seri Adı' FROM PROFILE_TBL pe " +
                    "INNER JOIN PROFILE_SERIES ps ON " +
                    "pe.seriesID = ps.SeriesID";
                using (SQLiteCommand cmd = new SQLiteCommand(exmQuery, conn))
                {
                    SQLiteDataAdapter adapt = new SQLiteDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgExamples.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void cmbxFull()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string query = "SELECT SeriesID, name FROM PROFILE_SERIES";

                SQLiteCommand pbcmd = new SQLiteCommand(query, conn);
                SQLiteDataAdapter adpb = new SQLiteDataAdapter(pbcmd);
                DataTable dt = new DataTable();
                adpb.Fill(dt);

                cmbSerie.ItemsSource = dt.DefaultView;
                cmbSerie.DisplayMemberPath = "name";
                cmbSerie.SelectedValuePath = "SeriesID";
            }
        }
        private void dgExample(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgExamples.SelectedItem is DataRowView row)
                {
                    txtExmName.Text = row["Ad"].ToString();
                    txtExmDesc.Text = row["Açıklama"].ToString();

                    var selectedSerie = cmbSerie.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == row["Seri Adı"].ToString());
                    cmbSerie.SelectedItem = selectedSerie;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("HATA!\n" + ex.Message);
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO PROFILE_TBL (name, description, seriesID) " +
                    "VALUES(@name, @description, @seriesID)";

                SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn);

                cmd.Parameters.AddWithValue("@name", txtExmName.Text);
                cmd.Parameters.AddWithValue("@description", txtExmDesc.Text);
                cmd.Parameters.AddWithValue("@seriesID", cmbSerie.SelectedValue);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                Clean();
                TableRead();
            }
        }

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            if (dgExamples.SelectedItem is DataRowView row)
            {
                using (SQLiteConnection conn = new SQLiteConnection(connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE PROFILE_TBL SET " +
                        "name = @name," +
                        "description = @description," +
                        "seriesID = @seriesID " +
                        "WHERE ID = @ID";

                    SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn);

                    cmd.Parameters.AddWithValue("@name", txtExmName.Text);
                    cmd.Parameters.AddWithValue("@description", txtExmDesc.Text);
                    cmd.Parameters.AddWithValue("@seriesID", Convert.ToInt32(cmbSerie.SelectedValue));
                    cmd.Parameters.AddWithValue("@ID", row["ID"]);

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
            if (result == MessageBoxResult.Yes)
            {
                if (dgExamples.SelectedItem is DataRowView row)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(connStr))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM PROFILE_TBL " +
                            "WHERE ID = @ID";

                        SQLiteCommand cmd = new SQLiteCommand(deleteQuery, conn);

                        cmd.Parameters.AddWithValue("@ID", row["ID"]);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Seçilen örnek silindi.");
                        Clean();
                        TableRead();

                    }
                }
            }
        }

        private void Clean()
        {
            txtExmName.Text = "";
            txtExmDesc.Text = "";
            cmbSerie.SelectedIndex = -1;
        }
    }
}

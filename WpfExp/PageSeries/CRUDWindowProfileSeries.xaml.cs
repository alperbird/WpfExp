using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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

namespace WpfExp.PageSeries
{
    /// <summary>
    /// Interaction logic for CRUDWindowProfileSeries.xaml
    /// </summary>
    public partial class CRUDWindowProfileSeries : Window
    {
        public string connStr = "Data Source= Turbocut-400-500.db; Version=3;";
        public CRUDWindowProfileSeries()
        {
            InitializeComponent();
            TableRead();
            cmbxFull();
        }

        public void TableRead()
        {
            using(SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string serieQuery = "SELECT ps.SeriesID,ps.name AS 'Profil Seri Adı', ps.description AS 'Açıklama', pb.name AS 'Marka' FROM PROFILE_SERIES ps " +
                    "INNER JOIN PROFILES_BRANDS pb ON " +
                    "ps.brandID = pb.ProfileID";
                using(SQLiteCommand cmd = new SQLiteCommand(serieQuery, conn))
                {
                    SQLiteDataAdapter adapt = new SQLiteDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgSeries.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void cmbxFull()
        {
            using(SQLiteConnection conn = new SQLiteConnection(connStr))
            {
                conn.Open();
                string query = "SELECT ProfileID, name FROM PROFILES_BRANDS";

                SQLiteCommand pbcmd = new SQLiteCommand(query, conn);
                SQLiteDataAdapter adpb = new SQLiteDataAdapter(pbcmd);
                DataTable dt = new DataTable();
                adpb.Fill(dt);

                cmbBrand.ItemsSource = dt.DefaultView;
                cmbBrand.DisplayMemberPath = "name";
                cmbBrand.SelectedValuePath = "ProfileID";
            }
        }

        private void dgSerie(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgSeries.SelectedItem is DataRowView row)
                {
                    txtSeriesName.Text = row["Profil Seri Adı"].ToString();
                    txtSeriesDesc.Text = row["Açıklama"].ToString();

                    var selectedBrand = cmbBrand.Items
                        .Cast<DataRowView>()
                        .FirstOrDefault(item => item["name"].ToString() == row["Marka"].ToString());
                    cmbBrand.SelectedItem = selectedBrand;

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
                string insertQuery = "INSERT INTO PROFILE_SERIES (name, description, brandID) " +
                    "VALUES(@name, @description, @brandID)";

                SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn);

                cmd.Parameters.AddWithValue("@name", txtSeriesName.Text);
                cmd.Parameters.AddWithValue("@description", txtSeriesDesc.Text);
                cmd.Parameters.AddWithValue("@brandID", cmbBrand.SelectedValue);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                Clean();
                TableRead();
            }
        }

        private void updateBtn(object sender, RoutedEventArgs e)
        {
            if(dgSeries.SelectedItem is DataRowView row)
            {
                using(SQLiteConnection conn =new SQLiteConnection(connStr))
                {
                    conn.Open();
                    string updateQuery = "UPDATE PROFILE_SERIES SET " +
                        "name = @name," +
                        "description = @description," +
                        "brandID = @brandID " +
                        "WHERE SeriesID = @SeriesID";

                    SQLiteCommand cmd = new SQLiteCommand(updateQuery, conn);

                    cmd.Parameters.AddWithValue("@name", txtSeriesName.Text);
                    cmd.Parameters.AddWithValue("@description", txtSeriesDesc.Text);
                    cmd.Parameters.AddWithValue("@brandID", Convert.ToInt32(cmbBrand.SelectedValue));
                    cmd.Parameters.AddWithValue("@SeriesID", row["SeriesID"]);

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
                if (dgSeries.SelectedItem is DataRowView row)
                {
                    using (SQLiteConnection conn = new SQLiteConnection(connStr))
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM PROFILE_SERIES " +
                            "WHERE SeriesID = @SeriesID";

                        SQLiteCommand cmd = new SQLiteCommand(deleteQuery, conn);

                        cmd.Parameters.AddWithValue("@SeriesID", row["SeriesID"]);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Seçilen profil serisi silindi.");
                        Clean();
                        TableRead();

                    }
                }
            }
        }

        private void Clean()
        {
            txtSeriesName.Text = "";
            txtSeriesDesc.Text = "";
            cmbBrand.SelectedIndex = -1;
        }

    }
}

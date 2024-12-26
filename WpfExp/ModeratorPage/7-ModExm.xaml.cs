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
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace WpfExp.ModeratorPage
{
    /// <summary>
    /// Interaction logic for _7_ModExm.xaml
    /// </summary>
    public partial class _7_ModExm : Window
    {
        private string connStr = @"Server=DESKTOP-F5QQ2O8;Database=OZGENC;Trusted_Connection=True;";
        public ObservableCollection<string> FileList { get; set; }
        public _7_ModExm()
        {
            InitializeComponent();
            FileList = new ObservableCollection<string>();
            lstFiles.ItemsSource = FileList;
            TableRead();
        }

        public void TableRead()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string exmQuery = "SELECT Id, name AS 'Ad', description AS 'Açıklama', filetype AS 'Dosyalar' FROM PROFILE_EXAMPLE";
                using (SqlCommand cmd = new SqlCommand(exmQuery, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgExamples.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void insertBtn(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir dosya seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string selectedFilePath = lstFiles.SelectedItem.ToString();
            string fileType = System.IO.Path.GetFileName(selectedFilePath)?.ToLower();

            int currentUserId = LoginPage.FindUserId.CurrentUserId;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string insertQuery = "INSERT INTO PROFILE_EXAMPLE (name, description, filetype) " +
                    "VALUES(@name, @description, @filetype)";

                SqlCommand cmd = new SqlCommand(insertQuery, conn);

                cmd.Parameters.AddWithValue("@name", txtExmName.Text);
                cmd.Parameters.AddWithValue("@description", txtExmDesc.Text);
                cmd.Parameters.AddWithValue("@filetype", fileType);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Kayıt işlemi başarılı bir şekilde gerçekleştirildi.");
                LogUserProcess.AddLog(currentUserId, $"Örnek eklendi: {txtExmName.Text}");
                Clean();
                TableRead();
            }
        }

        private void Clean()
        {
            txtExmName.Text = "";
            txtExmDesc.Text = "";
            FileList.Clear();
        }

        private void addFileBtn(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Desteklenen Dosyalar (*.xml;*.dxf;*.zip)|*.xml;*.dxf;*.zip" // İzin verilen uzantılar
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    var fileExtension = System.IO.Path.GetExtension(filePath).ToLower(); // Uzantıyı al
                    if (new[] { ".xml", ".dxf", ".zip" }.Contains(fileExtension)) // Uygun uzantıları kontrol et
                    {
                        if (!FileList.Contains(filePath)) // Aynı dosyayı tekrar eklemeyi önle
                        {
                            FileList.Add(filePath);
                        }
                        else
                        {
                            MessageBox.Show($"Dosya zaten listede: {filePath}", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Geçersiz dosya türü: {fileExtension}\nYalnızca xml, dxf, zip dosyalarına izin verilir.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void removeFileBtn(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedItem != null)
            {
                FileList.Remove(lstFiles.SelectedItem.ToString());
            }
            else
            {
                MessageBox.Show("Silmek için bir dosya seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}

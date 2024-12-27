using System;
using System.Collections.Generic;
using System.Data.Sql;
using System.Data;
using System.Linq;
using System.IO;
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
using System.Data.SqlClient;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using WpfExp.LogRecords;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace WpfExp.PageExample
{
    /// <summary>
    /// Interaction logic for CRUDWindowProfieExample.xaml
    /// </summary>
    public partial class CRUDWindowProfieExample : Window
    {
        private string connStr = @"Server=192.168.5.68,1433;Database=OZGENC;User Id=sa; Password=ozgencmachine16;Trusted_Connection=True;";
        public ObservableCollection<string> FileList { get; set; }
        public CRUDWindowProfieExample()
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
                string exmQuery = @"
            SELECT 
                Id, 
                name AS 'Ad', 
                description AS 'Açıklama', 
                filetype AS 'Dosyalar', 
                DATALENGTH(filecontent) / 1024 AS 'ContentSize' 
                FROM PROFILE_EXAMPLE";
                using (SqlCommand cmd = new SqlCommand(exmQuery, conn))
                {
                    SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapt.Fill(dt);

                    dgExamples.ItemsSource = dt.DefaultView;
                }
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

                    // FileList'i temizle ve seçili satırın dosyalarını yükle
                    FileList.Clear();

                    string files = row["Dosyalar"].ToString(); // Dosya bilgilerini al
                    if (!string.IsNullOrEmpty(files))
                    {
                        var fileList = files.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var file in fileList)
                        {
                            FileList.Add(file.Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("HATA!\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void insertBtn(object sender, RoutedEventArgs e)
        {
            if (FileList.Count == 0)
            {
                MessageBox.Show("Lütfen bir dosya seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int currentUserId = LoginPage.FindUserId.CurrentUserId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                foreach (var filePath in FileList)
                {
                    try
                    {
                        string fileName = System.IO.Path.GetFileName(filePath);
                        byte[] fileContent = File.ReadAllBytes(filePath); // Dosya içeriğini oku

                        string insertQuery = "INSERT INTO PROFILE_EXAMPLE (name, description, filetype, filecontent) " +
                                             "VALUES (@name, @description, @filetype, @filecontent)";

                        SqlCommand cmd = new SqlCommand(insertQuery, conn);
                        cmd.Parameters.AddWithValue("@name", txtExmName.Text);
                        cmd.Parameters.AddWithValue("@description", txtExmDesc.Text);
                        cmd.Parameters.AddWithValue("@filetype", fileName);
                        cmd.Parameters.AddWithValue("@filecontent", fileContent);

                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Dosya eklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            MessageBox.Show("Kayıt işlemi tamamlandı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            TableRead();
            Clean();
        }



        private void updateBtn(object sender, RoutedEventArgs e)
        {
            if (!(dgExamples.SelectedItem is DataRowView row)) // Modern 'is not' yerine klasik yazım
            {
                MessageBox.Show("Lütfen bir satır seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int currentUserId = LoginPage.FindUserId.CurrentUserId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                foreach (var filePath in FileList)
                {
                    try
                    {
                        string fileName = System.IO.Path.GetFileName(filePath);
                        byte[] fileContent = File.ReadAllBytes(filePath);

                        string updateQuery = "UPDATE PROFILE_EXAMPLE SET " +
                                             "name = @name, " +
                                             "description = @description, " +
                                             "filetype = @filetype, " +
                                             "filecontent = @filecontent " +
                                             "WHERE Id = @Id";

                        SqlCommand cmd = new SqlCommand(updateQuery, conn);
                        cmd.Parameters.AddWithValue("@name", txtExmName.Text);
                        cmd.Parameters.AddWithValue("@description", txtExmDesc.Text);
                        cmd.Parameters.AddWithValue("@filetype", fileName);
                        cmd.Parameters.AddWithValue("@filecontent", fileContent);
                        cmd.Parameters.AddWithValue("@Id", row["Id"]);

                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Dosya güncellenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            MessageBox.Show("Güncelleme işlemi tamamlandı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            TableRead();
            Clean();
        }



        private void deleteBtn(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Silmek istediğine emin misin?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return; // Kullanıcı "Hayır" dedi, işlem iptal
            }

            int currentUserId = LoginPage.FindUserId.CurrentUserId;

            if (dgExamples.SelectedItem is DataRowView row)
            {
                string deletedName = "";
                string deletedDesc = "";
                string deletedftype = "";

                // Silinecek veriyi al
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT Id, name, description, filetype FROM PROFILE_EXAMPLE WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", row["Id"]);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            deletedName = reader["name"].ToString();
                            deletedDesc = reader["description"].ToString();
                            deletedftype = reader["filetype"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Silinecek veri bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                // Veriyi sil
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string deleteQuery = "DELETE FROM PROFILE_EXAMPLE WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(deleteQuery, conn);
                    cmd.Parameters.AddWithValue("@Id", row["Id"]);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        MessageBox.Show("Kayıt silinemedi. Tekrar deneyin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    MessageBox.Show("Seçilen örnek silindi.");

                    string logMessage = $"Kayıt Silindi: " +
                                        $"Profil Örneği: {deletedName},\n " +
                                        $"Açıklama: {deletedDesc},\n" +
                                        $"Dosya: {deletedftype}";
                    LogUserProcess.AddLog(currentUserId, logMessage);

                    Clean();
                    TableRead();
                }
            }
            else
            {
                MessageBox.Show("Lütfen bir satır seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void Clean()
        {
            txtExmName.Text = "";
            txtExmDesc.Text = "";
            FileList.Clear();
        }

        #region File Upload
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
        #endregion

        private void clearAllFilesBtn(object sender, RoutedEventArgs e)
        {
            if (FileList == null || FileList.Count == 0)
            {
                MessageBox.Show("Listede silinecek dosya yok.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Tüm dosyaları silmek istediğinizden emin misiniz?",
                                                      "Onay",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                FileList.Clear(); // Bağlı koleksiyonu temizle
                MessageBox.Show("Tüm dosyalar silindi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}

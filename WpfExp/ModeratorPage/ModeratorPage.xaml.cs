using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
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
using Path = System.IO.Path;

namespace WpfExp.ModeratorPage
{
    /// <summary>
    /// Interaction logic for ModeratorPage.xaml
    /// </summary>
    public partial class ModeratorPage : Window
    {
        public ICommand OpenFileCommand { get; }
        public ModeratorPage()
        {
            InitializeComponent();
            conn = new Connection(@"Server=192.168.5.68,1433;Database=OZGENC;User Id=sa; Password=ozgencmachine16;Trusted_Connection=True;");
            LoadCountry();
        }

        public int selectedIndex { get; set; }
        private Connection conn;
        //Ülkeleri çeken fonksiyon
        private void LoadCountry()
        {
            List<string> countries = conn.GetCountry();
            ulkeler.ItemsSource = countries;

        }

        private void DataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            var rowData = e.Row.Item as DataRowView;
            if (rowData == null) return;

            try
            {
                int custId = Convert.ToInt32(rowData["CustomerId"]); // Müşteri ID'sini alıyoruz

                // Müşteriye bağlı detayları çekiyoruz
                var brandDetails = conn.DetailBrands(custId);
                var seriesDetails = conn.DetailSeries(custId);
                var exampleDetails = conn.DetailExamples(custId);

                // Detayları listeye ekliyoruz
                var fileDetails = new List<object>();
                foreach (DataRow row in exampleDetails.Rows)
                {
                    // Örnek ID ve diğer dosya detaylarını alıyoruz
                    fileDetails.Add(new
                    {
                        Id = row["Id"], // Bu ID sabit kullanılıyor
                        filetype = row["filetype"],
                        ContentSize = row.Table.Columns.Contains("ContentSize") ? row["ContentSize"] : "0"
                    });
                }

                // Button'un Tag özelliğini ayarlıyoruz
                var detailsElement = e.DetailsElement; // RowDetails içeriği
                if (detailsElement != null)
                {
                    var button = detailsElement.FindName("OpenFileButton") as Button;
                    if (button != null)
                    {
                        // İlk dosyanın ID'sini Tag olarak atıyoruz (örnek)
                        if (fileDetails.Count > 0)
                        {
                            button.Tag = fileDetails[0].GetType().GetProperty("Id")?.GetValue(fileDetails[0]);
                        }
                    }
                }

                var finalContext = new
                {
                    DetailBrands = brandDetails.DefaultView,
                    DetailSeries = seriesDetails.DefaultView,
                    DetailExamples = exampleDetails.DefaultView,
                    FileDetails = fileDetails
                };

                e.DetailsElement.DataContext = finalContext; // DataContext atama
            }
            catch (Exception ex)
            {
                MessageBox.Show($"HATA! Detaylar yüklenirken bir hata oluştu.\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void selectCities(string selectedCountry)
        {
            List<string> cities = conn.GetCities(selectedCountry);
            sehirler.ItemsSource = cities;

        }

        private void countries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbx = sender as ComboBox;
            selectedIndex = cmbx.SelectedIndex;
            if (ulkeler.SelectedItem != null)
            {
                string selectedCountry = ulkeler.SelectedItem.ToString();
                selectCities(selectedCountry);

            }

        }

        private void selectCustomers(string selectedCities)
        {
            List<Customers> customer = conn.GetCustomers(selectedCities);
            musteri.ItemsSource = customer;

            musteri.DisplayMemberPath = "name";
            musteri.SelectedValuePath = "CustomerID";
        }
        private void sehirler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbx = sender as ComboBox;
            selectedIndex = cmbx.SelectedIndex;
            if (cmbx.SelectedItem != null)
            {
                string selectedCities = sehirler.SelectedItem.ToString();
                List<Customers> customerList = conn.GetCustomers(selectedCities);
                musteri.ItemsSource = customerList;


            }

        }

        private void musteri_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = sender as ComboBox;
            if (a.SelectedValue != null)
            {
                int selectedCustomerId = (int)musteri.SelectedValue;
                LoadMachines(selectedCustomerId);
            }
        }

        private void LoadMachines(int selectedCustomerId)
        {
            var machineData = conn.GetMachines(selectedCustomerId);
            dgMakina.ItemsSource = machineData.DefaultView;
        }

        private void dgMakina_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            var DataRow = grid.SelectedItem;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ModButtonPage modPage = new ModButtonPage();
            modPage.Show();
        }

        private void ClosedWindow(object sender, EventArgs e)
        {
            WpfExp.LoginPage.LoginPage loginPage = new LoginPage.LoginPage();
            loginPage.Show();
        }

        private string SaveFileToDesktop(byte[] fileContent, string fileType)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string denemeFolderPath = Path.Combine(desktopPath, "Deneme");

            // "Deneme" klasörünü oluştur
            if (!Directory.Exists(denemeFolderPath))
            {
                Directory.CreateDirectory(denemeFolderPath);
            }

            // Benzersiz dosya adı oluştur
            string uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileType)}_{Guid.NewGuid()}{Path.GetExtension(fileType)}";

            string tempPath = Path.Combine(denemeFolderPath, uniqueFileName);

            // Dosyayı kaydet
            File.WriteAllBytes(tempPath, fileContent);

            return tempPath;
        }


        private void OpenFile(int fileId)
        {
            if (fileId == 0)
            {
                MessageBox.Show("Geçerli bir dosya seçilemedi!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string fileType;
            byte[] fileContent = conn.GetFileContent(fileId, out fileType);

            if (fileContent == null || string.IsNullOrEmpty(fileType))
            {
                MessageBox.Show("Dosya içeriği bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string filePath = SaveFileToDesktop(fileContent, fileType);

            // Dosyayı aç
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button != null && button.Tag != null && int.TryParse(button.Tag.ToString(), out int fileId) && fileId > 0)
                {
                    MessageBox.Show($"Dosya ID: {fileId}");
                    OpenFile(fileId);
                }
                else
                {
                    MessageBox.Show("Geçerli bir dosya seçilemedi veya ID değeri hatalı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

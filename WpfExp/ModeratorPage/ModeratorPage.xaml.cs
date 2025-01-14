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
using WpfExp.MainPage;
using WpfExp.Models;
using Path = System.IO.Path;

namespace WpfExp.ModeratorPage
{
    /// <summary>
    /// Interaction logic for ModeratorPage.xaml
    /// </summary>
    public partial class ModeratorPage : Window
    {
        private readonly Connection _conn;
        private readonly CountryRepo _countryRepo;
        private readonly CityRepo _cityRepo;
        private readonly CustomerRepo _customerRepo;
        private readonly MachineRepo _machineRepo;
        private readonly BrandRepo _brandRepo;
        private readonly SerieRepo _serieRepo;
        private readonly ExampleRepo _exampleRepo;
        private readonly FileRepo _fileRepo;
        public ICommand OpenFileCommand { get; }
        public int selectedIndex { get; set; }
        public ModeratorPage()
        {
            InitializeComponent();
            _conn = new Connection(); // App.config kısmında connection oluşturuldu
            _countryRepo = new CountryRepo(_conn);
            _cityRepo = new CityRepo(_conn);
            _customerRepo = new CustomerRepo(_conn);
            _machineRepo = new MachineRepo(_conn);
            _brandRepo = new BrandRepo(_conn);
            _serieRepo = new SerieRepo(_conn);
            _exampleRepo = new ExampleRepo(_conn);
            _fileRepo = new FileRepo(_conn);
            LoadCountry();
        }


        private void DataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {


            var rowData = e.Row.Item as DataRowView;
            if (rowData == null) return;

            try
            {
                int custId = Convert.ToInt32(rowData["CustomerId"]);


                var brandDetails = _brandRepo.DetailBrands(custId);
                var seriesDetails = _serieRepo.DetailSeries(custId);
                var exampleDetails = _exampleRepo.DetailExamples(custId);

                // Detayları listeye ekliyoruz
                var fileDetails = new List<FileModel>();
                foreach (DataRow row in exampleDetails.Rows)
                {
                    byte[] contentSize = row["ContentSize"] as byte[];
                    // Örnek ID ve diğer dosya detaylarını alıyoruz
                    fileDetails.Add(new FileModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        FileType = row["filetype"].ToString(),
                        ContentSize = contentSize
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
                foreach (DataColumn column in exampleDetails.Columns)
                {
                    Console.WriteLine($"Column Name: {column.ColumnName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"HATA! Detaylar yüklenirken bir hata oluştu.\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region ComboBox ve DataGrid İşlemleri
        private void LoadCountry()
        {
            List<string> countries = _countryRepo.GetCountry();
            ulkeler.ItemsSource = countries;

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

        private void selectCities(string selectedCountry)
        {
            List<string> cities = _cityRepo.GetCities(selectedCountry);
            sehirler.ItemsSource = cities;

        }

        private void sehirler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbx = sender as ComboBox;
            selectedIndex = cmbx.SelectedIndex;
            if (cmbx.SelectedItem != null)
            {
                string selectedCities = sehirler.SelectedItem.ToString();
                List<Customers> customerList = _customerRepo.GetCustomers(selectedCities);
                musteri.ItemsSource = customerList;


            }

        }

        private void selectCustomers(string selectedCities)
        {
            List<Customers> customer = _customerRepo.GetCustomers(selectedCities);
            musteri.ItemsSource = customer;

            musteri.DisplayMemberPath = "name";
            musteri.SelectedValuePath = "CustomerID";
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
            var machineData = _machineRepo.GetMachines(selectedCustomerId);
            dgMakina.ItemsSource = machineData.DefaultView;
        }

        private void dgMakina_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            var DataRow = grid.SelectedItem;
        }
        #endregion

        #region Dosya İşlemleri
        private string SaveFileToDesktop(byte[] fileContent, string fileType, OpenFilePaths paths)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string targetFolder = Path.Combine(desktopPath, "Profiller", paths.Country, paths.City, paths.Customer, paths.Brand);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string fileName = $"{Path.GetFileNameWithoutExtension(fileType)}_{Guid.NewGuid()}{Path.GetExtension(fileType)}";
            string filePath = Path.Combine(targetFolder, fileName);

            File.WriteAllBytes(filePath, fileContent);

            return filePath;
        }

        private void OpenFile(OpenFilePaths paths)
        {
            if (paths == null || paths.FileId <= 0)
            {
                MessageBox.Show("Geçerli bir dosya seçilemedi!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string fileType;
            byte[] fileContent = _fileRepo.GetFileContent(paths.FileId, out fileType);

            if (fileContent == null || string.IsNullOrEmpty(fileType))
            {
                MessageBox.Show("Dosya içeriği bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string filePath = SaveFileToDesktop(fileContent, fileType, paths);

            // Dosyayı aç
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is object dataContext)
            {
                try
                {
                    // FileDetails listesini DataContext'ten çek
                    var fileDetailsProperty = dataContext.GetType().GetProperty("FileDetails");
                    var fileDetails = fileDetailsProperty?.GetValue(dataContext) as IEnumerable<object>;

                    if (fileDetails == null || !fileDetails.Any())
                    {
                        MessageBox.Show("Açılacak dosya bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string country = ulkeler.SelectedItem?.ToString();
                    string city = sehirler.SelectedItem?.ToString();
                    string customer = GetCustomerNameSelectedRow();
                    string brand = GetBrandFromSelectedRow();

                    foreach (var file in fileDetails)
                    {
                        var idProperty = file.GetType().GetProperty("Id");
                        var fileTypeProperty = file.GetType().GetProperty("FileType");
                        var contentProperty = file.GetType().GetProperty("ContentSize");

                        if (idProperty != null && fileTypeProperty != null && contentProperty != null)
                        {
                            int fileId = Convert.ToInt32(idProperty.GetValue(file));
                            string fileType = fileTypeProperty.GetValue(file)?.ToString();
                            byte[] fileContent = contentProperty.GetValue(file) as byte[];

                            if (fileId > 0 && fileContent != null)
                            {
                                var paths = new OpenFilePaths
                                {
                                    FileId = fileId,
                                    Country = country,
                                    City = city,
                                    Customer = customer,
                                    Brand = brand
                                };

                                string filePath = SaveFileToDesktop(fileContent, fileType, paths);
                                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                            }
                            else
                            {
                                MessageBox.Show($"Dosya ID: {fileId} içerik eksik veya geçersiz!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }

                    MessageBox.Show("Tüm dosyalar başarıyla açıldı.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dosyalar açılırken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Geçerli bir dosya bağlamı bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Dosya İşlemleri Yardımcı Metotlar
        private string GetBrandFromSelectedRow()
        {
            if (dgMakina.SelectedItem is DataRowView selectedRow)
            {
                // Seçili satırdaki veriye doğrudan eriş
                if (selectedRow["BrandName"] != DBNull.Value)
                {
                    return selectedRow["BrandName"].ToString(); // Doğrudan "name" kolonunu al
                }
            }
            else
            {
                MessageBox.Show("Bir satır seçilmedi!");
            }
            return "Marka bilgisi bulunamadı!";
        }

        private string GetCustomerNameSelectedRow()
        {
            if (dgMakina.SelectedItem is DataRowView selectedRow)
            {
                // Seçili satırdaki veriye doğrudan eriş
                if (selectedRow["CustomerName"] != DBNull.Value)
                {
                    return selectedRow["CustomerName"].ToString(); // Doğrudan "name" kolonunu al
                }
            }
            else
            {
                MessageBox.Show("Bir satır seçilmedi!");
            }
            return "Müşterinin adı bulunamadı!";
        }
        #endregion

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
    }
}

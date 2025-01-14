using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices.ComTypes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data;
using WpfExp.Models;
using Syncfusion.Windows.Tools.Controls;
using WpfExp.LoginPage;
using System.Diagnostics;
using System.IO;
using System.Data.SqlClient;
using WpfExp.MainPage;
using Path = System.IO.Path;
using System.Collections;
using System.Runtime.InteropServices;


namespace WpfExp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RelayCommand<OpenFilePaths> OpenFileCommand { get; private set; }

        public int selectedIndex { get; set; }

        private readonly Connection _conn;
        private readonly CountryRepo _countryRepo;
        private readonly CityRepo _cityRepo;
        private readonly CustomerRepo _customerRepo;
        private readonly MachineRepo _machineRepo;
        private readonly BrandRepo _brandRepo;
        private readonly SerieRepo _serieRepo;
        private readonly ExampleRepo _exampleRepo;
        private readonly FileRepo _fileRepo;
        private readonly SerieRepoNonCustomer _serieRepoNonCustomer;
        private readonly ExampleRepoNonCustomer _exampleRepoNonCustomer;

        public MainWindow()
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
            _serieRepoNonCustomer = new SerieRepoNonCustomer(_conn);
            _exampleRepoNonCustomer = new ExampleRepoNonCustomer(_conn);

            LoadCountry();

            OpenFileCommand = new RelayCommand<OpenFilePaths>(OpenFile);
            DataContext = this;
            rbMusteriler.IsChecked = true;
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
                    byte[] fileContent = row.Table.Columns.Contains("filecontent") ? row["filecontent"] as byte[] : null;
                    long fileSize = row.Table.Columns.Contains("ContentSize") ? Convert.ToInt64(row["ContentSize"]) : 0;

                    fileDetails.Add(new FileModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        FileType = row["filetype"].ToString(),
                        ContentSize = fileContent, // Dosya içeriği
                        FileSize = fileSize // Dosya boyutu
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

        #region ComboBox ve DataGrid İşlemleri
        private void LoadCountry()
        {
            // Doğrudan ulkeler ComboBox'una erişim sağla
            if (ulkeler != null)
            {
                ulkeler.ItemsSource = _countryRepo.GetCountry();
            }
            else
            {
                MessageBox.Show("Ülkeler ComboBox bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void countries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ulkeler.SelectedItem != null)
            {
                string selectedCountry = ulkeler.SelectedItem.ToString();
                sehirler.ItemsSource = _cityRepo.GetCities(selectedCountry);
            }
        }


        private void selectCities(string selectedCountry)
        {
            try
            {
                // Seçilen ülkeye bağlı şehirleri getir
                List<string> cities = _cityRepo.GetCities(selectedCountry);

                if (cities == null || !cities.Any())
                {
                    MessageBox.Show("Seçilen ülkeye ait şehir bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Şehir verilerini ComboBox’a bağla
                sehirler.ItemsSource = cities;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Şehirler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void sehirler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sehirler.SelectedItem != null)
            {
                string selectedCity = sehirler.SelectedItem.ToString();
                musteri.ItemsSource = _customerRepo.GetCustomers(selectedCity);
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
            if (musteri.SelectedValue != null)
            {
                int selectedCustomerId = Convert.ToInt32(musteri.SelectedValue);
                LoadMachines(selectedCustomerId);
            }
            else
            {
                MessageBox.Show("Lütfen bir müşteri seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadMachines(int customerId)
        {
            try
            {
                // Müşteri ID’sine göre makineleri al
                DataTable machines = _machineRepo.GetMachines(customerId);

                if (machines == null || machines.Rows.Count == 0)
                {
                    MessageBox.Show("Seçilen müşteri için makine bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // DataGrid'e veri bağla
                dgMakina.ItemsSource = machines.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgMakina_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            var DataRow = grid.SelectedItem;
        }

        private void LoadSeriesForBrand(int brandId)
        {
            var seriesList = _serieRepoNonCustomer.GetSeriesByBrand(brandId);
            seriler.ItemsSource = seriesList;
            seriler.DisplayMemberPath = "name"; // Kullanıcıya gösterilecek alan
            seriler.SelectedValuePath = "Id";  // Seçim yapıldığında dönecek alan
        }



        private void LoadProfileExamplesForSeries(int seriesId)
        {
            var repo = new ExampleRepoNonCustomer(new Connection());
            var examplesTable = repo.GetExamplesBySeries(seriesId);

            dgMakina.ItemsSource = examplesTable.DefaultView;
        }

        private void LoadBrands()
        {
            var brandList = _brandRepo.GetAllBrands(); // Marka verilerini getir
            markalar.ItemsSource = brandList;
            markalar.DisplayMemberPath = "name"; // Combobox'ta gösterilecek alan
            markalar.SelectedValuePath = "Id";  // Seçim ID’si
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

        // Markalar için dosya açma işlemleri
        private string SaveFileToDesktopForBrands(byte[] fileContent, string fileType, OpenFilePaths paths)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string targetFolder = Path.Combine(desktopPath, "Profiller", paths.Brand, paths.Series);

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string fileName = $"{Path.GetFileNameWithoutExtension(fileType)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(fileType)}";
            string filePath = Path.Combine(targetFolder, fileName);

            File.WriteAllBytes(filePath, fileContent);

            return filePath;
        }

        private void OpenFileForBrands_Click(object sender, RoutedEventArgs e)
        {
            if (seriler.SelectedValue != null)
            {
                int seriesId = Convert.ToInt32(seriler.SelectedValue);
                var files = _serieRepoNonCustomer.GetFilesBySeriesId(seriesId);

                foreach (var file in files)
                {
                    string filePath = SaveFileToDesktopForBrands(file.ContentSize, file.FileType, new OpenFilePaths
                    {
                        Brand = markalar.Text,
                        Series = seriler.Text
                    });

                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
            }
            else
            {
                MessageBox.Show("Lütfen geçerli bir profil serisi seçin!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region MANSUR YAVAŞ'IN MERCÜ KAZANI

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
            GoButtonsWindow buttonWindow = new GoButtonsWindow();
            buttonWindow.Show();
        }

        private void ClosedWindow(object sender, EventArgs e)
        {
            try
            {
                AdminLoginPanel loginPanel = new AdminLoginPanel();
                loginPanel.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Admin Panel açılırken hata oluştu: {ex.Message}");
            }
        }

        #region Radio Button İşlemleri
        private void RbMusteriler_Checked(object sender, RoutedEventArgs e)
        {
            MusterilerGrid.Visibility = Visibility.Visible; // Müşterilere özel grid görünür
            MarkalarGrid.Visibility = Visibility.Collapsed; // Markalara özel grid gizlenir

            dgMakina.Visibility = Visibility.Visible; // Müşteri DataGrid’i görünür
            dgMarka.Visibility = Visibility.Collapsed; // Marka DataGrid’i gizle
            
        }

        private void RbMarkalar_Checked(object sender, RoutedEventArgs e)
        {
            MusterilerGrid.Visibility = Visibility.Collapsed; // Müşterilere özel grid gizle
            MarkalarGrid.Visibility = Visibility.Visible; // Markalara özel grid göster

            dgMakina.Visibility = Visibility.Collapsed; // Müşteri DataGrid gizle
            dgMarka.Visibility = Visibility.Visible; // Marka DataGrid göster
            dgMarka.ItemsSource = null; 

            LoadBrands(); 
        }

        private void markalar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (markalar.SelectedItem != null)
            {
                var selectedBrand = (ProfileBrands)markalar.SelectedItem;
                Console.WriteLine($"Seçilen Marka ID: {selectedBrand.Id}, Adı: {selectedBrand.name}");

                // Profil serilerini yükle
                LoadSeriesForBrand(selectedBrand.Id);
            }
            else
            {
                MessageBox.Show("Bir marka seçilmedi!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void seriler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (seriler.SelectedItem != null)
            {
                int selectedSeriesId = (int)seriler.SelectedValue; // Seçili Profil Serisi ID
                Console.WriteLine($"Seçilen Profil Serisi ID: {selectedSeriesId}");

                // Profil örneklerini yükle
                LoadExamplesForSeries(selectedSeriesId);
            }
        }


        private void LoadExamplesForSeries(int seriesId)
        {
            var exampleData = _exampleRepoNonCustomer.GetExamplesBySeries(seriesId);
            if (exampleData.Rows.Count == 0)
            {
                MessageBox.Show("Bu profil serisine ait veri bulunamadı!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            dgMarka.ItemsSource = exampleData.DefaultView; // DataGrid’e bağla
        }



        #endregion

    }
}


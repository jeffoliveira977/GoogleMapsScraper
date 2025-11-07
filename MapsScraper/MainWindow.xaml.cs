using CsvHelper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using GoogleMapsScraper;
using Microsoft.Playwright;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MapsScraper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SearchService _service = new(currentSearchId: "1e398cfc-eb7f-465f-b2a4-3537c1447de0");
        private ObservableCollection<PlaceResult> results = [];
        private DispatcherTimer timer = new();
        private int currentProgress;
        private string? uploadedFilePath;

        private static readonly JsonSerializerOptions CachedJsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
       
        public MainWindow()
        {
            InitializeComponent();
            InitializeData();
            this.Loaded += async (s, e) => await IniciarScrapingAsync();
        }

        private async Task IniciarScrapingAsync()
        {
            try
            {
                var scraper = new GoogleMapsScraper.Scraper("lojas de roupas em Porto Alegre");
                var records = await scraper.RunAsync();
                var db = new GoogleMapsScraper.LeadsDatabase();
                await db.SaveRecordsAsync(records);

                var search_term = "lojas de roupas";
                var location = "Porto Alegre";
                var search_id = Guid.NewGuid().ToString();
                var full_term = $"{search_term} em {location}";
                var status = "pending";
                var created_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var data = new Search
                {
                    SearchTerm = search_term,
                    Location = location,
                    FullTerm = full_term,
                    SearchId = search_id,
                    Status = status,
                    CreatedAt = created_at,
                    StartedAt = created_at,
                    TotalLeads = records.Count
                };

                _service.SaveTofile(data);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string jsonPath = $"data/leads_google_maps-{timestamp}.json";
                string csvPath = $"data/leads_google_maps-{timestamp}.csv";

                Directory.CreateDirectory("data");

                await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(records, CachedJsonOptions));

                using var writer = new StreamWriter(csvPath);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteRecords(records);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeData()
        {
            results = [];
            dgResults.ItemsSource = results;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            timer.Tick += Timer_Tick;

            UpdateResultCount();
            
            var searches = _service.GetSearches();

            CardsContainer.ItemsSource = searches;
        }

        private void BtnNewSearch_Click(object sender, RoutedEventArgs e)
        {
            
            modalOverlay.Visibility = Visibility.Visible;
        }

        private void Card_Click(object sender, RoutedEventArgs e)
        {
            itemsTable.Visibility = Visibility.Visible;
            CardsGrid.Visibility = Visibility.Hidden;
        }

        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
       
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Selecione um arquivo de texto"
            };

            if (openDialog.ShowDialog() == true)
            {
                uploadedFilePath = openDialog.FileName;
                txtFileName.Text = $"📄 {System.IO.Path.GetFileName(uploadedFilePath)}";

                try
                {
                   
                    string content = File.ReadAllText(uploadedFilePath);
                    txtModalSearchTerms.Text = content;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao ler arquivo: {ex.Message}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnStartExtraction_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(txtModalSearchTerms.Text) ||
                txtModalSearchTerms.Text.Contains("ex:"))
            {
                MessageBox.Show("Por favor, insira os termos de busca.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtModalLocation.Text) ||
                txtModalLocation.Text.Contains("ex:"))
            {
                MessageBox.Show("Por favor, insira a localização.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

           
            modalOverlay.Visibility = Visibility.Collapsed;
            
            results.Clear();
     
            txtStatus.Text = "Status: Extraindo dados do Google Maps...";
            txtStatus.Foreground = System.Windows.Media.Brushes.Orange;
            
            currentProgress = 0;
            progressBar.Value = 0;
            timer.Start();

           
            SimulateScraping();
        }

        private void SimulateScraping()
        {
            
            var random = new Random();
            var searchTerms = txtModalSearchTerms.Text.Split(new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

            var sampleNames = new[] { "La Pasta", "Il Forno", "Cucina Italiana", "Sapore Italiano",
                "Villa Romana", "Bella Vista", "Trattoria", "Osteria", "Pizzeria Margherita", "Cantina" };
            var sampleStreets = new[] { "Rua Augusta", "Av. Paulista", "Rua Oscar Freire",
                "Alameda Santos", "Rua Haddock Lobo", "Rua da Consolação" };
            var sampleNeighborhoods = new[] { "Consolação", "Jardins", "Pinheiros", "Vila Madalena",
                "Itaim Bibi", "Bela Vista" };

            int resultCount = random.Next(12, 25);

            for (int i = 0; i < resultCount; i++)
            {
                var place = new PlaceResult
                {
                    Name = $"{sampleNames[random.Next(sampleNames.Length)]} {i + 1}",
                    Address = $"{sampleStreets[random.Next(sampleStreets.Length)]}, {random.Next(100, 3000)} - {sampleNeighborhoods[random.Next(sampleNeighborhoods.Length)]}, {txtModalLocation.Text}",
                    Phone = $"(11) {random.Next(2000, 9999)}-{random.Next(1000, 9999)}",
                    Rating = $"⭐ {random.Next(35, 50) / 10.0:0.0}"
                };

                results.Add(place);
            }

            UpdateResultCount();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            currentProgress += 3;
            progressBar.Value = currentProgress;

            if (currentProgress >= 100)
            {
                timer.Stop();
                txtStatus.Text = $"Status: Extração concluída com sucesso! ✓";
                txtStatus.Foreground = System.Windows.Media.Brushes.Green;
                currentProgress = 0;
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (results.Count == 0)
            {
                MessageBox.Show("Não há resultados para exportar!", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv",
                FileName = $"GoogleMaps_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    using (var writer = new StreamWriter(saveDialog.FileName))
                    {
                      
                        writer.WriteLine("Nome,Endereço,Telefone,Avaliação");

                       
                        foreach (var result in results)
                        {
                            writer.WriteLine($"\"{result.Name}\",\"{result.Address}\",\"{result.Phone}\",\"{result.Rating}\"");
                        }
                    }

                    MessageBox.Show($"Dados exportados com sucesso!\n\nArquivo: {saveDialog.FileName}",
                        "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao exportar: {ex.Message}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            if (results.Count > 0)
            {
                var result = MessageBox.Show("Deseja realmente limpar todos os resultados?",
                    "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    results.Clear();
                    progressBar.Value = 0;
                    txtStatus.Text = "Status: Aguardando busca...";
                    txtStatus.Foreground = System.Windows.Media.Brushes.Gray;
                    UpdateResultCount();
                }
            }
        }

        private void UpdateResultCount()
        {
            txtResultCount.Text = $"{results.Count} resultados encontrados";
        }
    }
    public class PlaceResult
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Rating { get; set; }
    }
}
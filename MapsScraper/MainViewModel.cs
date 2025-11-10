using Aspose.Cells;
using Aspose.Cells.Utility;
using CsvHelper;
using DocumentFormat.OpenXml.InkML;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows.Input; // Para comandos (opcional, mas recomendado)
using System.Windows.Threading;

namespace GoogleMapsScraper
{
    // Implementa INotifyPropertyChanged para atualizar a UI quando as propriedades mudam
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Search> SearchLeads { get; set; } = [];

        private readonly SearchService _service = new(currentSearchId: "...");
        private readonly Queue<Search> _searchQueue = new();
        private readonly LeadsDatabase _database = new ();
        public ICommand ViewSearchDetailsCommand { get; }
        public ICommand BackToCardsCommand { get; }
        private bool _isProcessing = false;

        public string SearchTermsInput { get; set; } = "";
        public string LocationInput { get; set; } = "";

        private string _statusText = "Status: Aguardando busca...";

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public ObservableCollection<PlaceResult> ExtractedLeads { get; set; } = [];
        private ObservableCollection<LeadsData> LeadsData { get; set; } = [];

        public MainViewModel()
        {
            ViewSearchDetailsCommand = new RelayCommand(ViewSearchDetails);
            BackToCardsCommand = new RelayCommand(ExecuteBackToCards);
            InitializeData();
            LoadSearches();
        }
        private bool _isTableVisible = false;
        private bool _isCardsVisible = true;

        public bool IsTableVisible
        {
            get => _isTableVisible;
            set { _isTableVisible = value; 

                OnPropertyChanged(); }
        }

        public bool IsCardsVisible
        {
            get => _isCardsVisible;
            set { _isCardsVisible = value;
                OnPropertyChanged(); }
        }
       
        private void InitializeData()
        {

        }

        private void LoadSearches()
        {
            var searches = _service.GetSearches();
 
            SearchLeads.Clear();
            
            foreach (var search in searches)
            {
                SearchLeads.Add(search);
            }
        }

        private void ViewSearchDetails(object? parameter)
        {
            IsTableVisible = true;
            IsCardsVisible = false;

            if (parameter is Search clickedSearch)
            {
                if (clickedSearch.SearchId == null) return;

                var records = _database.GetLeadsBySearchId(clickedSearch.SearchId);
               
                this.ExtractedLeads.Clear();

                foreach (var record in records)
                {
                    this.ExtractedLeads.Add(MapToPlaceResult(record));
                    
                }

                clickedSearch.IsCurrent = true; 

                StatusText = $"Detalhes da busca: {clickedSearch.FullTerm}";
            }
        }
        private void ExecuteBackToCards(object? parameter)
        {
            IsTableVisible = false; 
            IsCardsVisible = true; 
        }
        private static readonly JsonSerializerOptions CachedJsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };


        public void ExportData(string format)
        {
            // 1. Validar e configurar o SaveFileDialog
            if (!TryConfigureSaveDialog(format, out var saveDialog, out var saveFormat))
            {
                // Se o formato for inválido ou o diálogo for cancelado
                return;
            }

            // 2. Mostrar o diálogo e obter o caminho
            if (saveDialog.ShowDialog() == true)
            {
                string filePath = saveDialog.FileName;

                // 3. Executar a exportação específica para o formato escolhido
                try
                {
                    var dataToExport = this.ExtractedLeads.ToList();

                    switch (saveFormat)
                    {
                        case ExportFormat.Excel:
                            ExportToExcel(dataToExport, filePath);
                            break;
                        case ExportFormat.CSV:
                            ExportToCSV(dataToExport, filePath);
                            break;
                        case ExportFormat.JSON:
                            ExportToJSON(dataToExport, filePath);
                            break;
                        default:
                            // Tratar formato não suportado, embora TryConfigureSaveDialog já deva pegar isso.
                            break;
                    }
                    // Opcional: Mostrar mensagem de sucesso (e.g., MessageBox.Show("Exportação concluída!"));
                }
                catch (Exception ex)
                {
                    // Opcional: Tratar e logar erros (e.g., MessageBox.Show($"Erro ao exportar: {ex.Message}"));
                }
            }
        }

        // --- Métodos Auxiliares e Enums ---

        // Enum para mapear a string do formato para um tipo.
        private enum ExportFormat { Excel, CSV, JSON, Unknown }

        /// <summary>
        /// Tenta configurar o SaveFileDialog e retorna o formato de exportação.
        /// </summary>
        private static bool TryConfigureSaveDialog(string format, out Microsoft.Win32.SaveFileDialog dialog, out ExportFormat exportFormat)
        {
            dialog = new Microsoft.Win32.SaveFileDialog();
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            exportFormat = format switch
            {
                "Excel" => ExportFormat.Excel,
                "CSV" => ExportFormat.CSV,
                "PDF" => ExportFormat.JSON,
                "JSON" => ExportFormat.JSON,
                _ => ExportFormat.Unknown
            };

            // Simplificar a configuração do diálogo com switch expression
            (dialog.Filter, dialog.DefaultExt, dialog.FileName) = exportFormat switch
            {
                ExportFormat.Excel => ("XLSX files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                                       ".xlsx",
                                       $"GoogleMaps_Export_{timestamp}.xlsx"),

                ExportFormat.CSV => ("CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                                     ".csv",
                                     $"GoogleMaps_Export_{timestamp}.csv"),

                ExportFormat.JSON => ("JSON files (*.json)|*.json|All files (*.*)|*.*",
                                      ".json",
                                      $"GoogleMaps_Export_{timestamp}.json"),

                _ => (null, null, null) // Retorno para formatos desconhecidos
            };

            return exportFormat != ExportFormat.Unknown;
        }

        /// <summary>
        /// Lógica de exportação para Excel (usando Aspose.Cells).
        /// </summary>
        private static void ExportToExcel<T>(List<T> data, string filePath)
        {
            var workbook = new Aspose.Cells.Workbook();
            var worksheet = workbook.Worksheets[0];

            string jsonString = JsonSerializer.Serialize(data);

            var options = new Aspose.Cells.Utility.JsonLayoutOptions
            {
                ArrayAsTable = true
            };

            Aspose.Cells.Utility.JsonUtility.ImportData(jsonString, worksheet.Cells, 0, 0, options);

            workbook.Save(filePath, Aspose.Cells.SaveFormat.Xlsx);
        }

        /// <summary>
        /// Lógica de exportação para CSV (usando CsvHelper).
        /// </summary>
        private static void ExportToCSV<T>(List<T> data, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(data);
        }

        /// <summary>
        /// Lógica de exportação para JSON.
        /// </summary>
        private static void ExportToJSON<T>(List<T> data, string filePath)
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(data, CachedJsonOptions));
        }

        public void StartSearch(string searchTerm, string location)
        {
           
            var searchId = Guid.NewGuid().ToString();
            var fullTerm = $"{searchTerm} {location}";
            var createdAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");


            var data = new Search
            {
                SearchTerm = searchTerm,
                Location = location,
                FullTerm = fullTerm,
                SearchId = searchId,
                Status = "Waiting",
                CreatedAt = createdAt,
                StartedAt = createdAt,
                TotalLeads = 0
            };

            SearchLeads.Add(data);

            _searchQueue.Enqueue(data);

            TryProcessQueue();
        }

        private async void TryProcessQueue()
        {
            if (_isProcessing) return;

            _isProcessing = true;
            while (_searchQueue.Count > 0)
            {
                var currentSearch = _searchQueue.Dequeue();

                currentSearch.Status = "Running";

                await ProcessSingleSearch(currentSearch);
            }
            _isProcessing = false;
        }

        public static PlaceResult MapToPlaceResult(BusinessRecord record)
        {
            return new PlaceResult
            {
                Name = record.Name,
                Categories = record.Categories,
                Email = record.Email,
                Phone = record.Phone,
                Url = record.Url,
                Domain = record.Domain,
                Facebook = record.Facebook,
                Instagram = record.Instagram,
                Youtube = record.Youtube,
                Tiktok = record.Tiktok,
                Twitter = record.Twitter,
                Cnpj = record.Cnpj,
                Rating = "6"
            };
        }

        private async Task ProcessSingleSearch(Search data)
        {

            try
            {
                var scraper = new GoogleMapsScraper.Scraper($"{data.SearchTerm} {data.Location}");
                var records = await scraper.RunAsync();

                records.ForEach(record => record.SearchId = data.SearchId);

                await _database.SaveRecordsAsync(records);

                data.Status = "Completed";
                data.TotalLeads = records.Count;
                //data.FinishedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"); // Adicione um campo para hora de fim

                _service.SaveTofile(data);

                this.ExtractedLeads.Clear();

                foreach (var record in records)
                {
                    this.ExtractedLeads.Add(MapToPlaceResult(record));
                }
            }
            catch (Exception)
            {
                data.Status = "Failed";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

  
}
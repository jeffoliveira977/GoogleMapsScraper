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
using System.Windows.Input;
using System.Windows.Threading;
using GoogleMapsScraper.Mapper;
using GoogleMapsScraper.Model;
using GoogleMapsScraper.Utils;
using GoogleMapsScraper.Services;

namespace GoogleMapsScraper.ViewModel
{
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<LeadsData> ExtractedLeads { get; set; } = [];

        public MainViewModel()
        {
            ViewSearchDetailsCommand = new GoogleMapsScraper.Utils.RelayCommand(ViewSearchDetails);
            BackToCardsCommand = new GoogleMapsScraper.Utils.RelayCommand(ExecuteBackToCards);
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
               
                ExtractedLeads.Clear();

                foreach (var record in records)
                {
                    ExtractedLeads.Add(DataMapper.MapToLeadsData(record));                 
                }

                StatusText = $"Detalhes da busca: {clickedSearch.FullTerm}";
            }
        }
        private void ExecuteBackToCards(object? parameter)
        {
            IsTableVisible = false; 
            IsCardsVisible = true; 
        }
        public void ExportData(string format)
        {
            ExportService.ExportData(format, [.. this.ExtractedLeads]);
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

        private async Task ProcessSingleSearch(Search data)
        {
            try
            {
                var scraper = new Scraper($"{data.SearchTerm} {data.Location}");
                var records = await scraper.RunAsync();

                records.ForEach(record => record.SearchId = data.SearchId);

                await _database.SaveRecordsAsync(records);

                data.Status = "Completed";
                data.TotalLeads = records.Count;
                //data.FinishedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"); // Adicione um campo para hora de fim

                _service.SaveTofile(data);

                ExtractedLeads.Clear();

                foreach (var record in records)
                {
                    ExtractedLeads.Add(DataMapper.MapToLeadsData(record));
                }
            }
            catch (Exception)
            {
                data.Status = "Failed";
            }
        }
    }
}
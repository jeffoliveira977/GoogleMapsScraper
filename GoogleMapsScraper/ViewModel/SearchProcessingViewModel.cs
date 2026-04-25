using GoogleMapsScraper.Mapper;
using GoogleMapsScraper.Model;
using GoogleMapsScraper.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMapsScraper.ViewModel
{
    public class SearchProcessingViewModel : INotifyPropertyChanged
    {
        private readonly SearchService _service = new();
        private readonly LeadsDatabase _database = new();
        private readonly Queue<Search> _queue = new();
        private bool _isProcessing = false;
        public ObservableCollection<Search> SearchLeads { get; set; } = [];
        private ObservableCollection<LeadsData>? _extractedLeads;

        public ObservableCollection<LeadsData> ExtractedLeads
        {
            get => _extractedLeads ??= [];
            set => Set(ref _extractedLeads, value);
        }
        public SearchProcessingViewModel()
        {
            ExtractedLeads = [];
            LoadSearches();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
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

        public void EnqueueNewSearch(string term, string location)
        {

            var searchId = Guid.NewGuid().ToString();
            var fullTerm = $"{term} {location}";
            var createdAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var data = new Search
            {
                SearchTerm = term,
                Location = location,
                FullTerm = fullTerm,
                SearchId = searchId,
                Status = "Waiting",
                CreatedAt = createdAt,
                StartedAt = createdAt,
                TotalLeads = 0
            };

            SearchLeads.Add(data);
            _queue.Enqueue(data);
            
            TryProcessQueue();
        }

        private async void TryProcessQueue() {
            if (_isProcessing) return;

            _isProcessing = true;

            while (_queue.Count > 0)
            {
                var currentSearch = _queue.Dequeue();
                currentSearch.Status = "Running";
 
                await ProcessSingleSearch(currentSearch);
            }
            _isProcessing = false;
        }

        private async Task ProcessSingleSearch(Search s)
        {
            try
            {
                var scraper = new Scraper($"{s.SearchTerm} {s.Location}");
                var records = await scraper.RunAsync();

                foreach (var record in records)
                {
                    Console.WriteLine($"Record: {record.Name}, {record.Url}, {record.Email}, {record.Phone}");
                }

                records.ForEach(record => record.SearchId = s.SearchId);

                await _database.SaveRecordsAsync(records);

                s.Status = "Completed";
                s.TotalLeads = records.Count;
                //data.FinishedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"); // Adicione um campo para hora de fim

                _service.SaveTofile(s);

                ExtractedLeads.Clear();

                foreach (var record in records)
                {
                    ExtractedLeads.Add(DataMapper.MapToLeadsData(record));
                }
            }
            catch (Exception ex)
            {
                s.Status = "Failed";
                Console.WriteLine($"Erro ao processar pesquisa: {ex.Message}");
            }
        }

        public void AddLeadsToExtracted(IEnumerable<LeadsData> leads)
        {
            if (leads == null) return;

            foreach (var lead in leads)
            {
                ExtractedLeads.Add(lead);
            }
        }
    }

}

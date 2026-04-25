using Aspose.Cells;
using Aspose.Cells.Utility;
using CsvHelper;
using DocumentFormat.OpenXml.InkML;
using GoogleMapsScraper.Mapper;
using GoogleMapsScraper.Model;
using GoogleMapsScraper.Services;
using GoogleMapsScraper.Utils;
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GoogleMapsScraper.ViewModel
{
    public enum PopupStateType
    {
        Info,     // Cor/Ícone Azul (Instalação)
        Success,  // Cor/Ícone Verde
        Error,    // Cor/Ícone Vermelho
        Hidden
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private PopupStateType _currentPopupState = PopupStateType.Hidden;

        public PopupStateType CurrentPopupState
        {
            get => _currentPopupState;
            set
            {
                if (_currentPopupState != value)
                {
                    _currentPopupState = value;
                    OnPropertyChanged(nameof(CurrentPopupState));
                }
            }
        }

        private bool _isPopupVisible = false;

        public bool IsPopupVisible
        {
            get => _isPopupVisible;
            set
            {
                if (_isPopupVisible != value)
                {
                    _isPopupVisible = value;
                    OnPropertyChanged(nameof(IsPopupVisible));
                }
            }
        }

        private bool _isModalVisible = false;
        public bool IsModalVisible
        {
            get => _isModalVisible;
            set
            {
                _isModalVisible = value;
                OnPropertyChanged(nameof(IsModalVisible));
            }
        }

        private string _popupTitle = string.Empty;
        public string PopupTitle
        {
            get => _popupTitle;
            set
            {
                _popupTitle = value;
                OnPropertyChanged(nameof(PopupTitle));
            }
        }

        private string _popupMessage = string.Empty;
        public string PopupMessage
        {
            get => _popupMessage;
            set
            {
                _popupMessage = value;
                OnPropertyChanged(nameof(PopupMessage));
            }
        }

        private bool _isProgressVisible;
        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set
            {
                _isProgressVisible = value;
                OnPropertyChanged(nameof(IsProgressVisible));
            }
        }

        private bool _isButtonVisible;
        public bool IsButtonVisible
        {
            get => _isButtonVisible;
            set
            {
                _isButtonVisible = value;
                OnPropertyChanged(nameof(IsButtonVisible));
            }
        }

        private readonly LeadsDatabase _database = new ();
        public ICommand ViewSearchDetailsCommand { get; }
        public ICommand StartExtractionCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ClearCommand { get; }

        public string SearchTermsInput { get; set; } = "";
        public string LocationInput { get; set; } = "";
        public string SelectedSearchId { get; set; } = "";
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

        public SearchProcessingViewModel SearchProcessingViewModel { get; }
        public MainViewModel()
        {
            SearchProcessingViewModel = new SearchProcessingViewModel();
            ViewSearchDetailsCommand = new RelayCommand(ViewSearchDetails);
            StartExtractionCommand = new RelayCommand(ExecuteStartExtraction, CanExecuteStartExtraction);
            ClearCommand = new RelayCommand(ClearSelectedSearch);
            ExportCommand = new RelayCommand(ExecuteExport);
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

        public void ShowPopupMessage(string title, string message, bool showProgress, bool isButtonVisible, PopupStateType state)
        {
            PopupTitle = title;
            PopupMessage = message;
            IsProgressVisible = showProgress;
            IsButtonVisible = isButtonVisible;
            CurrentPopupState = state;

            IsPopupVisible = true;
        }

        public void HidePopupMessage()
        {
            IsPopupVisible = false;
            IsProgressVisible = false;
            CurrentPopupState = PopupStateType.Hidden;
        }
  

        public void ShowInfoState(string title, string message, bool showProgress = false, bool isButtonVisible = false)
        {
            ShowPopupMessage(title, message, showProgress, isButtonVisible, PopupStateType.Info);
        }

        public void ShowSuccessState(string title, string message)
        {
            ShowPopupMessage(title, message, false, false, PopupStateType.Success);
        }

        public void ShowErrorState(string title, string message)
        {
            ShowPopupMessage(title, message, false, false, PopupStateType.Error);
        }

        public void ClearSelectedSearch(object? parameter)
        {
            if (string.IsNullOrEmpty(SelectedSearchId))
                return;

            _database.DeleteBySearchId(SelectedSearchId);

            SearchProcessingViewModel.ExtractedLeads.Clear();
        }

        private void ExecuteExport(object? parameter)
        {
            var format = parameter?.ToString();
            if (string.IsNullOrWhiteSpace(format))
                return;

            ExportService.ExportData(format, [..SearchProcessingViewModel.ExtractedLeads]);
        }

        private bool CanExecuteStartExtraction(object? parameter)
        {
            bool termsValid = !string.IsNullOrWhiteSpace(SearchTermsInput);
            bool locationValid = !string.IsNullOrWhiteSpace(LocationInput);

            return termsValid && locationValid;
        }

        private void ExecuteStartExtraction(object? parameter)
        {
            IsModalVisible = false;

            SearchProcessingViewModel.EnqueueNewSearch(SearchTermsInput, LocationInput);
        }

        private void ViewSearchDetails(object? parameter)
        {
            IsTableVisible = true;
            IsCardsVisible = false;

            if (parameter is Search search)
            {
                if (search.SearchId == null) return;

                SelectedSearchId = search.SearchId;
                var records = _database.GetLeadsBySearchId(search.SearchId);

                SearchProcessingViewModel.ExtractedLeads.Clear();

                var mappedLeads = records.Select(record => DataMapper.MapToLeadsData(record)).ToList();
                SearchProcessingViewModel.AddLeadsToExtracted(mappedLeads);

                Console.WriteLine(SearchProcessingViewModel.ExtractedLeads);
                StatusText = $"Detalhes da busca: {search.FullTerm}";
            }
        }
    }
}
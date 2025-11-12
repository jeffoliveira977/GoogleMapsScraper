using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;

namespace GoogleMapsScraper.Model
{
    public class Search : INotifyPropertyChanged
    {
        private string? _status;
        private int _total_leads;

        [JsonPropertyName("search_term")]
        public string? SearchTerm { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("full_term")]
        public string? FullTerm { get; set; }

        [JsonPropertyName("search_id")]
        public string? SearchId { get; set; }


        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("started_at")]
        public string? StartedAt { get; set; }

        [JsonPropertyName("total_leads")]
        public int TotalLeads { 
            get => _total_leads; 
            set
            {
                if (_total_leads != value)
                {
                    _total_leads = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonPropertyName("queue_position")]
        public int QueuePosition { get; set; }

        [JsonPropertyName("status")]
        public string? Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
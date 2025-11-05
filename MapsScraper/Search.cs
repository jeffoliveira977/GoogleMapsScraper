using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MapsScraper
{
    internal class Search
    {
        [JsonPropertyName("search_term")]
        public string SearchTerm { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("full_term")]
        public string FullTerm { get; set; }

        [JsonPropertyName("search_id")]
        public string SearchId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("started_at")]
        public string StartedAt { get; set; }

        [JsonPropertyName("total_leads")]
        public int TotalLeads { get; set; }

        [JsonPropertyName("queue_position")]
        public int QueuePosition { get; set; }

        public bool IsCurrent { get; set; }
    }
}

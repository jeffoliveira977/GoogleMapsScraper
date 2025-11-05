using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MapsScraper
{
    internal class SearchService(string? currentSearchId = null)
    {
        private readonly string _filePath = "data/searchs.json";
        private readonly string _currentSearchId = currentSearchId?? "";

        public List<Search> GetSearches()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return [];

                if (new FileInfo(_filePath).Length == 0)
                    return [];

                string json = File.ReadAllText(_filePath);
                var searches = JsonSerializer.Deserialize<List<Search>>(json);

                if (searches == null)
                    return [];

                foreach (var search in searches)
                    search.IsCurrent = search.SearchId == _currentSearchId;

                return [.. searches.OrderByDescending(s => ParseDate(s.CreatedAt))];
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Erro ao decodificar JSON: {ex.Message}");
                return [];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter pesquisas: {ex.Message}");
                return [];
            }
        }

        private static DateTime ParseDate(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return DateTime.MinValue;

            if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            return DateTime.MinValue;
        }
    }
}

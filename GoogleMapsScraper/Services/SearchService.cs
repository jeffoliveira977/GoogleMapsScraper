using GoogleMapsScraper.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GoogleMapsScraper.Services
{
    internal class SearchService(string? currentSearchId = null)
    {
        private readonly string _filePath = "data/searchs.json";
        private readonly string _currentSearchId = currentSearchId?? "";
        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            WriteIndented = true
        };

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

        public void SaveTofile(Search searches)
        {

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

                var data = new List<Search>();

                if (File.Exists(_filePath) && new FileInfo(_filePath).Length > 0)
                {
                    string text = File.ReadAllText(_filePath);
                    var existing = JsonSerializer.Deserialize<List<Search>>(text);

                    if (existing != null)
                        data = existing;
                }

                data.Add(searches);     

                string json = JsonSerializer.Serialize(data, jsonSerializerOptions);
                File.WriteAllText(_filePath, json);

            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Erro ao decodificar JSON: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter pesquisas: {ex.Message}");
                return;
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

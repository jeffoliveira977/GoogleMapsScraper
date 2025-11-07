using CsvHelper;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using static GoogleMapsScraper.BusinessRecord;

namespace GoogleMapsScraper
{
    public class Scraper(string query)
    {
        private readonly string _query = query;
        private IBrowser? _browser;
        private IPage? _page;
        private readonly List<BusinessRecord> _records = [];

        public async Task<List<BusinessRecord>> RunAsync()
        {
            using var playwright = await Playwright.CreateAsync();
            _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args =
                [
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-dev-shm-usage",
                "--disable-gpu"
            ]
            });

            var locale = CultureInfo.CurrentCulture.Name;

            var windowsId = TimeZoneInfo.Local.Id;
            if (!TimeZoneInfo.TryConvertWindowsIdToIanaId(windowsId, out var ianaId))
            {
                ianaId = "UTC";
            }

            var context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                            "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                Locale = locale,
                TimezoneId = ianaId,
            });

            _page = await context.NewPageAsync();

            _page.Response += async (_, response) =>
            {
                await HandleResponseAsync(response);
            };

            await NavigateToMapsAsync();
            await ScrollAndLoadResultsAsync();

            await _browser.CloseAsync();

            using var semaphore = new SemaphoreSlim(20);
            var validRecords = _records
                .Where(r => !string.IsNullOrWhiteSpace(r.Url) && !string.IsNullOrWhiteSpace(r.Domain))
                .ToList();

            Console.WriteLine($"Processando {validRecords.Count} registros com máximo de 20 simultâneos...");

            var tasks = validRecords.Select(record => ProcessRecordAsync(record, semaphore));
            await Task.WhenAll(tasks);

            Console.WriteLine("Processamento concluído!");

            return _records;
        }
        async static Task ProcessRecordAsync(BusinessRecord record, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                var data = await ContactExtractor.ExtractContactsUrl(record.Url!, record.Domain!);

                record.Cnpj = GetValue(data, "cnpj");
                record.Email = GetValue(data, "email");
                record.Facebook = GetValue(data, "facebook");
                record.Instagram = GetValue(data, "instagram");
                record.Linkedin = GetValue(data, "linkedin");
                record.Twitter = GetValue(data, "twitter");
                record.Youtube = GetValue(data, "youtube");

                if (!string.IsNullOrWhiteSpace(record.Email))
                    Console.WriteLine($"✅ {record.Name}: {record.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{record.Name ?? record.Url}: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }

        static string GetValue(Dictionary<string, object> dict, string key)
        {
            return dict.TryGetValue(key, out var value) ? value?.ToString() ?? "" : "";
        }

        private async Task NavigateToMapsAsync()
        {
            if (_page == null) return;

            string encodedQuery = Uri.EscapeDataString(_query);
            string url = $"https://www.google.com/maps/search/{encodedQuery}";
            await _page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            await _page.WaitForTimeoutAsync(1000);
        }

        private async Task HandleResponseAsync(IResponse response)
        {
            var url = response.Url;
            if (url.Contains("search?tbm=map"))
            {
                try
                {
                    string data = await response.TextAsync();
                    var parsedJson = JsonSerializer.Deserialize<JsonElement>(data[..^6]);
                    var d = parsedJson.GetProperty("d").GetString();

                    if (d == null) return;

                    var recordsData = JsonSerializer.Deserialize<JsonElement>(d[4..]);

                    var newRecords = ExtractBusiness(recordsData[64]);
                    _records.AddRange(newRecords);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar resposta: {ex.Message}");
                }
            }
        }

        private async Task ScrollAndLoadResultsAsync()
        {
            if (_page == null) return;

            int scrollAttempts = 0;
            int maxScrolls = 20;
            double previousHeight = 0;

            while (scrollAttempts < maxScrolls)
            {
                var height = await _page.EvaluateAsync<double>(@"
                () => {
                    const pane = document.getElementById('pane');
                    const currentFeed = pane?.nextElementSibling?.querySelector('div[role=""feed""]');
                    if (currentFeed) {
                        currentFeed.scrollTo({ top: currentFeed.scrollHeight, behavior: 'smooth' });
                        return currentFeed.scrollHeight;
                    }
                    window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' });
                    return document.body.scrollHeight;
                }");

                if (height == previousHeight)
                    break;

                previousHeight = height;
                scrollAttempts++;
                Console.WriteLine($"Rolando... tentativa {scrollAttempts}");
                await _page.WaitForTimeoutAsync(3000);
            }
        }

        public static List<BusinessRecord> ExtractBusiness(JsonElement rawData)
        {
            if (rawData.ValueKind != JsonValueKind.Array)
            {
                Console.WriteLine("Invalid data format for business extraction.");
                return [];
            }

            var extractedData = new List<BusinessRecord>();

            foreach (var entry in rawData.EnumerateArray())
            {
                try
                {
                    var record = new BusinessRecord(entry);
                    if (record.IsValid())
                    {
                        extractedData.Add(record);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }

            return extractedData;
        }

        public async Task StopAsync()
        {
            if (_page != null) await _page.CloseAsync();
            if (_browser != null) await _browser.CloseAsync();
        }
    }

}

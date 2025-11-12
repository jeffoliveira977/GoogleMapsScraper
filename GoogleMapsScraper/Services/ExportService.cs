using Aspose.Cells;
using Aspose.Cells.Utility;
using CsvHelper;
using GoogleMapsScraper.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace GoogleMapsScraper.Services
{
    internal class ExportService
    {
        private static readonly JsonSerializerOptions CachedJsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };


        public static void ExportData(string format, List<LeadsData> leadsDatas)
        {
            if (!TryConfigureSaveDialog(format, out var saveDialog, out var saveFormat))
            {
                return;
            }

            if (saveDialog.ShowDialog() == true)
            {
                string filePath = saveDialog.FileName;

                try
                {
                    switch (saveFormat)
                    {
                        case ExportFormat.Excel:
                            ExportToExcel(leadsDatas, filePath);
                            break;
                        case ExportFormat.CSV:
                            ExportToCSV(leadsDatas, filePath);
                            break;
                        case ExportFormat.JSON:
                            ExportToJSON(leadsDatas, filePath);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private enum ExportFormat { Excel, CSV, JSON, Unknown }

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

                _ => (null, null, null)
            };

            return exportFormat != ExportFormat.Unknown;
        }

        private static void ExportToExcel<T>(List<T> data, string filePath)
        {
            var workbook = new Workbook();
            var worksheet = workbook.Worksheets[0];

            string jsonString = JsonSerializer.Serialize(data);

            var options = new JsonLayoutOptions
            {
                ArrayAsTable = true
            };

            JsonUtility.ImportData(jsonString, worksheet.Cells, 0, 0, options);

            workbook.Save(filePath, SaveFormat.Xlsx);
        }

        private static void ExportToCSV<T>(List<T> data, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(data);
        }

        private static void ExportToJSON<T>(List<T> data, string filePath)
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(data, CachedJsonOptions));
        }

    }
}

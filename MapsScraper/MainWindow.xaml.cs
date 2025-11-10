using CsvHelper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using GoogleMapsScraper;
using Microsoft.Playwright;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MapsScraper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string? uploadedFilePath;
        public MainViewModel ViewModel { get; set; }
       
        public MainWindow()
        {
            string playwrightPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ms-playwright");
            Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", playwrightPath);
            
            InitializeComponent();
            ViewModel = new GoogleMapsScraper.MainViewModel();
            this.DataContext = ViewModel;
        }
        
        public static void CopyFolder(string sourcePath, string destinationPath)
        {
            Directory.CreateDirectory(destinationPath);
        
            foreach (string file in Directory.GetFiles(sourcePath))
            {
                File.Copy(file, System.IO.Path.Combine(destinationPath, System.IO.Path.GetFileName(file)), true);
            }
        
            foreach (string subDirectory in Directory.GetDirectories(sourcePath))
            {
                CopyFolder(subDirectory, System.IO.Path.Combine(destinationPath, System.IO.Path.GetFileName(subDirectory)));
            }
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string rawUrl = e.Uri.OriginalString;
            string finalUrl;

            if (rawUrl.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
                rawUrl.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
            {
                finalUrl = rawUrl;
            }
            else if (!rawUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                     !rawUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                finalUrl = $"http://{rawUrl}";
            }
            else
            {
                finalUrl = rawUrl;
            }

            try
            {
                Process.Start(new ProcessStartInfo(finalUrl) { UseShellExecute = true });

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao abrir link ({finalUrl}): {ex.Message}");
            }
        }

        private void BtnNewSearch_Click(object sender, RoutedEventArgs e)
        {
            
            modalOverlay.Visibility = Visibility.Visible;
           
        }

        private void ShowExportMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu is ContextMenu menu)
            {
                menu.PlacementTarget = button;

                menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;

                menu.IsOpen = true;
            }
        }

        private void MenuItemExport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string? format = menuItem.Tag?.ToString();

                MessageBox.Show($"Iniciando exportação para {format}...", "Exportar");
                ViewModel.ExportData(format);
            }
        }


        private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
        {
       
            modalOverlay.Visibility = Visibility.Collapsed;
        }

        private void BtnUploadFile_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Selecione um arquivo de texto"
            };

            if (openDialog.ShowDialog() == true)
            {
                uploadedFilePath = openDialog.FileName;
                txtFileName.Text = $"📄 {System.IO.Path.GetFileName(uploadedFilePath)}";

                try
                {
                   
                    string content = File.ReadAllText(uploadedFilePath);
                    txtModalSearchTerms.Text = content;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao ler arquivo: {ex.Message}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnStartExtraction_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(txtModalSearchTerms.Text) ||
                txtModalSearchTerms.Text.Contains("ex:"))
            {
                MessageBox.Show("Por favor, insira os termos de busca.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtModalLocation.Text) ||
                txtModalLocation.Text.Contains("ex:"))
            {
                MessageBox.Show("Por favor, insira a localização.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

           
            modalOverlay.Visibility = Visibility.Collapsed;
  
            ViewModel.StartSearch(txtModalSearchTerms.Text, txtModalLocation.Text);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
       
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv",
                FileName = $"GoogleMaps_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    using (var writer = new StreamWriter(saveDialog.FileName))
                    {
                      
                        writer.WriteLine("Nome,Endereço,Telefone,Avaliação");

                       
                       // foreach (var result in results)
                        //{
                         //   writer.WriteLine($"\"{result.Name}\",\"{result.Address}\",\"{result.Phone}\",\"{result.Rating}\"");
                        //}
                    }

                    MessageBox.Show($"Dados exportados com sucesso!\n\nArquivo: {saveDialog.FileName}",
                        "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao exportar: {ex.Message}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void UpdateResultCount()
        {
            
        }
    }
}

using System;
using System.IO;
using CsvHelper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Playwright;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using GoogleMapsScraper.ViewModel;

namespace GoogleMapsScraper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event EventHandler? OkClicked;
        private string? uploadedFilePath;
        public MainViewModel ViewModel { get; set; }

        public MainWindow()
        {
           
            InitializeComponent();

            ViewModel = new GoogleMapsScraper.ViewModel.MainViewModel();
            this.DataContext = ViewModel;

            _ = InitializePlaywrightAsync();
        }

        private async Task InitializePlaywrightAsync()
        {

            if (IsPlaywrightInstalled())
            {
                return;
            }

            // Mostrar popup de início da instalação
            ShowPopupInfo(
                "Instalação Necessária",
                "Iniciando a instalação do Playwright. A aplicação continuará quando o processo for concluído. Isso pode levar alguns minutos.",
                showProgress: true
            );

            // Executar instalação
            await RunPlaywrightInstallAsync();

            // Verificar resultado
            if (IsPlaywrightInstalled())
            {
                ShowPopupSuccess(
                    "Instalação Concluída",
                    "O Playwright foi instalado com sucesso. A aplicação está pronta para uso."
                );
            }
            else
            {
                ShowPopupError(
                    "Erro na Instalação",
                    "A instalação falhou. Verifique os logs do PowerShell e tente novamente."
                );
            }
        }
        public static bool IsPlaywrightInstalled()
        {
            string playwrightRootPath =
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ms-playwright");

            const string executableName = "headless_shell.exe";

            try
            {
                return Directory.EnumerateFiles(playwrightRootPath, executableName, SearchOption.AllDirectories).Any();
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Mostrar popup com informação (azul)
        private void ShowPopupInfo(string title, string message, bool showProgress = false)
        {
            PopupTitleText.Text = title;
            PopupMessageText.Text = message;
            PopupProgressBar.Visibility = showProgress ? Visibility.Visible : Visibility.Collapsed;
            PopupOkButton.Visibility = showProgress ? Visibility.Collapsed : Visibility.Visible;

            // Ícone azul de informação
            PopupIconPath.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(59, 130, 246));
            PopupIconPath.Data = Geometry.Parse("M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4M11,9V11H13V9H11M11,13V17H13V13H11Z");
            IconBackground.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 249, 255));

            if (showProgress)
            {
                StartPopupSpinAnimation();
            }

            InstallPopup.Visibility = Visibility.Visible;
        }

        // Mostrar popup de sucesso (verde)
        private void ShowPopupSuccess(string title, string message)
        {
            PopupTitleText.Text = title;
            PopupMessageText.Text = message;
            PopupProgressBar.Visibility = Visibility.Collapsed;
            PopupOkButton.Visibility = Visibility.Visible;

            // Ícone verde de sucesso
            PopupIconPath.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 197, 94));
            PopupIconPath.Data = Geometry.Parse("M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12A8,8 0 0,0 12,4M11,16.5L6.5,12L7.91,10.59L11,13.67L16.59,8.09L18,9.5L11,16.5Z");
            IconBackground.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 253, 244));

            StopPopupSpinAnimation();
            InstallPopup.Visibility = Visibility.Visible;
        }

        // Mostrar popup de erro (vermelho)
        private void ShowPopupError(string title, string message)
        {
            PopupTitleText.Text = title;
            PopupMessageText.Text = message;
            PopupProgressBar.Visibility = Visibility.Collapsed;
            PopupOkButton.Visibility = Visibility.Visible;

            // Ícone vermelho de erro
            PopupIconPath.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(239, 68, 68));
            PopupIconPath.Data = Geometry.Parse("M12,2C17.53,2 22,6.47 22,12C22,17.53 17.53,22 12,22C6.47,22 2,17.53 2,12C2,6.47 6.47,2 12,2M15.59,7L12,10.59L8.41,7L7,8.41L10.59,12L7,15.59L8.41,17L12,13.41L15.59,17L17,15.59L13.41,12L17,8.41L15.59,7Z");
            IconBackground.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(254, 242, 242));

            StopPopupSpinAnimation();
            InstallPopup.Visibility = Visibility.Visible;
        }

        // Iniciar animação de rotação
        private void StartPopupSpinAnimation()
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever
            };
            PopupIconRotation.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        // Parar animação de rotação
        private void StopPopupSpinAnimation()
        {
            PopupIconRotation.BeginAnimation(RotateTransform.AngleProperty, null);
            PopupIconRotation.Angle = 0;
        }

        private void PopupOkButton_Click(object sender, RoutedEventArgs e)
        {
            InstallPopup.Visibility = Visibility.Collapsed;
        }

        private static Task RunPlaywrightInstallAsync()
        {

            return Task.Run(() =>
            {
                string playwrightScriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "playwright.ps1");
                if (!File.Exists(playwrightScriptPath))
                {
                    throw new FileNotFoundException($"Script não encontrado: {playwrightScriptPath}");
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{playwrightScriptPath}\" install",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };

                using var process = Process.Start(startInfo) ?? throw new Exception($"Erro ao executar o powershell.");

                TimeSpan totalTimeout = TimeSpan.FromMinutes(10);
                TimeSpan checkInterval = TimeSpan.FromSeconds(5);
                DateTime startTime = DateTime.Now;
                bool success = false;

                while (DateTime.Now - startTime < totalTimeout)
                {
                    if (process.HasExited)
                    {
                        break;
                    }

                    Thread.Sleep(checkInterval);
                }

                if (success && !process.HasExited)
                {
                    process.Kill(true);
                    Console.WriteLine("Instalação verificada. Processo PowerShell encerrado.");
                    return;
                }

                process.WaitForExit(5000);

            });
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            OkClicked?.Invoke(this, EventArgs.Empty);
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
        
                ViewModel.ExportData(format?? "");
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

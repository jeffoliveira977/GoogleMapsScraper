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
       
        public MainViewModel ViewModel { get; set; }

        public MainWindow()
        {
           
            InitializeComponent();


            ViewModel = new MainViewModel();
            this.DataContext = ViewModel;

            PlaywrightPopupControl.DataContext = ViewModel;
            SearchParametersModalControl.DataContext = ViewModel;
            LeadsDataGridView.DataContext = ViewModel;

            _ = InitializePlaywrightAsync();
        }

        private async Task InitializePlaywrightAsync()
        {

            if (IsPlaywrightInstalled())
            {
                return;
            }

            // Mostrar popup de início da instalação
            ViewModel.ShowInfoState(
                "Instalação Necessária",
                "Iniciando a instalação do Playwright. A aplicação continuará quando o processo for concluído. Isso pode levar alguns minutos.",
                showProgress: true
            );

            // Executar instalação
            await RunPlaywrightInstallAsync();

            // Verificar resultado
            if (IsPlaywrightInstalled())
            {
                ViewModel.ShowSuccessState(
                    "Instalação Concluída",
                    "O Playwright foi instalado com sucesso. A aplicação está pronta para uso."
                );
            }
            else
            {
                ViewModel.ShowErrorState(
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

        private void OpenModal_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.IsModalVisible = true;
        }

        /*private void BtnUploadFile_Click(object sender, RoutedEventArgs e)
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
        }*/


    }
}

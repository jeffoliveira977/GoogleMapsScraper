using GoogleMapsScraper.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GoogleMapsScraper.Controls
{
    /// <summary>
    /// Interação lógica para SearchParametersModal.xam
    /// </summary>
    public partial class SearchBusinessPopup : UserControl
    {
        private string? uploadedFilePath;
        public SearchBusinessPopup()
        {
            InitializeComponent();
        }
        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.IsModalVisible = false;
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
                    string[] words = content.Split(';');
                    foreach (string word in words)
                    {
                        Console.WriteLine(word);
                        if (DataContext is MainViewModel vm)
                        {
                            vm.SearchProcessingViewModel.EnqueueNewSearch(word, "");
                        }
                    }
                    txtModalSearchTerms.Text = content;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao ler arquivo: {ex.Message}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}

using GoogleMapsScraper.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace GoogleMapsScraper.View
{
    /// <summary>
    /// Interação lógica para LeadsDataView.xam
    /// </summary>
    public partial class LeadsDataGrid : UserControl
    {
        public LeadsDataGrid()
        {
            InitializeComponent();
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
        private void BackToCards_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.IsTableVisible = false;
                vm.IsCardsVisible = true;
            }
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

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}

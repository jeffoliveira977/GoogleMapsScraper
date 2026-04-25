using GoogleMapsScraper.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace GoogleMapsScraper.Controls
{
    /// <summary>
    /// Interação lógica para PlaywrightInstallPopup.xam
    /// </summary>
    public partial class PlaywrightInstallPopup : UserControl
    {
        public PlaywrightInstallPopup()
        {
            InitializeComponent();
        }

        private void PopupOkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.HidePopupMessage();
            }
        }

    

    }
}

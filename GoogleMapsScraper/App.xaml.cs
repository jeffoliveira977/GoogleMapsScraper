using System.Configuration;
using System.Data;
using System.Windows;
using Wpf.Ui.Appearance;

namespace GoogleMapsScraper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Aplica o tema dark e mantém em sincronia com o Windows
            ApplicationThemeManager.Apply(ApplicationTheme.Dark);
        }
    }

}

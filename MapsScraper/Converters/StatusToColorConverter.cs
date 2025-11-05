using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MapsScraper.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString().ToLower() == "failed")
                return new SolidColorBrush(Color.FromRgb(220, 38, 38));
            else
                return new SolidColorBrush(Color.FromRgb(34, 197, 94)); 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
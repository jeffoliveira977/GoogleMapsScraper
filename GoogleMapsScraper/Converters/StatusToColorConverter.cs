using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GoogleMapsScraper.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var status = value?.ToString()?.ToLowerInvariant();

            string hexColor = status switch
            {
                "failed" => "#DC2626", // red
                "completed" => "#22C55E", // green
                "running" => "#2C2D42", // gray
                "waiting" => "#EAB308", // yellow
                _ => "#6B7280"  // light gray
            };

            Color color = (Color)ColorConverter.ConvertFromString(hexColor);

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
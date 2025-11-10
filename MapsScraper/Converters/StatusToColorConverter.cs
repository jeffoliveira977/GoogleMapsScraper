using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GoogleMapsScraper.Converters
{
    // O conversor mapeia o status (string) para a cor (SolidColorBrush)
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Converte o valor de entrada (Status) para string minúscula
            var status = value?.ToString().ToLower();

            Color color;

            if (status == "failed")
            {
                color = Color.FromRgb(0xDC, 0x26, 0x26);
            }
            else if (status == "completed")
            {
                color = Color.FromRgb(0x22, 0xC5, 0x5E);
            }
            else if (status == "running")
            {
                color = Color.FromArgb(0xFF, 0x2C, 0x2D, 0x42);
            }
            else if (status == "waiting")
            {
  
                color = Color.FromRgb(0xEA, 0xB3, 0x08);
            }
            else
            {
                color = Color.FromRgb(0x6B, 0x72, 0x80);
            }

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
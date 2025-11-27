using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Massiv.Services
{
    public class HexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorHex && !string.IsNullOrEmpty(colorHex))
            {
                try
                {
                    // Используем системный ColorConverter для преобразования строки в Color
                    var color = (Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    // Возвращаем прозрачный цвет по умолчанию в случае ошибки
                    return new SolidColorBrush(Colors.Transparent);
                }
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

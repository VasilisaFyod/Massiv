using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Massiv.Services
{
    public class PercentageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal percentage)
            {
                return percentage switch
                {
                    >= 90 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#008000")),
                    >= 80 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#85ccfa")),
                    >= 70 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffa500")), 
                    >= 60 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B00")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff0000"))     
                };
            }

            if (value is double doublePercentage)
            {
                return doublePercentage switch
                {
                    >= 90 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#008000")),
                    >= 80 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#85ccfa")),
                    >= 70 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffa500")),
                    >= 60 => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B00")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff0000"))
                };
            }

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080")); 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

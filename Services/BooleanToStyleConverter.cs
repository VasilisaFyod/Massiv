using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Massiv.Services
{
    public class BooleanToStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive && parameter is string param)
            {
                var styles = param.Split('|');
                if (styles.Length == 2)
                {
                    var activeStyle = Application.Current.TryFindResource(styles[0]);
                    var inactiveStyle = Application.Current.TryFindResource(styles[1]);

                    return isActive ? activeStyle : inactiveStyle;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

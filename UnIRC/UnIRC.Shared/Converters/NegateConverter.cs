using System;
using System.Globalization;
using System.Windows;
#if DESKTOP
using System.Windows.Data;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif

namespace UnIRC.Converters
{
    public class NegateConverter : IValueConverter
    {
#if DESKTOP
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string culture)
#endif
        {
            if (value is Visibility)
            {
                return ((Visibility)value == Visibility.Visible)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }

            var nullabool = value as bool?;
            return nullabool.HasValue && !nullabool.Value;
        }

#if DESKTOP
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
#endif
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}

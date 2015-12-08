using System;
using System.Globalization;
#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#else
using System.Windows.Data;
#endif

namespace UnIRC.Converters
{
    public class EmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
#if WINDOWS_UWP
            string language)
#else
            CultureInfo culture)
#endif
        {
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
#if WINDOWS_UWP
            string language)
#else
            CultureInfo culture)
#endif
        {
            return "";
        }
    }
}

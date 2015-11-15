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
    public class IsTrueVisibilityConverter : IValueConverter
    {
#if DESKTOP
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string culture)
#endif
        {
            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic) return Visibility.Visible;

            if (!(value is bool)) return Visibility.Collapsed;
            return ((bool)value)
                       ? Visibility.Visible
                       : Visibility.Collapsed;
        }

#if DESKTOP
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object ConvertBack(object value, Type targetType, object parameter, string culture)
#endif
        {
            throw new NotImplementedException();
        }
    }
}

using System;
#if DESKTOP
using System.Globalization;
using System.Windows;
using System.Windows.Data;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif

namespace UnIRC.Converters
{

    public class IsNotNullVisibilityConverter : IValueConverter
    {
#if DESKTOP
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string culture)
#endif
        {
            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic) return Visibility.Visible;

            return (value != null) ? Visibility.Visible : Visibility.Collapsed;
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

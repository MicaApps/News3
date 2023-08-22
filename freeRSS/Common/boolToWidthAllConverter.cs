using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace freeRSS.Common
{
    public class boolToWidthAllConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

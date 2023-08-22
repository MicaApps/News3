using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace freeRSS.Common
{
    public class BooleanToNullableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Boolean result = true;
            if ((Nullable<Boolean>)value == false || (Nullable<Boolean>)value == null) 
                result = false;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (Boolean)value;
        }
    }
}

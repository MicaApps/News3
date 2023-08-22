using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace freeRSS.Common
{
    public class DateTimeOffSetStringToDateTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string sourceTimeString = (string)value;
            string pattern = "(\\d+)/(\\d+)/(\\d+) (\\d+):(\\d+):(\\d+) ([^:]+):(\\d+)";

            DateTimeOffset targetTimeOffset;
            DateTime targetTime;

            string targetUIString = sourceTimeString;

            //正则匹配
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matchStrings = regex.Matches(sourceTimeString);


            // 取子表达式
            int Year, Month, Day, Hour, Minute, Second, OffsetHour, OffsetMin;
            TimeSpan Offset;

            if (matchStrings.Count > 0)
            {
                Year = int.Parse(matchStrings[0].Groups[1].Value);
                Month = int.Parse(matchStrings[0].Groups[2].Value);
                Day = int.Parse(matchStrings[0].Groups[3].Value);
                Hour = int.Parse(matchStrings[0].Groups[4].Value);
                Minute =  int.Parse(matchStrings[0].Groups[5].Value);
                Second = int.Parse(matchStrings[0].Groups[6].Value);

                OffsetHour = int.Parse(matchStrings[0].Groups[7].Value); 
                OffsetMin = int.Parse(matchStrings[0].Groups[8].Value);
                Offset = new TimeSpan(OffsetHour, OffsetMin, 0);

                targetTimeOffset = new DateTimeOffset(Year, Month, Day, Hour, Minute, Second, Offset);
                targetTime = targetTimeOffset.LocalDateTime;

                targetUIString = targetTime.ToString("f");
                return targetUIString;
            }
            else
            {
                Debug.WriteLine("时间正则匹配失败！");
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

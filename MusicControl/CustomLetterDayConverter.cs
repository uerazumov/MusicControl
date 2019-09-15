using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace MusicControl
{
    public class CustomLetterDayConverter : IValueConverter
    {
        public static HashSet<DateTime> dict = new HashSet<DateTime>();

        private static CustomLetterDayConverter instance;

        static CustomLetterDayConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = null;
            if (dict.Contains((DateTime)value))
                text = null;
            else
                text = "";

            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static CustomLetterDayConverter GetInstance()
        {
            if (instance == null)
                instance = new CustomLetterDayConverter();
            return instance;
        }
    }
}

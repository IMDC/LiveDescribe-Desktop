using System;
using System.Globalization;
using System.Windows.Data;

namespace LiveDescribe.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    class BoolToRecordingContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (bool)value;
            return v ? "Stop Recording" : "Record";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


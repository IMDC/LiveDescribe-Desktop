using LiveDescribe.Resources.UiStrings;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LiveDescribe.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    public class MillisecondsToTimeLeftStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = (double)value;

            return string.Format(UiStrings.Label_Format_TimeLeft,
                TimeSpan.FromMilliseconds(d).ToString("h\\:mm\\:ss\\.fff"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using LiveDescribe.Resources.UiStrings;

namespace LiveDescribe.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    public class MillisecondsToTimeLeftStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = (double) value;

            return string.Format("{0} {1}", UiStrings.Label_TimeLeft,
                TimeSpan.FromMilliseconds(d).ToString("h\\:mm\\:ss\\.fff"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

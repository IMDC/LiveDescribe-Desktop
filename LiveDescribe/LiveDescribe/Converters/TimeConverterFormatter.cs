using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LiveDescribe.Converters
{
    [ValueConversion(typeof(TimeSpan), typeof(string))]
    class TimeConverterFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan timespan = (TimeSpan)value;
            return timespan.ToString("hh\\:mm\\:ss\\.fff");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

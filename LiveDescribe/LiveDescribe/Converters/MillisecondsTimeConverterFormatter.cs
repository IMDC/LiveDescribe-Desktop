using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LiveDescribe.Converters
{
    public class MillisecondsTimeConverterFormatter : IValueConverter
    {
        public MillisecondsTimeConverterFormatter()
        { }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return TimeSpan.FromMilliseconds((double)value).ToString("h\\:mm\\:ss\\.fff");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan t = new TimeSpan();
            bool success = TimeSpan.TryParse(value.ToString(), out t);

            if (success)
                return t.TotalMilliseconds;
            else
                return 0;
        }
    }
}
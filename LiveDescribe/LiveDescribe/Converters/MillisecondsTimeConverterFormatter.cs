using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LiveDescribe.Converters
{

    public class MillisecondsTimeConverterFormatter : IValueConverter
    {
        private StringBuilder _builder;

        public MillisecondsTimeConverterFormatter()
        {
            _builder = new StringBuilder();
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            _builder.Clear();

            int val = System.Convert.ToInt32(value);

            int totalTimeLeft;

            int hours = val / 3600000;
            totalTimeLeft = (val - (hours * 3600000));
            int minutes = totalTimeLeft / 60000;
            totalTimeLeft = totalTimeLeft - (minutes * 60000);
            int seconds = totalTimeLeft / 1000;
            int milliseconds = totalTimeLeft - (seconds * 1000);
            TimeSpan timespan = new TimeSpan(0, hours, minutes, seconds, milliseconds);
            return timespan.ToString("h\\:mm\\:ss\\.fff");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

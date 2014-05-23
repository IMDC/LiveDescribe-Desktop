using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LiveDescribe.Converters
{
    class TimeConverterFormatter : IValueConverter
    {
        private StringBuilder _builder;

        public TimeConverterFormatter()
        {
            _builder = new StringBuilder();
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            _builder.Clear();
            String val = value.ToString();

            if (val.Length == 16)
            {
                _builder.Append(val.Substring(0, 2));
                _builder.Append(":");
                _builder.Append(val.Substring(3, 2));
                _builder.Append(":");
                _builder.Append(val.Substring(6, 2));
                _builder.Append(":");
                _builder.Append(val.Substring(9, 3));
                return _builder.ToString();
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

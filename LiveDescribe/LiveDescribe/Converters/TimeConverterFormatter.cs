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
            if (value.ToString().Length == 16)
            {
                _builder.Append(value.ToString().Substring(3, 2));
                _builder.Append(":");
                _builder.Append(value.ToString().Substring(6, 2));
                _builder.Append(":");
                _builder.Append(value.ToString().Substring(9, 3));
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using LiveDescribe.Resources.UiStrings;

namespace LiveDescribe.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToMutedContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var v = (bool) value;
            return v ? UiStrings.Command_UnMute : UiStrings.Command_Mute;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

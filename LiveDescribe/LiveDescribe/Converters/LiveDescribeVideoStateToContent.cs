using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using LiveDescribe.Resources.UiStrings;

namespace LiveDescribe.Converters
{
    [ValueConversion(typeof(LiveDescribeVideoStates), typeof(string))]
    public class LivedescribeVideoStateToContent : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var v = (LiveDescribeVideoStates)value;
            return v == LiveDescribeVideoStates.PlayingVideo ? UiStrings.Command_Pause : UiStrings.Command_Play;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

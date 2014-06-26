using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LiveDescribe.Converters
{
    [ValueConversion(typeof(LiveDescribeVideoStates), typeof(bool))]
    public class VideoStateToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentState = (LiveDescribeVideoStates)value;
            var val = currentState != LiveDescribeVideoStates.PlayingVideo;
            return currentState != LiveDescribeVideoStates.PlayingVideo;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

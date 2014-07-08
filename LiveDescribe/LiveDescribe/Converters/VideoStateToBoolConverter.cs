using System;
using System.Globalization;
using System.Windows.Data;

namespace LiveDescribe.Converters
{
    [ValueConversion(typeof(LiveDescribeVideoStates), typeof(bool))]
    public class VideoStateToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentState = (LiveDescribeVideoStates)value;
            return (currentState != LiveDescribeVideoStates.PlayingVideo
                && currentState != LiveDescribeVideoStates.PlayingExtendedDescription);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

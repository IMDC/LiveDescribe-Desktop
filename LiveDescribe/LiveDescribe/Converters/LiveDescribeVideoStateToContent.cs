using System;
using System.Windows.Controls;
using System.Windows.Data;
using LiveDescribe.Resources.UiStrings;
using LiveDescribe.Utilities;

namespace LiveDescribe.Converters
{
    [ValueConversion(typeof(LiveDescribeVideoStates), typeof(Image))]
    public class LivedescribeVideoStateToContent : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var v = (LiveDescribeVideoStates)value;
            return v == LiveDescribeVideoStates.PlayingVideo ? CustomResources.Pause : CustomResources.Play;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

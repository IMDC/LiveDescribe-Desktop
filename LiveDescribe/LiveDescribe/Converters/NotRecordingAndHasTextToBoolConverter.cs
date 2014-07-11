using System;
using System.Globalization;
using System.Windows.Data;

namespace LiveDescribe.Converters
{
    class NotRecordingAndHasTextToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var isRecording = (bool)values[0];
            var spaceHasText = (bool)values[1];

            return !isRecording && spaceHasText;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

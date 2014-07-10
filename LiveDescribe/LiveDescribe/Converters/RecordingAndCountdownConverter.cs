using LiveDescribe.Resources.UiStrings;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LiveDescribe.Converters
{
    public class RecordingAndCountdownConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var isRecording = (bool)values[0];
            var countingDown = (bool)values[1];

            if (isRecording)
                return UiStrings.Command_StopRecord;

            if (countingDown)
                return UiStrings.Command_Cancel;

            return UiStrings.Command_Record;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Globalization;
using System.Windows.Data;

namespace LiveDescribe.Converters
{
    /// <summary>
    /// A converter that will convert a boolean to a specified content depending on its value.
    /// </summary>
    public class BoolToContentConverter : IValueConverter
    {
        public BoolToContentConverter()
        {
            IsThreeState = false;
        }

        /// <summary>
        /// Determines whether the bool that is being converted is nullable or not. If true, a null
        /// value or one of incorrect type will convert to NullContent. If False, a value that's
        /// null or the wrong type will convert to FalseContent. This property is false by default.
        /// </summary>
        public bool IsThreeState { get; set; }

        /// <summary>
        /// The content to return if true.
        /// </summary>
        public object TrueContent { get; set; }

        /// <summary>
        /// The content to return if false.
        /// </summary>
        public object FalseContent { get; set; }

        /// <summary>
        /// The content to return if the converter is three-state and the value given was null or
        /// of incorrect type.
        /// </summary>
        public object NullContent { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nullableBool = value as bool?;

            return nullableBool.HasValue
                ? (nullableBool.Value) ? TrueContent : FalseContent
                : (IsThreeState) ? NullContent : FalseContent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

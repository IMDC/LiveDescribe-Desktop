using LiveDescribe.Model;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace LiveDescribe.Factories
{
    public class FormattedTextFactory
    {
        /// <summary>
        /// Creates a formatted text instance to draw onto a canvas over an interval.
        /// </summary>
        /// <param name="interval">The interval to create text for.</param>
        /// <param name="textBrush">The brush used to colour the text.</param>
        /// <returns>A formatted text run meant to be drawn to a canvas.</returns>
        public static FormattedText IntervalText(DescribableInterval interval, Brush textBrush)
        {
            return new FormattedText(interval.Index + " " + interval.Title,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                10,
                textBrush);
        }
    }
}

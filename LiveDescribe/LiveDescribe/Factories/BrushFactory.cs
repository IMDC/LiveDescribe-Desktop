using System.Windows.Media;

namespace LiveDescribe.Factories
{
    public static class BrushFactory
    {
        /// <summary>
        /// Creates a new frozen SolidColourBrush with the given colour.
        /// </summary>
        /// <param name="brushColour">Colour to use.</param>
        /// <returns>An instance of SolidColourBrush that is already frozen.</returns>
        public static Brush SolidColour(Color brushColour)
        {
            var brush = new SolidColorBrush(brushColour);
            brush.Freeze();
            return brush;
        }
    }
}

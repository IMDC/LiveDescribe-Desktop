using System.Windows.Controls;
using System.Windows.Media;

namespace LiveDescribe.Extensions
{
    public static class GeometryGroupExtensions
    {
        /// <summary>
        /// Freezes and then creates an image object from a GeometryGroup.
        /// </summary>
        /// <param name="brush">The fill brush for the group.</param>
        /// <param name="pen">The Border pen for the group.</param>
        /// <param name="group">The group to create an image from.</param>
        /// <returns>An image object that can be added to the canvas.</returns>
        public static Image CreateImage(this  GeometryGroup group, Brush brush, Pen pen)
        {
            group.Freeze();

            var drawing = new GeometryDrawing(brush, pen, group);
            drawing.Freeze();

            var drawingImage = new DrawingImage(drawing);
            drawingImage.Freeze();

            return new Image { Source = drawingImage };
        }
    }
}

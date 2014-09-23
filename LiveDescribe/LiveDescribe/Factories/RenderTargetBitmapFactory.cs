using LiveDescribe.Model;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LiveDescribe.Factories
{
    public static class RenderTargetBitmapFactory
    {
        private static readonly Pen LinePen = new Pen(Brushes.Black, 1);

        public static RenderTargetBitmap CreateDescriptionWaveForm(Description description, Rect bounds)
        {
            if (bounds.Width <= 1 || bounds.Height <= 1)
                return null;

            var drawingVisual = new DrawingVisual();

            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawLine(LinePen, new Point(0, 0), new Point(bounds.Width, bounds.Height));
            }

            var bitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);
            bitmap.Freeze();

            return bitmap;
        }
    }
}

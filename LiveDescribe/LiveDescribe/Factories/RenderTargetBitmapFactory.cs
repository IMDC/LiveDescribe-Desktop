using LiveDescribe.Model;
using LiveDescribe.Utilities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LiveDescribe.Factories
{
    public static class RenderTargetBitmapFactory
    {
        private const int DefaultDpi = 96;

        private static readonly Pen LinePen = new Pen(Brushes.Black, 1);

        public static RenderTargetBitmap CreateDescriptionWaveForm(Description description, Rect bounds,
            double canvasWidth)
        {
            if (bounds.Width <= 1 || bounds.Height <= 1)
                return null;

            /*if (description.WaveformImage != null)
                return description.WaveformImage;*/

            if (description.Waveform == null)
                description.GenerateWaveForm();

            var drawingVisual = new DrawingVisual();

            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawLine(LinePen, new Point(0, 0), new Point(bounds.Width, bounds.Height));

                var data = description.Waveform.Data;

                double samplesPerPixel = Math.Max(data.Count / canvasWidth, 1);
                double middle = bounds.Height / 2;
                double yscale = middle;

                double samplesPerSecond = (description.Waveform.Header.SampleRate *
                    (description.Waveform.Header.BlockAlign / (double)description.Waveform.SampleRatio));

                var waveformLineGroup = new GeometryGroup();

                int endPixel = (int)bounds.Width;

                for (int pixel = 0; pixel <= endPixel; pixel++)
                {
                    double offsetTime = (description.Duration / (bounds.Width * Milliseconds.PerSecond))
                        * pixel;
                    double sampleStart = Math.Max(samplesPerSecond * offsetTime, 0);

                    if (sampleStart + samplesPerPixel < data.Count)
                    {
                        var range = data.GetRange((int)sampleStart, (int)samplesPerPixel);

                        double max = (double)range.Max() / short.MaxValue;
                        double min = (double)range.Min() / short.MaxValue;

                        waveformLineGroup.Children.Add(new LineGeometry
                        {
                            StartPoint = new Point(pixel, middle + max * yscale),
                            EndPoint = new Point(pixel, middle + min * yscale),
                        });
                    }
                }

                waveformLineGroup.Freeze();
                dc.DrawGeometry(Brushes.Black, LinePen, waveformLineGroup);
            }

            var bitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, DefaultDpi,
                DefaultDpi, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);
            bitmap.Freeze();

            description.WaveformImage = bitmap;

            return bitmap;
        }
    }
}

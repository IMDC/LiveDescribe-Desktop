using LiveDescribe.Converters;
using LiveDescribe.Utilities;
using LiveDescribe.ViewModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace LiveDescribe.Controls
{
    public class NumberLineCanvas : Canvas
    {
        /// <summary>The time interval between two lines.</summary>
        private const double LineTimeSeconds = 1;
        /// <summary>The number of lines that need to be drawn before drawing a long line.</summary>
        private const int LongLineDivisor = 5;

        /// <summary>The length of a short line as a percent of the height of the canvas.</summary>
        private const double ShortLineLengthPercent = 0.5;
        /// <summary>The length of a long line as a percent of the height of the canvas.</summary>
        private const double LongLineLengthPercent = 0.8;

        private NumberLineCanvasViewModel _viewModel;
        private readonly MillisecondsTimeConverterFormatter _millisecondsTimeConverter;
        private readonly Pen _shortLinePen;
        private readonly Pen _longLinePen;

        public NumberLineCanvas()
        {
            _millisecondsTimeConverter = new MillisecondsTimeConverterFormatter();
            _shortLinePen = new Pen(Brushes.Black, 1);
            _shortLinePen.Freeze();

            _longLinePen = new Pen(Brushes.Blue, 1);
            _longLinePen.Freeze();

            DataContextChanged += OnDataContextChanged;
        }

        public void AddLinesToNumberTimeLine(double canvasWidth, double horizontalOffset, double parentActualWidth)
        {
            if (_viewModel == null || _viewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded
                || canvasWidth == 0)
                return;

            var shortLineGroup = new GeometryGroup();
            var longLineGroup = new GeometryGroup();

            //Number of lines in the amount of time that the video plays for
            int numLines = (int)(_viewModel.Player.DurationMilliseconds / (LineTimeSeconds * Milliseconds.PerSecond));
            int beginLine = (int)((numLines / canvasWidth) * horizontalOffset);
            int endLine = beginLine + (int)((numLines / canvasWidth) * parentActualWidth) + 1;
            //Clear the canvas because we don't want the remaining lines due to importing a new video
            //or resizing the window
            Children.Clear();

            double widthPerLine = canvasWidth / numLines;

            //Keep track of which line each group begins on
            int firstShortLine = -1;
            int firstLongLine = -1;

            for (int i = beginLine; i <= endLine; i++)
            {
                double xPos = widthPerLine * i;

                if (i % LongLineDivisor == 0)
                {
                    if (firstLongLine == -1)
                        firstLongLine = i;

                    longLineGroup.Children.Add(new LineGeometry
                    {
                        StartPoint = new Point(xPos, 0),
                        EndPoint = new Point(xPos, ActualHeight * LongLineLengthPercent),
                    });

                    var timestamp = new TextBlock
                    {
                        Text = (string)_millisecondsTimeConverter.Convert((i * LineTimeSeconds) * 1000, typeof(int), null,
                        CultureInfo.CurrentCulture)
                    };
                    Canvas.SetLeft(timestamp, ((widthPerLine * i) - 24));
                    Children.Add(timestamp);
                }
                else
                {
                    if (firstShortLine == -1)
                        firstShortLine = i;

                    shortLineGroup.Children.Add(new LineGeometry
                    {
                        StartPoint = new Point(xPos, 0),
                        EndPoint = new Point(xPos, ActualHeight * ShortLineLengthPercent), //* 0.5
                    });
                }
            }

            shortLineGroup.Freeze();
            var shortLineDrawing = new GeometryDrawing(Brushes.Black, _shortLinePen, shortLineGroup);
            shortLineDrawing.Freeze();

            var shortLineDrawingImage = new DrawingImage(shortLineDrawing);
            shortLineDrawingImage.Freeze();

            var shortLineImage = new Image { Source = shortLineDrawingImage };

            SetLeft(shortLineImage, widthPerLine * firstShortLine);
            SetTop(shortLineImage, 0);

            Children.Add(shortLineImage);


            longLineGroup.Freeze();
            var longLineDrawing = new GeometryDrawing(Brushes.Black, _longLinePen, longLineGroup);
            longLineDrawing.Freeze();

            var longLineDrawingImage = new DrawingImage(longLineDrawing);
            longLineDrawingImage.Freeze();

            var longLineImage = new Image { Source = longLineDrawingImage };

            SetLeft(longLineImage, widthPerLine * firstLongLine);
            SetTop(longLineImage, 0);

            Children.Add(longLineImage);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as NumberLineCanvasViewModel;
        }
    }
}

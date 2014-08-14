using LiveDescribe.Converters;
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
        private const double LineTime = 1; //each line in the NumberLineCanvas appears every 1 second
        private const int LongLineTime = 5; // every 5 LineTimes, you get a Longer Line

        private NumberLineCanvasViewModel _viewModel;
        private readonly MillisecondsTimeConverterFormatter _millisecondsTimeConverter;
        private readonly Pen _shortLinePen;

        public NumberLineCanvas()
            : base()
        {
            _millisecondsTimeConverter = new MillisecondsTimeConverterFormatter();
            _shortLinePen = new Pen(Brushes.Black, 1);
            _shortLinePen.Freeze();

            DataContextChanged += OnDataContextChanged;
        }

        public void AddLinesToNumberTimeLine(double canvasWidth, double horizontalOffset)
        {
            if (_viewModel == null || _viewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded
                || canvasWidth == 0)
                return;

            var shortLineGroup = new GeometryGroup();
            var longLineGroup = new GeometryGroup();

            //Number of lines in the amount of time that the video plays for
            int numlines = (int)(_viewModel.Player.DurationMilliseconds / (LineTime * 1000));
            int beginLine = (int)((numlines / canvasWidth) * horizontalOffset);
            int endLine = beginLine + (int)((numlines / canvasWidth) * ActualWidth) + 1;
            //Clear the canvas because we don't want the remaining lines due to importing a new video
            //or resizing the window
            Children.Clear();

            for (int i = beginLine; i <= endLine; ++i)
            {
                double xPos = canvasWidth / numlines * i;

                var p1 = new Point(xPos, 0);
                Point p2;

                if (i % LongLineTime == 0)
                {
                    p2 = new Point(xPos, ActualHeight / 1.2);
                    longLineGroup.Children.Add(new LineGeometry(p1, p2));
                    /*Children.Add(new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Blue,
                        StrokeThickness = 1.5,
                        Y1 = 0,
                        Y2 = ActualHeight / 1.2,
                        X1 = canvasWidth / numlines * i,
                        X2 = canvasWidth / numlines * i,
                    });*/

                    var timestamp = new TextBlock
                    {
                        Text = (string)_millisecondsTimeConverter.Convert((i * LineTime) * 1000, typeof(int), null,
                            CultureInfo.CurrentCulture)
                    };
                    SetLeft(timestamp, ((canvasWidth / numlines * i) - 24));
                    Children.Add(timestamp);
                }
                else
                {
                    p2 = new Point(xPos, ActualHeight / 2);
                    shortLineGroup.Children.Add(new LineGeometry(p1, p2));
                    /*Children.Add(new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Black,
                        Y1 = 0,
                        Y2 = ActualHeight / 2,
                        X1 = canvasWidth / numlines * i,
                        X2 = canvasWidth / numlines * i
                    });*/
                }
            }

            shortLineGroup.Freeze();
            var shortLineDrawing = new GeometryDrawing(Brushes.Black, _shortLinePen, shortLineGroup);
            shortLineDrawing.Freeze();

            var shortLineDrawingImage = new DrawingImage(shortLineDrawing);
            shortLineDrawingImage.Freeze();

            var shortLineImage = new Image { Source = shortLineDrawingImage };

            SetLeft(shortLineImage, 0);
            SetTop(shortLineImage, 0);

            Children.Add(shortLineImage);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as NumberLineCanvasViewModel;
        }
    }
}

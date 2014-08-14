using LiveDescribe.Converters;
using LiveDescribe.ViewModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace LiveDescribe.Controls
{
    public class NumberLineCanvas : Canvas
    {
        private const double LineTime = 1; //each line in the NumberLineCanvas appears every 1 second
        private const int LongLineTime = 5; // every 5 LineTimes, you get a Longer Line

        private NumberLineCanvasViewModel _viewModel;
        private readonly MillisecondsTimeConverterFormatter _millisecondsTimeConverter;

        public NumberLineCanvas()
            : base()
        {
            _millisecondsTimeConverter = new MillisecondsTimeConverterFormatter();

            DataContextChanged += OnDataContextChanged;
        }

        public void AddLinesToNumberTimeLine(double _canvasWidth, double HorizontalOffset)
        {
            if (_viewModel == null || _viewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded
                || _canvasWidth == 0)
                return;

            //Number of lines in the amount of time that the video plays for
            int numlines = (int)(_viewModel.Player.DurationMilliseconds / (LineTime * 1000));
            int beginLine = (int)((numlines / _canvasWidth) * HorizontalOffset);
            int endLine = beginLine + (int)((numlines / _canvasWidth) * ActualWidth) + 1;
            //Clear the canvas because we don't want the remaining lines due to importing a new video
            //or resizing the window
            Children.Clear();

            for (int i = beginLine; i <= endLine; ++i)
            {
                if (i % LongLineTime == 0)
                {
                    Children.Add(new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Blue,
                        StrokeThickness = 1.5,
                        Y1 = 0,
                        Y2 = ActualHeight / 1.2,
                        X1 = _canvasWidth / numlines * i,
                        X2 = _canvasWidth / numlines * i,
                    });

                    var timestamp = new TextBlock
                    {
                        Text = (string)_millisecondsTimeConverter.Convert((i * LineTime) * 1000, typeof(int), null,
                            CultureInfo.CurrentCulture)
                    };
                    SetLeft(timestamp, ((_canvasWidth / numlines * i) - 24));
                    Children.Add(timestamp);
                }
                else
                {
                    Children.Add(new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Black,
                        Y1 = 0,
                        Y2 = ActualHeight / 2,
                        X1 = _canvasWidth / numlines * i,
                        X2 = _canvasWidth / numlines * i
                    });
                }
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as NumberLineCanvasViewModel;
        }
    }
}

using LiveDescribe.Converters;
using LiveDescribe.Factories;
using LiveDescribe.Utilities;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LiveDescribe.Controls.Canvases
{
    public class NumberLineCanvas : GeometryImageCanvas
    {
        /// <summary>The time interval between two lines.</summary>
        private const double LineTimeSeconds = 1;
        /// <summary>The number of lines that need to be drawn before drawing a long line.</summary>
        private const int LongLineDivisor = 5;

        /// <summary>The length of a short line as a percent of the height of the canvas.</summary>
        private const double ShortLineLengthPercent = 0.5;
        /// <summary>The length of a long line as a percent of the height of the canvas.</summary>
        private const double LongLineLengthPercent = 0.2;

        private NumberLineViewModel _viewModel;
        private readonly MillisecondsTimeConverterFormatter _timestampConverter;
        private readonly Pen _shortLinePen;
        private readonly Pen _longLinePen;
        private readonly Image _lineImage;

        public NumberLineCanvas()
        {
            Background = Brushes.White;

            _lineImage = new Image();
            Children.Add(_lineImage);

            _timestampConverter = new MillisecondsTimeConverterFormatter();

            _shortLinePen = PenFactory.LinePen(Brushes.Black);
            _longLinePen = PenFactory.LinePen(Brushes.Blue);

            DataContextChanged += OnDataContextChanged;
        }

        public override void Draw()
        {
            if (_viewModel == null || _viewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded
                || Width == 0)
            {
                ResetImageOnCanvas(_lineImage);
                return;
            }

            var drawingVisual = new DrawingVisual();

            //Number of lines in the amount of time that the video plays for
            int numLines = (int)(VideoDurationMsec / (LineTimeSeconds * Milliseconds.PerSecond));
            int beginLine = (int)((numLines / Width) * VisibleX);
            int endLine = beginLine + (int)((numLines / Width) * VisibleWidth) + 1;

            double widthPerLine = Width / numLines;

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                for (int i = beginLine; i <= endLine; i++)
                {
                    double xPos = widthPerLine * i;

                    if (i % LongLineDivisor == 0)
                    {
                        var p0 = new Point(xPos, 0);
                        var p1 = new Point(xPos, ActualHeight * LongLineLengthPercent);

                        drawingContext.DrawLine(_longLinePen, p0, p1);

                        var timestamp = (string)_timestampConverter.Convert((i * LineTimeSeconds) * 1000, typeof(int),
                            null, CultureInfo.CurrentCulture);
                        var ft = FormattedTextFactory.Text(timestamp, 11, Brushes.Black);

                        drawingContext.DrawText(ft, new Point(xPos - ft.Width / 2, p1.Y - 1));
                    }
                    else
                    {
                        drawingContext.DrawLine(_shortLinePen,
                            new Point(xPos, 0),
                            new Point(xPos, ActualHeight * ShortLineLengthPercent));
                    }
                }
            }

            DisplayVisualOnCanvas(_lineImage, drawingVisual);
        }

        protected override void SetBrushes()
        {
            throw new System.NotImplementedException();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as NumberLineViewModel;
        }
    }
}

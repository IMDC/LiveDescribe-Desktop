using LiveDescribe.Extensions;
using LiveDescribe.Interfaces;
using LiveDescribe.Utilities;
using LiveDescribe.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LiveDescribe.Controls
{
    public class AudioCanvas : Canvas
    {
        #region Fields
        private AudioCanvasViewModel _viewModel;
        private readonly Pen _linePen;
        #endregion

        #region Constructor
        public AudioCanvas()
        {
            _linePen = new Pen(Brushes.Black, 1);
            _linePen.Freeze();

            InitEventHandlers();
        }

        private void InitEventHandlers()
        {
            DataContextChanged += OnDataContextChanged;

            //Resize all spaces to fit the height of this canvas.
            SizeChanged += (sender, args) =>
            {
                if (_viewModel == null || Math.Abs(args.PreviousSize.Height - args.NewSize.Height) < 0.01)
                    return;

                foreach (var space in _viewModel.Spaces)
                    space.Height = ActualHeight;
            };
        }
        #endregion

        public ItemCanvas.ActionState CurrentActionState { get; set; }

        /// <summary>
        /// Draws the waveform for the current window of sound and adds it to the AudioCanvas.
        /// </summary>
        /// <param name="canvasWidth">The width of the canvas.</param>
        /// <param name="horizontalOffset">The horizontal offset from the beginning of the canvas.</param>
        /// <param name="parentActualWidth">The ActualWidth of the parent container.</param>
        /// <param name="videoDuration">The duration of the video.</param>
        public void DrawWaveForm(double canvasWidth, double horizontalOffset, double parentActualWidth,
            double videoDuration)
        {
            if (_viewModel == null || _viewModel.Waveform == null || canvasWidth == 0 || parentActualWidth == 0
                || _viewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return;

            var data = _viewModel.Waveform.Data;
            double samplesPerPixel = Math.Max(data.Count / canvasWidth, 1);
            double middle = ActualHeight / 2;
            double yscale = middle;

            Children.Clear();

            int beginPixel = (int)horizontalOffset;
            int endPixel = beginPixel + (int)parentActualWidth;

            int ratio = _viewModel.Waveform.Header.Channels == 2 ? 40 : 80;
            double samplesPerSecond =
                (_viewModel.Waveform.Header.SampleRate * (_viewModel.Waveform.Header.BlockAlign / (double)ratio));

            var waveformLineGroup = new GeometryGroup();

            double absMin = 0;

            for (int pixel = beginPixel; pixel <= endPixel; pixel++)
            {
                double offsetTime = (videoDuration / (canvasWidth * Milliseconds.PerSecond))
                    * pixel;
                double sampleStart = samplesPerSecond * offsetTime;

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

                    absMin = Math.Min(absMin, min);
                }
            }

            var waveformImage = waveformLineGroup.CreateImage(Brushes.Black, _linePen);

            SetLeft(waveformImage, beginPixel);
            SetTop(waveformImage, middle + absMin * yscale);

            Children.Add(waveformImage);
        }

        public void DrawSpaces(double canvasWidth, double horizontalOffset, double parentActualWidth,
            double videoDuration)
        {
            if (_viewModel == null || _viewModel.Spaces == null || canvasWidth == 0 || parentActualWidth == 0
                || _viewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return;

            var backgroundGroup = new GeometryGroup();
            var selectedGroup = new GeometryGroup();

            double beginPixel = horizontalOffset;
            double endPixel = beginPixel + parentActualWidth;

            double beginTimeMsec = (videoDuration / (canvasWidth))
                    * beginPixel;
            double endTimeMsec = (videoDuration / (canvasWidth))
                    * endPixel;

            foreach (var space in _viewModel.Spaces)
            {
                if (IsIntervalVisible(space, beginTimeMsec, endTimeMsec))
                {
                    var rect = new RectangleGeometry(new Rect(space.X, space.Y, space.Width, space.Height));

                    if (space.IsSelected)
                        selectedGroup.Children.Add(rect);
                    else
                        backgroundGroup.Children.Add(rect);
                }
            }

            if (0 < backgroundGroup.Children.Count)
            {
                var backgroundImage = backgroundGroup.CreateImage(Brushes.DodgerBlue, _linePen);

                Children.Add(backgroundImage);

                //The Image has to be set to the smallest X value of the visible spaces.
                double minX = backgroundGroup.Children[0].Bounds.X;
                for (int i = 1; i < backgroundGroup.Children.Count; i++)
                {
                    minX = Math.Min(minX, backgroundGroup.Children[i].Bounds.X);
                }

                SetLeft(backgroundImage, minX);
                SetTop(backgroundImage, 0);
            }

            if (0 < selectedGroup.Children.Count)
            {
                var selectedImage = selectedGroup.CreateImage(Brushes.Black, _linePen);

                Children.Add(selectedImage);

                //There can only be one selected item
                SetLeft(selectedImage, selectedGroup.Children[0].Bounds.X);
                SetTop(selectedImage, 0);
            }
        }

        private bool IsIntervalVisible(IDescribableInterval interval, double visibleBeginMsec, double visibleEndMsec)
        {
            return (visibleBeginMsec <= interval.StartInVideo && interval.StartInVideo <= visibleEndMsec)
                    || (visibleBeginMsec <= interval.EndInVideo && interval.EndInVideo <= visibleEndMsec)
                    || (interval.StartInVideo <= visibleBeginMsec && visibleEndMsec <= interval.EndInVideo);
        }

        #region Event Handlers
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as AudioCanvasViewModel;
        }
        #endregion
    }
}

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

            DataContextChanged += OnDataContextChanged;
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

            waveformLineGroup.Freeze();

            var waveformDrawing = new GeometryDrawing(Brushes.Black, _linePen, waveformLineGroup);
            waveformDrawing.Freeze();

            var waveformDrawingImage = new DrawingImage(waveformDrawing);
            waveformDrawingImage.Freeze();

            var waveformImage = new Image { Source = waveformDrawingImage };

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
                backgroundGroup.Freeze();

                var backgroundDrawing = new GeometryDrawing(Brushes.DodgerBlue, _linePen, backgroundGroup);
                backgroundDrawing.Freeze();

                var backgroundDrawingImage = new DrawingImage(backgroundDrawing);
                backgroundDrawingImage.Freeze();

                var backgroundImage = new Image { Source = backgroundDrawingImage };

                Children.Add(backgroundImage);

                SetLeft(backgroundImage, backgroundGroup.Children[0].Bounds.X);
                SetTop(backgroundImage, 0);
            }

            if (0 < selectedGroup.Children.Count)
            {
                selectedGroup.Freeze();

                var selectedDrawing = new GeometryDrawing(Brushes.Black, _linePen, selectedGroup);
                selectedDrawing.Freeze();

                var selectedDrawingImage = new DrawingImage(selectedDrawing);
                selectedDrawingImage.Freeze();

                var selectedImage = new Image { Source = selectedDrawingImage };

                Children.Add(selectedImage);

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

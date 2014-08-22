using LiveDescribe.Extensions;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using LiveDescribe.Utilities;
using LiveDescribe.ViewModel;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LiveDescribe.Controls
{
    public class AudioCanvas : Canvas
    {
        #region Fields
        private AudioCanvasViewModel _viewModel;
        private readonly Pen _linePen;
        private Brush _spaceBrush;
        private Brush _selectedItemBrush;
        private CanvasMouseSelection _mouseSelection;
        #endregion

        #region Constructor
        public AudioCanvas()
        {
            _linePen = new Pen(Brushes.Black, 1);
            _linePen.Freeze();

            /* This method contains a null reference in the designer, causing an exception and not
             * rendering the canvas. This check guards against it.
             */
            if (!DesignerProperties.GetIsInDesignMode(this))
                SetBrushes();

            _mouseSelection = CanvasMouseSelection.NoSelection;

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

            Settings.Default.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ColourScheme")
                    SetBrushes();
            };
        }
        #endregion

        #region Events

        public event EventHandler CanvasRedrawRequested;
        #endregion

        #region Properties
        public IntervalMouseAction CurrentIntervalMouseAction { get; set; }
        #endregion

        /// <summary>
        /// Draws the waveform for the current window of sound and adds it to the AudioCanvas.
        /// </summary>
        /// <param name="visibleStartPoint">The horizontal offset from the beginning of the canvas.</param>
        /// <param name="visibleWidth">The ActualWidth of the parent container.</param>
        /// <param name="videoDuration">The duration of the video.</param>
        public void DrawWaveForm(double visibleStartPoint, double visibleWidth, double videoDuration)
        {
            if (_viewModel == null || _viewModel.Waveform == null || Width == 0 || visibleWidth == 0
                || _viewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return;

            var data = _viewModel.Waveform.Data;
            double samplesPerPixel = Math.Max(data.Count / Width, 1);
            double middle = ActualHeight / 2;
            double yscale = middle;

            Children.Clear();

            int beginPixel = (int)visibleStartPoint;
            int endPixel = beginPixel + (int)visibleWidth;

            int ratio = _viewModel.Waveform.Header.Channels == 2 ? 40 : 80;
            double samplesPerSecond =
                (_viewModel.Waveform.Header.SampleRate * (_viewModel.Waveform.Header.BlockAlign / (double)ratio));

            var waveformLineGroup = new GeometryGroup();

            double absMin = 0;

            for (int pixel = beginPixel; pixel <= endPixel; pixel++)
            {
                double offsetTime = (videoDuration / (Width * Milliseconds.PerSecond))
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

        public void DrawSpaces(double visibleStartPoint, double visibleWidth, double videoDuration)
        {
            if (_viewModel == null || _viewModel.Spaces == null || Width == 0 || visibleWidth == 0
                || _viewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return;

            var backgroundGroup = new GeometryGroup();
            var selectedGroup = new GeometryGroup();

            double beginPixel = visibleStartPoint;
            double endPixel = beginPixel + visibleWidth;

            double beginTimeMsec = (videoDuration / (Width)) * beginPixel;
            double endTimeMsec = (videoDuration / (Width)) * endPixel;

            foreach (var space in _viewModel.Spaces)
            {
                if (IsIntervalVisible(space, beginTimeMsec, endTimeMsec))
                {
                    var rect = new RectangleGeometry(new Rect(space.X, space.Y, (space).Width, space.Height));

                    if (space.IsSelected)
                        selectedGroup.Children.Add(rect);
                    else
                        backgroundGroup.Children.Add(rect);
                }
            }

            if (0 < backgroundGroup.Children.Count)
            {
                var backgroundImage = backgroundGroup.CreateImage(_spaceBrush, _linePen);

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
                var selectedImage = selectedGroup.CreateImage(_selectedItemBrush, _linePen);

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

        /// <summary>
        /// Sets the brushes based off of ColourScheme settings.
        /// </summary>
        private void SetBrushes()
        {
            _spaceBrush = new SolidColorBrush(Settings.Default.ColourScheme.SpaceColour);
            _spaceBrush.Freeze();

            _selectedItemBrush = new SolidColorBrush(Settings.Default.ColourScheme.SelectedItemColour);
            _selectedItemBrush.Freeze();

            OnCanvasRedrawRequested();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (_viewModel == null)
                return;

            var point = e.GetPosition(this);

            foreach (var space in _viewModel.Spaces)
            {
                if (space.X <= point.X && point.X <= space.X + space.Width)
                {
                    SelectSpace(space);
                }
                else
                    space.IsSelected = false;
            }

            OnCanvasRedrawRequested();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (_mouseSelection.Action != IntervalMouseAction.None)
            {
                _mouseSelection.Item.IsSelected = false;
                _mouseSelection = CanvasMouseSelection.NoSelection;
                Mouse.Capture(null);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var mousePos = e.GetPosition(this);

            switch (_mouseSelection.Action)
            {
                case IntervalMouseAction.Dragging:
                    double startTime = Math.Max(0, (_viewModel.Player.DurationMilliseconds / (Width)) * mousePos.X);
                    _mouseSelection.Item.EndInVideo = startTime + _mouseSelection.Item.Duration;
                    _mouseSelection.Item.StartInVideo = startTime;

                    break;
                case IntervalMouseAction.None: return;
            }

            OnCanvasRedrawRequested();
        }

        private void SelectSpace(Space space)
        {
            space.IsSelected = true;
            space.SpaceMouseDownCommand.Execute();

            _mouseSelection = new CanvasMouseSelection(IntervalMouseAction.Dragging, space);

            CaptureMouse();
        }

        #region Event Handlers
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as AudioCanvasViewModel;
        }
        #endregion

        #region Event Invokation
        protected virtual void OnCanvasRedrawRequested()
        {
            var handler = CanvasRedrawRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion
    }
}

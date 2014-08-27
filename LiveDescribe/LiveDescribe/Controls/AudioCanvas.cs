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
        #region Constants
        /// <summary>
        /// The distance away from the beginning or ending of an interval in pixels that the user
        /// has to click on to be able to move the caption.
        /// </summary>
        private const double SelectionPixelWidth = 10;

        private const double MinIntervalDurationMsec = 300;
        #endregion

        #region Fields
        private AudioCanvasViewModel _viewModel;
        private readonly Pen _linePen;
        private Brush _completedSpaceBrush;
        private Brush _spaceBrush;
        private Brush _selectedItemBrush;
        private CanvasMouseSelection _mouseSelection;
        private Image _waveformImage;
        private Image _completedSpaceImage;
        private Image _spaceImage;
        private Image _selectedImage;

        /// <summary>
        /// The leftmost pixel of the canvas is visible to the user.
        /// </summary>
        private double _visibleX;

        /// <summary>
        /// How many pixels wide the viewable area of the canvas is.
        /// </summary>
        private double _visibleWidth;

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

        #region Properties
        public IntervalMouseAction CurrentIntervalMouseAction { get; set; }

        public double VideoDurationMsec { get; set; }
        #endregion

        #region Canvas Drawing

        public void Draw()
        {
            DrawWaveForm();
            DrawSpaces();
        }

        /// <summary>
        /// Draws the waveform for the current window of sound and adds it to the AudioCanvas.
        /// </summary>
        public void DrawWaveForm()
        {
            if (_viewModel == null || _viewModel.Waveform == null || Width == 0 || _visibleWidth == 0
                || _viewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return;

            if (Children.Contains(_waveformImage))
                Children.Remove(_waveformImage);

            var data = _viewModel.Waveform.Data;
            double samplesPerPixel = Math.Max(data.Count / Width, 1);
            double middle = ActualHeight / 2;
            double yscale = middle;

            int beginPixel = (int)_visibleX;
            int endPixel = beginPixel + (int)_visibleWidth;

            int ratio = _viewModel.Waveform.Header.Channels == 2 ? 40 : 80;
            double samplesPerSecond =
                (_viewModel.Waveform.Header.SampleRate * (_viewModel.Waveform.Header.BlockAlign / (double)ratio));

            var waveformLineGroup = new GeometryGroup();

            double absMin = 0;

            for (int pixel = beginPixel; pixel <= endPixel; pixel++)
            {
                double offsetTime = (VideoDurationMsec / (Width * Milliseconds.PerSecond))
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

            _waveformImage = waveformLineGroup.CreateImage(Brushes.Black, _linePen);

            SetLeft(_waveformImage, beginPixel);
            SetTop(_waveformImage, middle + absMin * yscale);

            Children.Add(_waveformImage);
        }

        public void DrawSpaces()
        {
            if (_viewModel == null || _viewModel.Spaces == null || Width == 0 || _visibleWidth == 0
                || _viewModel.Player.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return;

            if (Children.Contains(_completedSpaceImage))
                Children.Remove(_completedSpaceImage);
            if (Children.Contains(_spaceImage))
                Children.Remove(_spaceImage);
            if (Children.Contains(_selectedImage))
                Children.Remove(_selectedImage);

            /* The way this method draws spaces is as follows: There are 3 different images drawn:
             * 1 for the selected item if any, 1 for completed spaces, and one for any other spaces.
             * They are drawn and overlayed on top of each other in the order: Completed -> Spaces
             * -> SelectedSpace.
             */

            var spaceGroup = new GeometryGroup { FillRule = FillRule.Nonzero };
            var completedSpaceGroup = new GeometryGroup { FillRule = FillRule.Nonzero };
            var selectedGroup = new GeometryGroup();

            double beginPixel = _visibleX;
            double endPixel = beginPixel + _visibleWidth;

            double beginTimeMsec = (VideoDurationMsec / (Width)) * beginPixel;
            double endTimeMsec = (VideoDurationMsec / (Width)) * endPixel;

            foreach (var space in _viewModel.Spaces)
            {
                if (IsIntervalVisible(space, beginTimeMsec, endTimeMsec))
                {
                    var rect = new RectangleGeometry(new Rect(space.X, space.Y, (space).Width, space.Height));

                    if (space.IsSelected)
                        selectedGroup.Children.Add(rect);
                    else if (space.IsRecordedOver)
                        completedSpaceGroup.Children.Add(rect);
                    else
                        spaceGroup.Children.Add(rect);
                }
            }

            if (0 < completedSpaceGroup.Children.Count)
            {
                _completedSpaceImage = completedSpaceGroup.CreateImage(_completedSpaceBrush, _linePen);

                Children.Add(_completedSpaceImage);

                double minX = completedSpaceGroup.Children[0].Bounds.X;
                for (int i = 1; i < completedSpaceGroup.Children.Count; i++)
                {
                    minX = Math.Min(minX, completedSpaceGroup.Children[i].Bounds.X);
                }

                SetLeft(_completedSpaceImage, minX);
                SetTop(_completedSpaceImage, 0);
            }

            if (0 < spaceGroup.Children.Count)
            {
                _spaceImage = spaceGroup.CreateImage(_spaceBrush, _linePen);

                Children.Add(_spaceImage);

                //The Image has to be set to the smallest X value of the visible spaces.
                double minX = spaceGroup.Children[0].Bounds.X;
                for (int i = 1; i < spaceGroup.Children.Count; i++)
                {
                    minX = Math.Min(minX, spaceGroup.Children[i].Bounds.X);
                }

                SetLeft(_spaceImage, minX);
                SetTop(_spaceImage, 0);
            }

            if (0 < selectedGroup.Children.Count)
            {
                _selectedImage = selectedGroup.CreateImage(_selectedItemBrush, _linePen);

                Children.Add(_selectedImage);

                //There can only be one selected item
                SetLeft(_selectedImage, selectedGroup.Children[0].Bounds.X);
                SetTop(_selectedImage, 0);
            }
        }

        /// <summary>
        /// Draws the selectedItem image based off of mouse selection. Call this method to only
        /// draw the currently selected space.
        /// </summary>
        public void DrawMouseSelection()
        {
            if (_mouseSelection.Action == IntervalMouseAction.None)
                return;

            if (Children.Contains(_selectedImage))
                Children.Remove(_selectedImage);

            var selectedGroup = new GeometryGroup();
            selectedGroup.Children.Add(new RectangleGeometry(new Rect
            {
                X = _mouseSelection.Item.X,
                Y = _mouseSelection.Item.Y,
                Width = _mouseSelection.Item.Width,
                Height = _mouseSelection.Item.Height,
            }));

            _selectedImage = selectedGroup.CreateImage(_selectedItemBrush, _linePen);

            Children.Add(_selectedImage);

            SetLeft(_selectedImage, selectedGroup.Children[0].Bounds.X);
            SetTop(_selectedImage, 0);
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

            _completedSpaceBrush = new SolidColorBrush(Settings.Default.ColourScheme.CompletedSpaceColour);
            _completedSpaceBrush.Freeze();

            Draw();
        }
        #endregion

        #region Mouse Interaction
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (_viewModel == null)
                return;

            var point = e.GetPosition(this);

            foreach (var space in _viewModel.Spaces)
            {
                if (space.X - SelectionPixelWidth <= point.X && point.X <= space.X + space.Width + SelectionPixelWidth)
                    SelectSpace(space, point);
                else
                    space.IsSelected = false;
            }

            DrawSpaces();
        }

        private void SelectSpace(Space space, Point clickPoint)
        {
            space.IsSelected = true;
            space.MouseDownCommand.Execute();

            if (space.X - SelectionPixelWidth <= clickPoint.X && clickPoint.X <= space.X + SelectionPixelWidth)
            {
                _mouseSelection = new CanvasMouseSelection(IntervalMouseAction.ChangeStartTime, space,
                    XPosToMilliseconds(clickPoint.X) - space.StartInVideo);
            }
            else if (space.X + space.Width - SelectionPixelWidth <= clickPoint.X
                && clickPoint.X <= space.X + space.Width + SelectionPixelWidth)
            {
                _mouseSelection = new CanvasMouseSelection(IntervalMouseAction.ChangeEndTime, space,
                    XPosToMilliseconds(clickPoint.X) - space.EndInVideo);
            }
            else
            {
                _mouseSelection = new CanvasMouseSelection(IntervalMouseAction.Dragging, space,
                    XPosToMilliseconds(clickPoint.X) - space.StartInVideo);
            }

            CaptureMouse();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (_mouseSelection.Action != IntervalMouseAction.None)
            {
                _mouseSelection = CanvasMouseSelection.NoSelection;
                Mouse.Capture(null);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var mousePos = e.GetPosition(this);
            double startTime;

            switch (_mouseSelection.Action)
            {
                case IntervalMouseAction.None:
                    return;
                case IntervalMouseAction.Dragging:
                    //Ensure that the space can not be moved to an invalid time.
                    startTime = BoundBetween(0, XPosToMilliseconds(mousePos.X) - _mouseSelection.MouseClickTimeDifference,
                        VideoDurationMsec - _mouseSelection.Item.Duration);
                    _mouseSelection.Item.MoveInterval(startTime);
                    DrawMouseSelection();
                    break;
                case IntervalMouseAction.ChangeStartTime:
                    startTime = BoundBetween(0, XPosToMilliseconds(mousePos.X) - _mouseSelection.MouseClickTimeDifference,
                        _mouseSelection.Item.EndInVideo - MinIntervalDurationMsec);
                    _mouseSelection.Item.StartInVideo = startTime;
                    DrawMouseSelection();
                    break;
                case IntervalMouseAction.ChangeEndTime:
                    double endTime = BoundBetween(_mouseSelection.Item.StartInVideo + MinIntervalDurationMsec,
                        XPosToMilliseconds(mousePos.X) - _mouseSelection.MouseClickTimeDifference, VideoDurationMsec);
                    _mouseSelection.Item.EndInVideo = endTime;
                    DrawMouseSelection();
                    break;
            }
        }
        #endregion

        private double XPosToMilliseconds(double x)
        {
            return (VideoDurationMsec / (Width)) * x;
        }

        public void SetVisibleBoundaries(double visibleX, double visibleWidth)
        {
            _visibleX = visibleX;
            _visibleWidth = visibleWidth;
        }

        /// <summary>
        /// Ensures that the given value is bounded within the given lower and upper bounds.
        /// </summary>
        /// <param name="lowerBound">The lowest number that the value can be.</param>
        /// <param name="value">The value to bounds check.</param>
        /// <param name="upperBound">The highest number that the value can be.</param>
        /// <returns>The value bounded between the two bounds.</returns>
        public double BoundBetween(double lowerBound, double value, double upperBound)
        {
            return Math.Min(upperBound, Math.Max(lowerBound, value));
        }

        #region Event Handlers
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as AudioCanvasViewModel;
        }
        #endregion
    }
}

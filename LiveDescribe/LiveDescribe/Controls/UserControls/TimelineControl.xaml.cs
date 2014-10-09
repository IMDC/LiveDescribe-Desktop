using GalaSoft.MvvmLight.Threading;
using LiveDescribe.Extensions;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LiveDescribe.Controls.UserControls
{
    /// <summary>
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class TimelineControl : UserControl
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constants
        private const double MarkerOffset = 10.0;
        /// <summary>
        /// when the marker hits 95% of the page it scrolls
        /// </summary>
        private const double PageScrollPercentLimit = 0.95;
        private const double PageScrollPercentAmount = 0.90;
        /// <summary>30 seconds page time before audiocanvas & descriptioncanvas scroll</summary>
        private const double PageTimeBeforeCanvasScrolls = 30;
        /// <summary>The smallest possible width for an interval.</summary>
        private const double MinIntervalWidth = 8;
        #endregion

        #region Field
        private double _canvasWidth;
        private double _videoDurationMsec;
        private TimelineViewModel _viewmodel;
        #endregion

        #region Constructor and Initializers
        public TimelineControl()
        {
            InitializeComponent();

            DataContextChanged += (sender, args) =>
            {
                _viewmodel = args.NewValue as TimelineViewModel;

                if (_viewmodel == null)
                    return;

                /* The UI events are initialized here because they can not be passed in by the
                 * constructor.
                 */
                InitEventHandlers();
            };
        }

        private void InitEventHandlers()
        {
            #region MediaViewModel Events
            _viewmodel.MediaViewModel.VideoOpenedRequested += (o, args) =>
            {
                _videoDurationMsec = _viewmodel.MediaViewModel.MediaVideo.DurationMilliseconds;

                AudioCanvas.VideoDurationMsec = _videoDurationMsec;
                DescriptionCanvas.VideoDurationMsec = _videoDurationMsec;
                NumberLineCanvas.VideoDurationMsec = _videoDurationMsec;

                MarkerControl.Marker.IsEnabled = true;

                /* The descriptions and timeline are set and drawn when the video is loaded, as
                    * opposed to when the project is loaded because it is only at this time that
                    * we know the actual duration of the video, and therefore can calculate
                    * interval positions and canvas widths.
                    */
                CalculateCanvasWidth();
                SetTimelineWidth();

                foreach (var desc in _viewmodel.ProjectManager.AllDescriptions)
                    SetDescriptionLocation(desc);

                foreach (var space in _viewmodel.ProjectManager.Spaces)
                    SetSpaceLocation(space);

                DrawTimeline();
            };

            //captures the mouse when a mousedown request is sent to the Marker
            _viewmodel.MediaViewModel.OnMarkerMouseDownRequested += (sender, args) => MarkerControl.Marker.CaptureMouse();

            //updates the video position when the mouse is released on the Marker
            _viewmodel.MediaViewModel.OnMarkerMouseUpRequested += (sender, args) => MarkerControl.Marker.ReleaseMouseCapture();

            //updates the canvas and video position when the Marker is moved
            _viewmodel.MediaViewModel.OnMarkerMouseMoveRequested += (sender, e) =>
            {
                if (!MarkerControl.Marker.IsMouseCaptured)
                    return;

                if (ScrollRightIfCanForGraphicsThread(Canvas.GetLeft(MarkerControl.Marker)))
                    return;

                if (ScrollLeftIfCanForGraphicsThread(Canvas.GetLeft(MarkerControl.Marker)))
                    return;

                var xPosition = Mouse.GetPosition(AudioCanvas).X;
                var middleOfMarker = xPosition - MarkerOffset;

                //make sure the middle of the marker doesn't go below the beginning of the canvas
                if (xPosition < -MarkerOffset)
                {
                    Canvas.SetLeft(MarkerControl.Marker, -MarkerOffset);
                    UpdateVideoPosition(0);
                    return;
                }

                var newPositionInVideo = (xPosition / _canvasWidth) * _videoDurationMsec;
                if (newPositionInVideo >= _videoDurationMsec)
                {
                    var newPositionOfMarker = (_canvasWidth / _videoDurationMsec) * (_videoDurationMsec);
                    Canvas.SetLeft(MarkerControl.Marker, newPositionOfMarker - MarkerOffset);
                    UpdateVideoPosition((int)(_videoDurationMsec));
                    return;
                }
                Canvas.SetLeft(MarkerControl.Marker, middleOfMarker);
                UpdateVideoPosition((int)newPositionInVideo);
            };

            #endregion

            #region NumberLineCanvas Events
            NumberLineCanvas.MouseDown += (sender, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    //execute the pause command because we want to pause the video when someone is clicking through the video
                    _viewmodel.MediaViewModel.PauseCommand.Execute();
                    var xPosition = e.GetPosition(NumberLineCanvas).X;
                    var newValue = (xPosition / _canvasWidth) * _videoDurationMsec;

                    UpdateMarkerPosition(xPosition - MarkerOffset);
                    UpdateVideoPosition((int)newValue);
                }
            };
            #endregion

            #region ProjectManager Events

            _viewmodel.ProjectManager.ProjectClosed += (sender, args) =>
            {
                DrawTimeline();

                UpdateMarkerPosition(-MarkerOffset);
                MarkerControl.Marker.IsEnabled = false;
            };

            #endregion

            #region TimelineScrollViewer Events
            TimeLineScrollViewer.ScrollChanged += (sender, args) =>
                {
                    //Update visible canvas boundaries
                    AudioCanvas.SetVisibleBoundaries(TimeLineScrollViewer.HorizontalOffset,
                        TimeLineScrollViewer.ActualWidth);
                    DescriptionCanvas.SetVisibleBoundaries(TimeLineScrollViewer.HorizontalOffset,
                        TimeLineScrollViewer.ActualWidth);
                    NumberLineCanvas.SetVisibleBoundaries(TimeLineScrollViewer.HorizontalOffset,
                        TimeLineScrollViewer.ActualWidth);

                    DrawTimeline();
                };

            TimeLineScrollViewer.SizeChanged += (sender, args) =>
            {
                //Video is loaded
                if (_viewmodel.MediaViewModel.MediaVideo.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                    SetTimelineWidthAndDraw();

                //update marker to fit the entire AudioCanvas even when there's no video loaded
                MarkerControl.Marker.Points[4] = new Point(MarkerControl.Marker.Points[4].X,
                    TimeLineScrollViewer.ActualHeight);
            };
            #endregion
        }
        #endregion

        #region Scrolling
        private bool ScrollRightIfCan(double xPos)
        {
            //all calls to dispatcher.Invoke are to get or set values that are located in the UI thread
            //because this method is located in the Play_Tick method which runs in a separate thread
            //that is how you must get/set the values

            double singlePageWidth = 0;
            double scrolledAmount = 0;

            DispatcherHelper.UIDispatcher.Invoke(() =>
            {
                singlePageWidth = TimeLineScrollViewer.ActualWidth;
                scrolledAmount = TimeLineScrollViewer.HorizontalOffset;
            });
            double scrollOffsetRight = PageScrollPercentLimit * singlePageWidth;
            if (!((xPos - scrolledAmount) >= scrollOffsetRight)) return false;
            DispatcherHelper.UIDispatcher.Invoke(() => TimeLineScrollViewer.ScrollToHorizontalOffset(scrollOffsetRight + scrolledAmount));
            return true;
        }


        private bool ScrollRightIfCanForGraphicsThread(double xPos)
        {
            double singlePageWidth = 0;
            double scrolledAmount = 0;
            singlePageWidth = TimeLineScrollViewer.ActualWidth;
            scrolledAmount = TimeLineScrollViewer.HorizontalOffset;
            double scrollOffsetRight = PageScrollPercentLimit * singlePageWidth;

            if (scrolledAmount >= TimeLineScrollViewer.ScrollableWidth)
                return false;

            if (!((xPos - scrolledAmount) >= scrollOffsetRight)) return false;
            TimeLineScrollViewer.ScrollToHorizontalOffset((PageScrollPercentAmount * singlePageWidth) + scrolledAmount);
            return true;
        }

        private bool ScrollLeftIfCanForGraphicsThread(double xPos)
        {
            double singlePageWidth = TimeLineScrollViewer.ActualWidth;
            double scrolledAmount = TimeLineScrollViewer.HorizontalOffset;

            //we can't scroll left cause we already scrolled as far left as possible
            if (scrolledAmount == 0)
                return false;

            double scrollOffsetLeft = (1 - PageScrollPercentLimit) * singlePageWidth;
            if (!((xPos - scrolledAmount) <= scrollOffsetLeft)) return false;
            TimeLineScrollViewer.ScrollToHorizontalOffset(scrolledAmount - (PageScrollPercentAmount * singlePageWidth));
            return true;
        }
        #endregion

        #region Interval Interaction and Modification
        public void AddDescriptionEventHandlers(Description description)
        {
            /* Draw the description only if the video is loaded, because there is currently
             * an issue with the video loading after the descriptions are added from an
             * opened project.
             */
            if (_viewmodel.MediaViewModel.MediaVideo.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                SetDescriptionLocation(description);

            description.NavigateToRequested += (sender1, e1) =>
            {
                UpdateMarkerPosition((description.StartInVideo / _videoDurationMsec) * (AudioCanvas.Width) - MarkerOffset);
                UpdateVideoPosition((int)description.StartInVideo);
                //Scroll 1 second before the start in video of the space
                TimeLineScrollViewer.ScrollToHorizontalOffset((AudioCanvas.Width / _videoDurationMsec) *
                    (description.StartInVideo - 1000));
            };

            description.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsSelected")
                    Dispatcher.Invoke(() => DescriptionCanvas.Draw());
            };

            description.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "StartInVideo"
                    || e.PropertyName == "EndInVideo"
                    || e.PropertyName == "SetStartAndEndInVideo")
                    SetDescriptionLocation(description);
            };
        }

        public void AddSpaceEventHandlers(Space space)
        {
            //Adding a space depends on where you right clicked so we create and add it in the view
            //Set space only if the video is loaded/playing/recording/etc
            if (_viewmodel.MediaViewModel.MediaVideo.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                SetSpaceLocation(space);

            space.NavigateToRequested += (o, args) =>
            {
                UpdateMarkerPosition((space.StartInVideo / _videoDurationMsec) * (AudioCanvas.Width) - MarkerOffset);
                UpdateVideoPosition((int)space.StartInVideo);
                //Scroll 1 second before the start in video of the space
                TimeLineScrollViewer.ScrollToHorizontalOffset(
                    (AudioCanvas.Width / _videoDurationMsec) * (space.StartInVideo - 1000));
            };

            space.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsSelected" || args.PropertyName == "IsRecordedOver")
                    Dispatcher.Invoke(() => AudioCanvas.DrawSpaces());
            };

            space.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "StartInVideo"
                    || e.PropertyName == "EndInVideo"
                    || e.PropertyName == "SetStartAndEndInVideo")
                    SetSpaceLocation(space);
            };
        }

        private void SetSpaceLocation(Space space)
        {
            SetIntervalLocation(space, AudioCanvas);
        }

        private void SetDescriptionLocation(Description description)
        {
            SetIntervalLocation(description, DescriptionCanvas);
        }

        private void SetIntervalLocation(IDescribableInterval interval, Canvas containingCanvas)
        {
            interval.X = (containingCanvas.Width / _videoDurationMsec) * interval.StartInVideo;
            interval.Y = 0;
            interval.Height = containingCanvas.ActualHeight;
            /* Set interval to a minimum width so that all descriptions, even those with 0 duration
             * (ie extended descriptions) are still visible.
             */
            interval.Width = Math.Max(MinIntervalWidth,
                (containingCanvas.Width / _videoDurationMsec) * (interval.EndInVideo - interval.StartInVideo));
        }
        #endregion

        #region Timeline Updating and Drawing
        private void CalculateCanvasWidth()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            _canvasWidth = (_videoDurationMsec / (PageTimeBeforeCanvasScrolls * 1000)) * screenWidth;
        }

        /// <summary>
        /// Update's the instance variables that keep track of the timeline height and width, and
        /// calculates the size of the timeline if the width of the audio canvas is greater then the
        /// timeline width it automatically overflows and scrolls due to the scrollview then update
        /// the width of the marker to match the audio canvas
        /// </summary>
        private void SetTimelineWidthAndDraw()
        {
            SetTimelineWidth();
            DrawTimeline();
        }

        private void SetTimelineWidth()
        {
            NumberLineCanvas.Width = _canvasWidth;
            DescriptionCanvas.Width = _canvasWidth;
            AudioCanvas.Width = _canvasWidth;
        }

        private void DrawTimeline()
        {
            AudioCanvas.Draw();
            DescriptionCanvas.Draw();
            NumberLineCanvas.Draw();
        }

        public void UpdateTimelinePosition()
        {
            try
            {
                //This method runs on a separate thread therefore all calls to get values or set
                //values that are located on the UI thread must be gotten with Dispatcher.Invoke
                double canvasLeft = 0;
                DispatcherHelper.UIDispatcher.Invoke(() => { canvasLeft = Canvas.GetLeft(MarkerControl.Marker); });
                ScrollRightIfCan(canvasLeft);
                DispatcherHelper.UIDispatcher.Invoke(() =>
                {
                    double position = (_viewmodel.MediaViewModel.MediaVideo.Position.TotalMilliseconds / _videoDurationMsec) * (AudioCanvas.Width);
                    UpdateMarkerPosition(position - MarkerOffset);
                });
            }
            catch (System.Threading.Tasks.TaskCanceledException exception)
            {
                //do nothing this exception is thrown when the application is exited
                Log.Warn("Task Cancelled exception", exception);
            }
        }

        public void ResetMarkerPosition()
        {
            UpdateMarkerPosition(-MarkerOffset);
        }

        /// <summary>
        /// Updates the Marker Position in the timeline and sets the corresponding time in the timelabel
        /// </summary>
        /// <param name="xPos">the x position in which the marker is supposed to move</param>
        private void UpdateMarkerPosition(double xPos)
        {
            Canvas.SetLeft(MarkerControl.Marker, xPos);
            _viewmodel.MediaViewModel.PositionTimeLabel = _viewmodel.MediaViewModel.MediaVideo.Position;
        }

        /// <summary>
        /// Updates the video position to a spot in the video
        /// </summary>
        /// <param name="vidPos">the new position in the video</param>
        private void UpdateVideoPosition(int vidPos)
        {
            _viewmodel.MediaViewModel.MediaVideo.Position = new TimeSpan(0, 0, 0, 0, vidPos);
            _viewmodel.MediaViewModel.PositionTimeLabel = _viewmodel.MediaViewModel.MediaVideo.Position;
        }
        #endregion
    }
}

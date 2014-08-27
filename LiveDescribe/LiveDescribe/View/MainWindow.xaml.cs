using GalaSoft.MvvmLight.Threading;
using LiveDescribe.Controls;
using LiveDescribe.Converters;
using LiveDescribe.Extensions;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using LiveDescribe.Resources;
using LiveDescribe.Resources.UiStrings;
using LiveDescribe.ViewModel;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constants
        private const double DefaultSpaceLengthInMilliSeconds = 3000;
        private const double MarkerOffset = 10.0;
        /// <summary>
        /// when the marker hits 95% of the page it scrolls
        /// </summary>
        private const double PageScrollPercentLimit = 0.95;
        private const double PageScrollPercentAmount = 0.90;
        /// <summary>30 seconds page time before audiocanvas & descriptioncanvas scroll</summary>
        private const double PageTimeBeforeCanvasScrolls = 30;
        private const double LineTime = 1; //each line in the NumberLineCanvas appears every 1 second
        private const int LongLineTime = 5; // every 5 LineTimes, you get a Longer Line
        #endregion

        #region Instance Variables
        /// <summary>
        /// The width of the entire canvas.
        /// </summary>
        private double _canvasWidth;
        private double _videoDuration = -1;
        private readonly MediaControlViewModel _mediaControlViewModel;
        private readonly ProjectManager _projectManager;
        private readonly DescriptionInfoTabViewModel _descriptionInfoTabViewModel;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly MillisecondsTimeConverterFormatter _millisecondsTimeConverter;
        private Point _rightClickPointOnAudioCanvas;
        private readonly LiveDescribeMediaPlayer _videoMedia;

        private readonly ItemCanvas _descriptionCanvas;
        private readonly Polyline _marker;
        #endregion

        public MainWindow()
        {
            var splashscreen = new SplashScreen("../Resources/Images/LiveDescribe-Splashscreen.png");
            splashscreen.Show(true);
            CustomResources.LoadResources();

            if (!Defines.Debug)
                System.Threading.Thread.Sleep(2000);

            InitializeComponent();

            Settings.Default.Upgrade();
            Settings.Default.InitializeDefaultValuesIfNull();

            _videoMedia = MediaControl.VideoMedia;

            var mainWindowViewModel = new MainWindowViewModel(_videoMedia);

            DataContext = mainWindowViewModel;
            _mainWindowViewModel = mainWindowViewModel;

            _mediaControlViewModel = mainWindowViewModel.MediaControlViewModel;
            _projectManager = mainWindowViewModel.ProjectManager;
            _descriptionInfoTabViewModel = mainWindowViewModel.DescriptionInfoTabViewModel;

            _descriptionCanvas = DescriptionCanvasControl.DescriptionCanvas;
            _descriptionCanvas.UndoRedoManager = mainWindowViewModel.UndoRedoManager;

            _marker = MarkerControl.Marker;

            _millisecondsTimeConverter = new MillisecondsTimeConverterFormatter();

            SetRecentDocumentsList();

            #region TimeLineScrollViewer Event Listeners
            TimeLineScrollViewer.ScrollChanged += (sender, e) =>
            {
                //Update visible canvas boundaries
                AudioCanvas.SetVisibleBoundaries(TimeLineScrollViewer.HorizontalOffset, TimeLineScrollViewer.ActualWidth);

                DrawTimeline();
            };
            #endregion

            #region Event Listeners for VideoMedia
            //if the videomedia's path changes (a video is added)
            //then play and stop the video to load the video so the VideoOpenedRequest event gets fired
            //this is done because the MediaElement does not load the video until it is played
            //therefore you can't know the duration of the video or if it hasn't been loaded properly unless it fails
            //I know this is a little hackish, but it's a consequence of how the MediaElement works
            _videoMedia.PathChangedEvent += (sender, e) =>
                {
                    _videoMedia.Play();
                    _videoMedia.Pause();
                };
            #endregion

            #region Event Listeners For MainWindowViewModel (Pause, Play, Mute)
            //These events are put inside the main control because they will also effect the list
            //of audio descriptions an instance of DescriptionCollectionViewModel is inside the main control
            //and the main control will take care of synchronizing the video, and the descriptions

            //listens for PlayRequested Event
            mainWindowViewModel.PlayRequested += (sender, e) => CommandManager.InvalidateRequerySuggested();

            //listens for PauseRequested Event
            mainWindowViewModel.PauseRequested += (sender, e) => CommandManager.InvalidateRequerySuggested();

            //listens for when the media has gone all the way to the end
            mainWindowViewModel.MediaEnded += (sender, e) =>
                {
                    UpdateMarkerPosition(-MarkerOffset);
                    //this is to recheck all the graphics states
                    CommandManager.InvalidateRequerySuggested();
                };

            mainWindowViewModel.GraphicsTick += Play_Tick;

            mainWindowViewModel.PlayingDescription += (sender, args) =>
            {
                try
                {
                    if (args.Value.IsExtendedDescription)
                        DispatcherHelper.UIDispatcher.Invoke(() =>
                            SpaceAndDescriptionsTabControl.ExtendedDescriptionsListView.ScrollToCenterOfView(args.Value));
                    else
                        DispatcherHelper.UIDispatcher.Invoke(() =>
                       SpaceAndDescriptionsTabControl.DescriptionsListView.ScrollToCenterOfView(args.Value));
                }
                catch (Exception exception)
                {
                    Log.Warn("Task Cancelled exception", exception);
                }
            };
            #endregion

            #region Event Listeners For MediaControlViewModel

            //listens for VideoOpenedRequested event
            //this event only gets thrown when if the MediaFailed event doesn't occur
            //and as soon as the video is loaded when play is pressed
            mainWindowViewModel.MediaControlViewModel.VideoOpenedRequested += (sender, e) =>
                {
                    _videoDuration = _videoMedia.NaturalDuration.TimeSpan.TotalMilliseconds;
                    AudioCanvas.VideoDurationMsec = _videoDuration;
                    DescriptionCanvasControl.DescriptionCanvas.VideoDuration = _videoDuration;
                    _canvasWidth = CalculateWidth();
                    _marker.IsEnabled = true;

                    //Video gets played and paused so you can seek initially when the video gets loaded
                    _videoMedia.Play();
                    _videoMedia.Pause();

                    SetTimeline();

                    foreach (var desc in _projectManager.AllDescriptions)
                        DrawDescribableInterval(desc);

                    foreach (var space in _projectManager.Spaces)
                        DrawDescribableInterval(space);
                };

            //captures the mouse when a mousedown request is sent to the Marker
            mainWindowViewModel.MediaControlViewModel.OnMarkerMouseDownRequested += (sender, e) => _marker.CaptureMouse();

            //updates the video position when the mouse is released on the Marker
            mainWindowViewModel.MediaControlViewModel.OnMarkerMouseUpRequested += (sender, e) => _marker.ReleaseMouseCapture();

            //updates the canvas and video position when the Marker is moved
            mainWindowViewModel.MediaControlViewModel.OnMarkerMouseMoveRequested += (sender, e) =>
                {
                    if (!_marker.IsMouseCaptured) return;

                    if (ScrollRightIfCanForGraphicsThread(Canvas.GetLeft(_marker)))
                        return;

                    if (ScrollLeftIfCanForGraphicsThread(Canvas.GetLeft(_marker)))
                        return;

                    var xPosition = Mouse.GetPosition(AudioCanvas).X;
                    var middleOfMarker = xPosition - MarkerOffset;

                    //make sure the middle of the marker doesn't go below the beginning of the canvas
                    if (xPosition < -MarkerOffset)
                    {
                        Canvas.SetLeft(_marker, -MarkerOffset);
                        UpdateVideoPosition(0);
                        return;
                    }

                    var newPositionInVideo = (xPosition / _canvasWidth) * _videoDuration;
                    if (newPositionInVideo >= _videoDuration)
                    {
                        var newPositionOfMarker = (_canvasWidth / _videoDuration) * (_videoDuration);
                        Canvas.SetLeft(_marker, newPositionOfMarker - MarkerOffset);
                        UpdateVideoPosition((int)(_videoDuration));
                        return;
                    }
                    Canvas.SetLeft(_marker, middleOfMarker);
                    UpdateVideoPosition((int)newPositionInVideo);
                };

            #endregion

            #region Event Listeners For AudioCanvasViewModel
            AudioCanvasViewModel audioCanvasViewModel = mainWindowViewModel.AudioCanvasViewModel;
            audioCanvasViewModel.AudioCanvasMouseDownEvent += AudioCanvas_OnMouseDown;
            audioCanvasViewModel.AudioCanvasMouseRightButtonDownEvent += AudioCanvas_RecordRightClickPosition;

            _mainWindowViewModel.AudioCanvasViewModel.RequestSpaceTime += (sender, args) =>
            {
                var space = args.Value;

                double middle = _rightClickPointOnAudioCanvas.X;  // going to be the middle of the space
                double middleTime = (_videoDuration / AudioCanvas.Width) * middle;  // middle of the space in milliseconds
                double starttime = middleTime - (DefaultSpaceLengthInMilliSeconds / 2);
                double endtime = middleTime + (DefaultSpaceLengthInMilliSeconds / 2);

                //Bounds checking when creating a space
                if (starttime >= 0 && endtime <= _videoDuration)
                {
                    space.StartInVideo = starttime;
                    space.EndInVideo = endtime;
                }
                else if (starttime < 0 && endtime > _videoDuration)
                {
                    space.StartInVideo = 0;
                    space.EndInVideo = _videoDuration;
                }
                else if (starttime < 0)
                {
                    space.StartInVideo = 0;
                    space.EndInVideo = endtime;
                }
                else if (endtime > _videoDuration)
                {
                    space.StartInVideo = starttime;
                    space.EndInVideo = _videoDuration;
                }
            };
            #endregion

            #region Event Listeners For DescriptionCanvasViewModel
            DescriptionCanvasViewModel descriptionCanvasViewModel = mainWindowViewModel.DescriptionCanvasViewModel;
            descriptionCanvasViewModel.DescriptionCanvasMouseDownEvent += DescriptionCanvas_MouseDown;
            #endregion

            #region Event Handlers for ProjectManager

            //When a description is added, attach an event to the StartInVideo and EndInVideo properties
            //so when those properties change it redraws them
            _projectManager.AllDescriptions.CollectionChanged += (sender, e) =>
            {
                if (e.Action != NotifyCollectionChangedAction.Add)
                    return;

                foreach (Description d in e.NewItems)
                    AddDescriptionEventHandlers(d);
            };

            _projectManager.Spaces.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (Space space in e.NewItems)
                        AddSpaceEventHandlers(space);
                }
            };

            _projectManager.ProjectLoaded += (sender, e) => SetTimeline();

            _projectManager.ProjectClosed += (sender, e) =>
            {
                AudioCanvas.Children.Clear();
                NumberLineCanvas.Children.Clear();

                UpdateMarkerPosition(-MarkerOffset);
                _marker.IsEnabled = false;
            };
            #endregion

            #region Event Handlers for Settings
            Settings.Default.RecentProjects.CollectionChanged += (sender, args) => SetRecentDocumentsList();
            #endregion
        }

        #region Play_Tick
        /// <summary>
        /// Updates the Marker Position on the Timer
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void Play_Tick(object sender, EventArgs e)
        {
            try
            {
                //This method runs on a separate thread therefore all calls to get values or set
                //values that are located on the UI thread must be gotten with Dispatcher.Invoke
                double canvasLeft = 0;
                DispatcherHelper.UIDispatcher.Invoke(() => { canvasLeft = Canvas.GetLeft(_marker); });
                ScrollRightIfCan(canvasLeft);
                DispatcherHelper.UIDispatcher.Invoke(() =>
                {
                    double position = (_videoMedia.Position.TotalMilliseconds / _videoDuration) * (AudioCanvas.Width);
                    UpdateMarkerPosition(position - MarkerOffset);
                });
            }
            catch (System.Threading.Tasks.TaskCanceledException exception)
            {
                //do nothing this exception is thrown when the application is exited
                Log.Warn("Task Cancelled exception", exception);
            }
        }
        #endregion

        #region View Listeners

        /// <summary>
        /// Updates TimlineScrollViewer's childrens' size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeLineScrollViewer_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Video is loaded
            if (_videoMedia.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                SetTimeline();

            //update marker to fit the entire AudioCanvas even when there's no video loaded
            _marker.Points[4] = new Point(_marker.Points[4].X, TimeLineScrollViewer.ActualHeight);
        }

        /// <summary>
        /// Gets called when the mouse is down on the audio canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioCanvas_OnMouseDown(object sender, MouseEventArgs e)
        {
            //if we aren't dragging a description or space, we want to unselect them out of the list
            if (AudioCanvas.CurrentIntervalMouseAction == IntervalMouseAction.None &&
                _descriptionCanvas.CurrentIntervalMouseAction == IntervalMouseAction.None)
                _descriptionInfoTabViewModel.ClearSelection();
        }

        private void DescriptionCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            //if we aren't dragging a description or space, we want to unselect them out of the list
            if (AudioCanvas.CurrentIntervalMouseAction == IntervalMouseAction.None &&
                _descriptionCanvas.CurrentIntervalMouseAction == IntervalMouseAction.None)
                _descriptionInfoTabViewModel.ClearSelection();
        }

        private void AudioCanvas_RecordRightClickPosition(object sender, EventArgs e)
        {
            //record the position in which you right clicked on the canvas
            //this position is used to calculate where on the audio canvas to draw a space
            var e1 = (MouseEventArgs)e;
            _rightClickPointOnAudioCanvas = e1.GetPosition(AudioCanvas);
        }

        /// <summary>
        /// Gets executed when the area in the NumberLineCanvas canvas gets clicked It changes the
        /// position of the video then redraws the marker in the correct spot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberTimeline_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //execute the pause command because we want to pause the video when someone is clicking through the video
                _mediaControlViewModel.PauseCommand.Execute();
                var xPosition = e.GetPosition(NumberLineCanvas).X;
                var newValue = (xPosition / _canvasWidth) * _videoDuration;

                UpdateMarkerPosition(xPosition - MarkerOffset);
                UpdateVideoPosition((int)newValue);
            }
        }

        #endregion

        #region Methods

        private void AddDescriptionEventHandlers(Description description)
        {
            /* Draw the description only if the video is loaded, because there is currently
             * an issue with the video loading after the descriptions are added from an
             * opened project.
             */
            if (_videoMedia.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                DrawDescribableInterval(description);

            description.MouseDown += (sender1, e1) =>
            {
                //Add mouse down event on every description here
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    if (description.IsExtendedDescription)
                    {
                        _descriptionInfoTabViewModel.SelectedExtendedDescription = description;
                        SpaceAndDescriptionsTabControl.ExtendedDescriptionsListView.ScrollToCenterOfView(description);
                    }
                    else
                    {
                        _descriptionInfoTabViewModel.SelectedRegularDescription = description;
                        SpaceAndDescriptionsTabControl.DescriptionsListView.ScrollToCenterOfView(description);
                    }
                }
            };

            description.NavigateToRequested += (sender1, e1) =>
            {
                UpdateMarkerPosition((description.StartInVideo / _videoDuration) * (AudioCanvas.Width) - MarkerOffset);
                UpdateVideoPosition((int)description.StartInVideo);
                //Scroll 1 second before the start in video of the space
                TimeLineScrollViewer.ScrollToHorizontalOffset((AudioCanvas.Width / _videoDuration) *
                                                              (description.StartInVideo - 1000));

                if (description.IsExtendedDescription)
                {
                    _descriptionInfoTabViewModel.SelectedExtendedDescription = description;
                    SpaceAndDescriptionsTabControl.ExtendedDescriptionsListView.ScrollToCenterOfView(description);
                }
                else
                {
                    _descriptionInfoTabViewModel.SelectedRegularDescription = description;
                    SpaceAndDescriptionsTabControl.DescriptionsListView.ScrollToCenterOfView(description);
                }
            };

            AddIntervalEventHandlers(description);
        }

        private void AddSpaceEventHandlers(Space space)
        {
            //Adding a space depends on where you right clicked so we create and add it in the view
            //Set space only if the video is loaded/playing/recording/etc
            if (_videoMedia.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                DrawDescribableInterval(space);

            space.MouseDown += (sender1, e1) =>
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    _descriptionInfoTabViewModel.SelectedSpace = space;
                    SpaceAndDescriptionsTabControl.SpacesListView.ScrollToCenterOfView(space);
                }
            };

            space.NavigateToRequested += (o, args) =>
            {
                UpdateMarkerPosition((space.StartInVideo / _videoDuration) * (AudioCanvas.Width) - MarkerOffset);
                UpdateVideoPosition((int)space.StartInVideo);
                //Scroll 1 second before the start in video of the space
                TimeLineScrollViewer.ScrollToHorizontalOffset((AudioCanvas.Width / _videoDuration) *
                                                              (space.StartInVideo - 1000));

                _descriptionInfoTabViewModel.SelectedSpace = space;
                SpaceAndDescriptionsTabControl.SpacesListView.ScrollToCenterOfView(space);
            };

            AddIntervalEventHandlers(space);
        }

        private void AddIntervalEventHandlers(IDescribableInterval interval)
        {
            interval.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "StartInVideo"
                    || e.PropertyName == "EndInVideo"
                    || e.PropertyName == "SetStartAndEndInVideo")
                    DrawDescribableInterval(interval);
            };
        }

        /// <summary>
        /// Updates the Marker Position in the timeline and sets the corresponding time in the timelabel
        /// </summary>
        /// <param name="xPos">the x position in which the marker is supposed to move</param>
        private void UpdateMarkerPosition(double xPos)
        {
            Canvas.SetLeft(_marker, xPos);
            _mediaControlViewModel.PositionTimeLabel = _videoMedia.Position;
        }

        /// <summary>
        /// Updates the video position to a spot in the video
        /// </summary>
        /// <param name="vidPos">the new position in the video</param>
        private void UpdateVideoPosition(int vidPos)
        {
            _videoMedia.Position = new TimeSpan(0, 0, 0, 0, vidPos);
            _mediaControlViewModel.PositionTimeLabel = _videoMedia.Position;
        }

        /// <summary>
        /// This method is called inside the Play_Tick method which runs on a separate thread
        /// Scrolls the scrollviewer to the right as much as the PageScrollPercent when the marker
        /// reaches the PageScrollPercent of the width of the page
        /// </summary>
        /// <returns>true if it can scroll right</returns>
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

        /// <summary>
        /// Calculates the width required for the audioCanvas and then sets _canvasWidth to this value
        /// </summary>
        private double CalculateWidth()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double staticCanvasWidth = (_videoDuration / (PageTimeBeforeCanvasScrolls * 1000)) * screenWidth;
            AudioCanvas.MaxWidth = staticCanvasWidth;
            return staticCanvasWidth;
        }

        private void SetRecentDocumentsList()
        {
            OpenRecentMenuItem.Items.Clear();

            if (Settings.Default.RecentProjects.Count == 0)
            {
                OpenRecentMenuItem.IsEnabled = false;
            }
            else
            {
                int counter = 1;
                foreach (var namedFilePath in Settings.Default.RecentProjects)
                {
                    OpenRecentMenuItem.Items.Add(new MenuItem
                    {
                        Header = string.Format(UiStrings.MenuItem_Format_RecentProjectItem, counter, namedFilePath.Name),
                        ToolTip = namedFilePath.Path,
                        Command = _mainWindowViewModel.OpenProjectPath,
                        CommandParameter = namedFilePath.Path,
                    });
                    counter++;
                }

                OpenRecentMenuItem.Items.Add(new Separator());
                OpenRecentMenuItem.Items.Add(new MenuItem
                {
                    Header = UiStrings.MenuItem_ClearList,
                    Command = _mainWindowViewModel.ClearRecentProjects
                });

                OpenRecentMenuItem.IsEnabled = true;
            }
        }

        #endregion

        #region graphics Functions

        private void DrawDescribableInterval(IDescribableInterval interval)
        {
            interval.X = (AudioCanvas.Width / _videoDuration) * interval.StartInVideo;
            interval.Y = 0;
            interval.Height = AudioCanvas.ActualHeight;
            interval.Width = (AudioCanvas.Width / _videoDuration) * (interval.EndInVideo - interval.StartInVideo);
        }

        /// <summary>
        /// Resizes all the descriptions height to fit the description canvas
        /// </summary>
        private void ResizeDescriptions()
        {
            foreach (var description in _projectManager.AllDescriptions)
                description.Height = _descriptionCanvas.ActualHeight;
        }

        /// <summary>
        /// Update's the instance variables that keep track of the timeline height and width, and
        /// calculates the size of the timeline if the width of the audio canvas is greater then the
        /// timeline width it automatically overflows and scrolls due to the scrollview then update
        /// the width of the marker to match the audio canvas
        /// </summary>
        private void SetTimeline()
        {
            NumberLineCanvas.Width = _canvasWidth;
            _descriptionCanvas.Width = _canvasWidth;
            AudioCanvas.Width = _canvasWidth;

            DrawTimeline();
            ResizeDescriptions();
        }

        private void DrawTimeline()
        {
            AudioCanvas.Draw();
            NumberLineCanvas.DrawNumberTimeLine(TimeLineScrollViewer.HorizontalOffset,
                TimeLineScrollViewer.ActualWidth, _videoDuration);
        }

        #endregion

        #region Control Event Handlers
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            bool success = _mainWindowViewModel.TryExit();

            if (!success)
                e.Cancel = true;
            else
                Log.Info("Exiting program...");
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
    }
}
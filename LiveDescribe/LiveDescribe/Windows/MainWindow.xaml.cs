using GalaSoft.MvvmLight.Threading;
using LiveDescribe.Controls;
using LiveDescribe.Controls.Canvases;
using LiveDescribe.Controls.UserControls;
using LiveDescribe.Extensions;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using LiveDescribe.Resources;
using LiveDescribe.Resources.UiStrings;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace LiveDescribe.Windows
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

        #region Instance Variables
        /// <summary>
        /// The width of the entire canvas.
        /// </summary>
        private double _canvasWidth;
        private double _videoDuration = -1;
        private readonly MediaViewModel _mediaViewModel;
        private readonly ProjectManager _projectManager;
        private readonly IntervalInfoListViewModel _intervalInfoListViewModel;
        private readonly MainViewModel _mainViewModel;
        private readonly LiveDescribeMediaPlayer _videoMedia;

        private readonly Polyline _marker;
        #endregion

        public MainWindow()
        {
            var splashscreen = new SplashScreen("../Resources/Images/LiveDescribe-Splashscreen.png");
            splashscreen.Show(true);
            CustomResources.LoadResources();

            if (!Defines.Debug)
                System.Threading.Thread.Sleep(2000);

            Settings.Default.Upgrade();
            Settings.Default.InitializeDefaultValuesIfNull();

            InitializeComponent();

            //check which exporting options are available, depending on build
            if (Defines.Zagga)
            {
                ExportWithVideo.Visibility = Visibility.Visible;
                ExportAudioOnly.Visibility = Visibility.Collapsed;
            }
            else
            {
                ExportWithVideo.Visibility = Visibility.Collapsed;
                ExportAudioOnly.Visibility = Visibility.Visible;
            }

            _videoMedia = MediaControl.VideoMedia;

            var mainWindowViewModel = new MainViewModel(_videoMedia);

            DataContext = mainWindowViewModel;
            _mainViewModel = mainWindowViewModel;

            _mediaViewModel = mainWindowViewModel.MediaViewModel;
            _projectManager = mainWindowViewModel.ProjectManager;
            _intervalInfoListViewModel = mainWindowViewModel.IntervalInfoListViewModel;

            _marker = MarkerControl.Marker;

            SetRecentDocumentsList();

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

            #region Event Listeners For MainViewModel (Pause, Play, Mute)
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

            #region Event Listeners For MediaViewModel

            mainWindowViewModel.MediaViewModel.VideoOpenedRequested += (sender, e) =>
            {
                //Video gets played and paused so you can seek initially when the video gets loaded
                _videoMedia.Play();
                _videoMedia.Pause();
            };

            #endregion

            #region DescriptionCanvas Events
            //If one canvas is clicked, we want to unselect the other canvas' selection
            TimelineControl.DescriptionCanvas.MouseLeftButtonDown += (sender, args) =>
            {
                if (AudioCanvas.MouseAction == IntervalMouseAction.ItemSelected)
                    AudioCanvas.ClearMouseSelection();
            };

            TimelineControl.DescriptionCanvas.MouseLeftButtonUp += (sender, args) =>
            {
                if (DescriptionCanvas.MouseAction == IntervalMouseAction.None)
                    _intervalInfoListViewModel.ClearSelection();
            };
            #endregion

            #region AudioCanvas Events
            TimelineControl.AudioCanvas.MouseLeftButtonDown += (sender, args) =>
                {
                    if (DescriptionCanvas.MouseAction == IntervalMouseAction.ItemSelected)
                        DescriptionCanvas.ClearMouseSelection();
                };

            TimelineControl.AudioCanvas.MouseLeftButtonUp += (sender, args) =>
            {
                if (AudioCanvas.MouseAction == IntervalMouseAction.None)
                    _intervalInfoListViewModel.ClearSelection();
            };
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
            TimelineControl.UpdateTimelinePosition();
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
                SetDescriptionLocation(description);

            description.MouseDown += (sender1, e1) =>
            {
                //Add mouse down event on every description here
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    if (description.IsExtendedDescription)
                    {
                        _intervalInfoListViewModel.SelectedExtendedDescription = description;
                        SpaceAndDescriptionsTabControl.ExtendedDescriptionsListView.ScrollToCenterOfView(description);
                    }
                    else
                    {
                        _intervalInfoListViewModel.SelectedRegularDescription = description;
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
                    _intervalInfoListViewModel.SelectedExtendedDescription = description;
                    SpaceAndDescriptionsTabControl.ExtendedDescriptionsListView.ScrollToCenterOfView(description);
                }
                else
                {
                    _intervalInfoListViewModel.SelectedRegularDescription = description;
                    SpaceAndDescriptionsTabControl.DescriptionsListView.ScrollToCenterOfView(description);
                }
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

        private void AddSpaceEventHandlers(Space space)
        {
            //Adding a space depends on where you right clicked so we create and add it in the view
            //Set space only if the video is loaded/playing/recording/etc
            if (_videoMedia.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                SetSpaceLocation(space);

            space.MouseDown += (sender1, e1) =>
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    _intervalInfoListViewModel.SelectedSpace = space;
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

                _intervalInfoListViewModel.SelectedSpace = space;
                SpaceAndDescriptionsTabControl.SpacesListView.ScrollToCenterOfView(space);
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

        /// <summary>
        /// Updates the Marker Position in the timeline and sets the corresponding time in the timelabel
        /// </summary>
        /// <param name="xPos">the x position in which the marker is supposed to move</param>
        private void UpdateMarkerPosition(double xPos)
        {
            Canvas.SetLeft(_marker, xPos);
            _mediaViewModel.PositionTimeLabel = _videoMedia.Position;
        }

        /// <summary>
        /// Updates the video position to a spot in the video
        /// </summary>
        /// <param name="vidPos">the new position in the video</param>
        private void UpdateVideoPosition(int vidPos)
        {
            _videoMedia.Position = new TimeSpan(0, 0, 0, 0, vidPos);
            _mediaViewModel.PositionTimeLabel = _videoMedia.Position;
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
                        Command = _mainViewModel.OpenProjectPath,
                        CommandParameter = namedFilePath.Path,
                    });
                    counter++;
                }

                OpenRecentMenuItem.Items.Add(new Separator());
                OpenRecentMenuItem.Items.Add(new MenuItem
                {
                    Header = UiStrings.MenuItem_ClearList,
                    Command = _mainViewModel.ClearRecentProjects
                });

                OpenRecentMenuItem.IsEnabled = true;
            }
        }

        #endregion

        #region Location, Sizing, and Drawing Methods

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
            interval.X = (containingCanvas.Width / _videoDuration) * interval.StartInVideo;
            interval.Y = 0;
            interval.Height = containingCanvas.ActualHeight;
            /* Set interval to a minimum width so that all descriptions, even those with 0 duration
             * (ie extended descriptions) are still visible.
             */
            interval.Width = Math.Max(MinIntervalWidth,
                (containingCanvas.Width / _videoDuration) * (interval.EndInVideo - interval.StartInVideo));
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

        #endregion

        #region Control Event Handlers
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            bool success = _mainViewModel.TryExit();

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
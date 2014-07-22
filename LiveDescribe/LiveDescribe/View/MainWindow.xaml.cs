﻿using GalaSoft.MvvmLight.Threading;
using LiveDescribe.Controls;
using LiveDescribe.Converters;
using LiveDescribe.Extensions;
using LiveDescribe.Managers;
using LiveDescribe.Model;
using LiveDescribe.Utilities;
using LiveDescribe.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
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
        private const double LineTime = 1; //each line in the NumberTimeline appears every 1 second
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

        private readonly ItemCanvas _audioCanvas;
        private readonly ItemCanvas _descriptionCanvas;
        private readonly Polyline _marker;
        #endregion

        public MainWindow()
        {
            var splashscreen = new SplashScreen("../Resources/Images/LiveDescribe-Splashscreen.png");
            splashscreen.Show(true);
            CustomResources.LoadResources();
#if !DEBUG
            Thread.Sleep(2000);
#endif
            InitializeComponent();
#if ZAGGA
            DescriptionRecordingControl.ExtendedDescriptionCheckBox.Visibility = Visibility.Hidden;
            SpaceAndDescriptionsTabControl.ExtendedDescriptionsTabItem.Visibility = Visibility.Hidden;
#endif
            _videoMedia = MediaControl.VideoMedia;

            var mainWindowViewModel = new MainWindowViewModel(_videoMedia);

            DataContext = mainWindowViewModel;
            _mainWindowViewModel = mainWindowViewModel;

            _mediaControlViewModel = mainWindowViewModel.MediaControlViewModel;
            _projectManager = mainWindowViewModel.ProjectManager;
            _descriptionInfoTabViewModel = mainWindowViewModel.DescriptionInfoTabViewModel;

            _audioCanvas = AudioCanvasControl.AudioCanvas;
            _descriptionCanvas = DescriptionCanvasControl.DescriptionCanvas;
            _marker = MarkerControl.Marker;

            _millisecondsTimeConverter = new MillisecondsTimeConverterFormatter();

            #region TimeLineScrollViewer Event Listeners
            TimeLineScrollViewer.ScrollChanged += (sender, e) =>
            {
                DrawWaveForm();
                AddLinesToNumberTimeLine();
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

            mainWindowViewModel.ProjectClosed += (sender, e) =>
            {
                _audioCanvas.Children.Clear();
                NumberTimeline.Children.Clear();

                UpdateMarkerPosition(-MarkerOffset);
                _marker.IsEnabled = false;
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
                    AudioCanvasControl.AudioCanvas.VideoDuration = _videoDuration;
                    DescriptionCanvasControl.DescriptionCanvas.VideoDuration = _videoDuration;
                    _canvasWidth = CalculateWidth();
                    _marker.IsEnabled = true;

                    //Video gets played and paused so you can seek initially when the video gets loaded
                    _videoMedia.Play();
                    _videoMedia.Pause();

                    SetTimeline();

                    foreach (var desc in _projectManager.AllDescriptions)
                        DrawDescription(desc);

                    foreach (var space in _projectManager.Spaces)
                        SetSpaceLocation(space);
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

                    var xPosition = Mouse.GetPosition(_audioCanvas).X;
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
                double middleTime = (_videoDuration / _audioCanvas.Width) * middle;  // middle of the space in milliseconds
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


            //listens for when the audio stripping is complete then draws the timeline and the wave form
            //and sets the busy stripping audio to false so that the loading screen goes away
            _projectManager.SpacesAudioAnalysisCompleted += (sender, e) => SetTimeline();
            #endregion
        }

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
                    double position = (_videoMedia.Position.TotalMilliseconds / _videoDuration) * (_audioCanvas.Width);
                    UpdateMarkerPosition(position - MarkerOffset);
                });
            }
            catch (System.Threading.Tasks.TaskCanceledException exception)
            {
                //do nothing this exception is thrown when the application is exited
                Log.Warn("Task Cancelled exception", exception);
            }
        }

        #region View Listeners

        /// <summary>
        /// Updates TimlineScrollViewer's childrens' size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeLineScrollViewer_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Log.Info("TimeLineScrollViewer Sized Changed");

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
            if (_audioCanvas.CurrentActionState == ItemCanvas.ActionState.None &&
                _descriptionCanvas.CurrentActionState == ItemCanvas.ActionState.None)
                _descriptionInfoTabViewModel.ClearSelection();
        }

        private void DescriptionCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            //if we aren't dragging a description or space, we want to unselect them out of the list
            if (_audioCanvas.CurrentActionState == ItemCanvas.ActionState.None &&
                _descriptionCanvas.CurrentActionState == ItemCanvas.ActionState.None)
                _descriptionInfoTabViewModel.ClearSelection();
        }

        private void AudioCanvas_RecordRightClickPosition(object sender, EventArgs e)
        {
            //record the position in which you right clicked on the canvas
            //this position is used to calculate where on the audio canvas to draw a space
            var e1 = (MouseEventArgs)e;
            _rightClickPointOnAudioCanvas = e1.GetPosition(_audioCanvas);
        }

        /// <summary>
        /// Gets executed when the area in the NumberTimeline canvas gets clicked It changes the
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
                var xPosition = e.GetPosition(NumberTimeline).X;
                var newValue = (xPosition / _canvasWidth) * _videoDuration;

                UpdateMarkerPosition(xPosition - MarkerOffset);
                UpdateVideoPosition((int)newValue);
            }
        }

        #endregion

        #region Helper Functions

        private void AddDescriptionEventHandlers(Description description)
        {
            /* Draw the description only if the video is loaded, because there is currently
             * an issue with the video loading after the descriptions are added from an
             * opened project.
             */
            if (_videoMedia.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                DrawDescription(description);

            description.DescriptionMouseDownEvent += (sender1, e1) =>
            {
                //Add mouse down event on every description here
                var e2 = (MouseEventArgs)e1;
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

            description.GoToThisDescriptionEvent += (sender1, e1) =>
            {
                UpdateMarkerPosition((description.StartInVideo / _videoDuration) * (_audioCanvas.Width) - MarkerOffset);
                UpdateVideoPosition((int)description.StartInVideo);
                //Scroll 1 second before the start in video of the space
                TimeLineScrollViewer.ScrollToHorizontalOffset((_audioCanvas.Width / _videoDuration) *
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
        }

        private void AddSpaceEventHandlers(Space space)
        {
            //Adding a space depends on where you right clicked so we create and add it in the view
            //Set space only if the video is loaded/playing/recording/etc
            if (_videoMedia.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                SetSpaceLocation(space);

            space.SpaceMouseDownEvent += (sender1, e1) =>
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    _descriptionInfoTabViewModel.SelectedSpace = space;
                    SpaceAndDescriptionsTabControl.SpacesListView.ScrollToCenterOfView(space);
                }
            };

            space.PropertyChanged += (o, args) =>
            {
                if (args.PropertyName.Equals("StartInVideo") || args.PropertyName.Equals("EndInVideo"))
                    SetSpaceLocation(space);
            };

            space.GoToThisSpaceEvent += (o, args) =>
            {
                UpdateMarkerPosition((space.StartInVideo / _videoDuration) * (_audioCanvas.Width) - MarkerOffset);
                UpdateVideoPosition((int)space.StartInVideo);
                //Scroll 1 second before the start in video of the space
                TimeLineScrollViewer.ScrollToHorizontalOffset((_audioCanvas.Width / _videoDuration) *
                                                              (space.StartInVideo - 1000));

                _descriptionInfoTabViewModel.SelectedSpace = space;
                SpaceAndDescriptionsTabControl.SpacesListView.ScrollToCenterOfView(space);
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
            _audioCanvas.MaxWidth = staticCanvasWidth;
            return staticCanvasWidth;
        }

        #endregion

        #region graphics Functions
        /// <summary>
        /// Draws the wavform of the video audio on the canvas
        /// </summary>
        private void DrawWaveForm()
        {
            double width = TimeLineScrollViewer.ActualWidth;

            if (_mediaControlViewModel.Waveform == null || _canvasWidth == 0 || width == 0)
                return;

            List<short> data = _mediaControlViewModel.Waveform.Data;
            double samplesPerPixel = Math.Max(data.Count / _canvasWidth, 1);
            double middle = _audioCanvas.ActualHeight / 2;
            double yscale = middle;

            _audioCanvas.Children.Clear();

            int begin = (int)TimeLineScrollViewer.HorizontalOffset;
            int ratio = _mediaControlViewModel.Waveform.Header.NumChannels == 2 ? 40 : 80;
            double samplesPerSecond =
                (_mediaControlViewModel.Waveform.Header.SampleRate * (_mediaControlViewModel.Waveform.Header.BlockAlign / (double)ratio));

            double pixel = begin;

            while (pixel <= begin + width)
            {
                double offsetTime = (_videoDuration / (_canvasWidth * 1000)) * pixel;
                double sampleStart = samplesPerSecond * offsetTime;

                if (sampleStart + samplesPerPixel < data.Count)
                {
                    double max = (double)data.GetRange((int)sampleStart, (int)samplesPerPixel).Max() / short.MaxValue;
                    double min = (double)data.GetRange((int)sampleStart, (int)samplesPerPixel).Min() / short.MaxValue;
                    _audioCanvas.Children.Add(new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Black,
                        SnapsToDevicePixels = true, //Turn off anti-aliasing effect
                        Y1 = middle + max * yscale,
                        Y2 = middle + min * yscale,
                        X1 = pixel,
                        X2 = pixel,
                    });
                }
                pixel++;
            }

            //re-add children of AudioCanvas
            _audioCanvas.Children.Add(AudioCanvasControl.SpacesItemControl);
        }

        private void DrawDescription(Description description)
        {
            //set the Description values that are bound to the graphics in MainWindow.xaml
            description.X = (description.StartInVideo / _videoDuration) * _audioCanvas.Width;
            description.Y = 0;
            description.Width = (_audioCanvas.Width / _videoDuration) * (description.EndWaveFileTime -
                description.StartWaveFileTime);
            description.Height = _descriptionCanvas.ActualHeight;
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
        /// Resizes all the Spaces to fit the AudioCanvas and not overlap the NumberTimeline
        /// </summary>
        private void ResizeSpaces()
        {
            foreach (var space in _projectManager.Spaces)
                space.Height = _audioCanvas.ActualHeight;
        }

        private void AddLinesToNumberTimeLine()
        {
            if (_videoMedia.CurrentState == LiveDescribeVideoStates.VideoNotLoaded || _canvasWidth == 0)
                return;

            //Number of lines in the amount of time that the video plays for
            int numlines = (int)(_videoDuration / (LineTime * 1000));
            int beginLine = (int)((numlines / _canvasWidth) * TimeLineScrollViewer.HorizontalOffset);
            int endLine = beginLine + (int)((numlines / _canvasWidth) * TimeLineScrollViewer.ActualWidth) + 1;
            //Clear the canvas because we don't want the remaining lines due to importing a new video
            //or resizing the window
            NumberTimeline.Children.Clear();

            for (int i = beginLine; i <= endLine; ++i)
            {
                if (i % LongLineTime == 0)
                {
                    NumberTimeline.Children.Add(new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Blue,
                        StrokeThickness = 1.5,
                        Y1 = 0,
                        Y2 = NumberTimeline.ActualHeight / 1.2,
                        X1 = _canvasWidth / numlines * i,
                        X2 = _canvasWidth / numlines * i,
                    });

                    var timestamp = new TextBlock
                    {
                        Text = (string)_millisecondsTimeConverter.Convert((i * LineTime) * 1000, typeof(int), null,
                            CultureInfo.CurrentCulture)
                    };
                    Canvas.SetLeft(timestamp, ((_canvasWidth / numlines * i) - 24));
                    NumberTimeline.Children.Add(timestamp);
                }
                else
                {
                    NumberTimeline.Children.Add(new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Black,
                        Y1 = 0,
                        Y2 = NumberTimeline.ActualHeight / 2,
                        X1 = _canvasWidth / numlines * i,
                        X2 = _canvasWidth / numlines * i
                    });
                }
            }
        }

        /// <summary>
        /// Update's the instance variables that keep track of the timeline height and width, and
        /// calculates the size of the timeline if the width of the audio canvas is greater then the
        /// timeline width it automatically overflows and scrolls due to the scrollview then update
        /// the width of the marker to match the audio canvas
        /// </summary>
        private void SetTimeline()
        {
            Log.Info("Setting timeline");
            NumberTimeline.Width = _canvasWidth;
            _audioCanvas.Width = _canvasWidth;
            _descriptionCanvas.Width = _canvasWidth;

            DrawWaveForm();
            AddLinesToNumberTimeLine();
            ResizeDescriptions();
            ResizeSpaces();
        }

        /// <summary>
        /// Sets the location of the given space on the canvas.
        /// </summary>
        /// <param name="space"></param>
        private void SetSpaceLocation(Space space)
        {
            space.X = (_audioCanvas.Width / _videoDuration) * space.StartInVideo;
            space.Y = 0;
            space.Height = _audioCanvas.ActualHeight;
            space.Width = (_audioCanvas.Width / _videoDuration) * (space.EndInVideo - space.StartInVideo);
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
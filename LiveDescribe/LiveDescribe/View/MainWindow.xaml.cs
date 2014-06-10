﻿using LiveDescribe.Controls;
using LiveDescribe.Model;
using LiveDescribe.View_Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private enum SpacesActionState { None, Dragging, ResizingEndOfSpace, ResizingBeginningOfSpace };

        private enum DescriptionsActionState
        {
            None,
            Dragging
        };

        #region Constants
        private const double DefaultSpaceLengthInMilliSeconds = 3000;
        private const double MarkerOffset = 10.0;
        /// <summary>
        /// when the marker hits 95% of the page it scrolls
        /// </summary>
        private const double PageScrollPercent = 0.95;
        /// <summary>30 seconds page time before audiocanvas & descriptioncanvas scroll</summary>
        private const double PageTimeBeforeCanvasScrolls = 30;
        private const double LineTime = 1; //each line in the NumberTimeline appears every 1 second
        private const int LongLineTime = 5; // every 5 LineTimes, you get a Longer Line
        // resizing the space only happens at ResizeSpaceOffset amount of pixels away from the
        // beginning and ending of a space
        private const int ResizeSpaceOffset = 10;
        #endregion

        #region Instance Variables

        private Description _descriptionBeingModified;
        private Space _spaceBeingModified;

        /// <summary>
        /// The width of the entire canvas.
        /// </summary>
        private double _canvasWidth = 0;
        private double _videoDuration = -1;
        private readonly MediaControlViewModel _mediaControlViewModel;
        private readonly SpacesViewModel _spacesViewModel;
        /// <summary>
        /// used to format a timespan object which in this case in the videoMedia.Position
        /// </summary>
        private readonly DescriptionViewModel _descriptionViewModel;
        private readonly DescriptionInfoTabViewModel _descriptionInfoTabViewModel;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private double _originalPositionForDraggingDescription = -1;
        private double _originalPositionForDraggingSpace = -1;
        private Point _rightClickPointOnAudioCanvas;
        private SpacesActionState _spacesActionState = SpacesActionState.None;
        private DescriptionsActionState _descriptionActionState = DescriptionsActionState.None;
        private readonly Cursor _grabCursor;
        private readonly Cursor _grabbingCursor;
        private readonly LiveDescribeMediaPlayer _videoMedia;
        #endregion

        public MainWindow()
        {

            var splashScreen = new SplashScreen("../Images/LiveDescribe-Splashscreen.png");
            splashScreen.Show(true);
            Thread.Sleep(2000);
            InitializeComponent();

            _videoMedia = MediaControl.VideoMedia;

            var mainWindowViewModel = new MainWindowViewModel(_videoMedia);

            DataContext = mainWindowViewModel;
            _mainWindowViewModel = mainWindowViewModel;

            _mediaControlViewModel = mainWindowViewModel.MediaControlViewModel;
            _descriptionViewModel = mainWindowViewModel.DescriptionViewModel;
            _spacesViewModel = mainWindowViewModel.SpacesViewModel;
            _descriptionInfoTabViewModel = mainWindowViewModel.DescriptionInfoTabViewModel;

            var cursfile = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Cursors/grab.cur"));
            _grabCursor = new Cursor(cursfile.Stream);

            cursfile = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Cursors/grabbing.cur"));
            _grabbingCursor = new Cursor(cursfile.Stream);

            #region TimeLineScrollViewer Event Listeners
            TimeLineScrollViewer.ScrollChanged += (sender, e) =>
            {
                DrawWaveForm();
                AddLinesToNumberTimeLine();
            };
            #endregion

            #region Event Listeners for VideoMedia
            //if the videomedia's path changes (a video is added)
            //then play and stop the video to load the video
            //this is done because the MediaElement does not load the video until it is played
            //therefore you can't know the duration of the video or if it hasn't been loaded properly unless it fails
            //I know this is a little hackish, but it's a consequence of how the MediaElement works
            _videoMedia.PathChangedEvent += (sender, e) =>
                {
                    _videoMedia.Play();
                    _videoMedia.Pause();
                };
            #endregion

            #region Event Listeners For Main Control (Pause, Play, Mute)
            //These events are put inside the main control because they will also effect the list
            //of audio descriptions an instance of DescriptionViewModel is inside the main control
            //and the main control will take care of synchronizing the video, and the descriptions

            //listens for PlayRequested Event
            mainWindowViewModel.PlayRequested += (sender, e) =>
                {
                    //this is to recheck all the graphics states
                    CommandManager.InvalidateRequerySuggested();

                    double position = (_videoMedia.Position.TotalMilliseconds / _videoDuration) * (AudioCanvas.Width);
                    UpdateMarkerPosition(position - MarkerOffset);
                };

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
                AudioCanvas.Children.Clear();
                NumberTimeline.Children.Clear();

                UpdateMarkerPosition(-MarkerOffset);
                Marker.IsEnabled = false;
            };

            mainWindowViewModel.GraphicsTick += Play_Tick;
            #endregion

            #region Event Listeners For MediaControlViewModel

            //listens for VideoOpenedRequested event
            //this event only gets thrown when if the MediaFailed event doesn't occur
            //and as soon as the video is loaded when play is pressed
            mainWindowViewModel.MediaControlViewModel.VideoOpenedRequested += (sender, e) =>
                {
                    _videoDuration = _videoMedia.NaturalDuration.TimeSpan.TotalMilliseconds;
                    _canvasWidth = CalculateWidth();
                    Marker.IsEnabled = true;
                    SetTimeline();

                    foreach (var desc in _descriptionViewModel.AllDescriptions)
                    {
                        DrawDescription(desc);
                    }

                    foreach (var space in _spacesViewModel.Spaces)
                    {
                        SetSpaceLocation(space);
                    }
                };

            //listens for when the audio stripping is complete then draws the timeline and the wave form
            //and sets the busy stripping audio to false so that the loading screen goes away
            mainWindowViewModel.MediaControlViewModel.OnStrippingAudioCompleted += (sender, e) =>
                {
                    SetTimeline();

                    //make this false so that the loading screen goes away after the timeline and the wave form are drawn
                    mainWindowViewModel.LoadingViewModel.Visible = false;
                };

            //captures the mouse when a mousedown request is sent to the Marker
            mainWindowViewModel.MediaControlViewModel.OnMarkerMouseDownRequested += (sender, e) => Marker.CaptureMouse();

            //updates the video position when the mouse is released on the Marker
            mainWindowViewModel.MediaControlViewModel.OnMarkerMouseUpRequested += (sender, e) => Marker.ReleaseMouseCapture();

            //updates the canvas and video position when the Marker is moved
            mainWindowViewModel.MediaControlViewModel.OnMarkerMouseMoveRequested += (sender, e) =>
                {
                    if (!Marker.IsMouseCaptured) return;

                    if (ScrollRightIfCan(Canvas.GetLeft(Marker)))
                    {
                        Marker.ReleaseMouseCapture();
                        return;
                    }


                    var xPosition = Mouse.GetPosition(AudioCanvas).X;
                    var middleOfMarker = xPosition - MarkerOffset;

                    //make sure the middle of the marker doesn't go below the beginning of the canvas
                    if (xPosition < -MarkerOffset)
                    {
                        Canvas.SetLeft(Marker, -MarkerOffset);
                        UpdateVideoPosition(0);
                        return;
                    }

                    var newPositionInVideo = (xPosition / _canvasWidth) * _videoDuration;
                    if (newPositionInVideo >= _videoDuration)
                    {
                        var newPositionOfMarker = (_canvasWidth / _videoDuration) * (_videoDuration);
                        Canvas.SetLeft(Marker, newPositionOfMarker - MarkerOffset);
                        UpdateVideoPosition((int)(_videoDuration));
                        return;
                    }
                    Canvas.SetLeft(Marker, middleOfMarker);
                    UpdateVideoPosition((int)newPositionInVideo);
                };

            mainWindowViewModel.MediaControlViewModel.FastForwardEvent += (sender, e) =>
                    UpdateMarkerPosition(((_canvasWidth / _videoDuration) * _videoMedia.Position.TotalMilliseconds) - MarkerOffset);

            mainWindowViewModel.MediaControlViewModel.RewindEvent += (sender, e) =>
                    UpdateMarkerPosition(((_canvasWidth / _videoDuration) * _videoMedia.Position.TotalMilliseconds) - MarkerOffset);

            #endregion

            #region Event Listeners for DescriptionViewModel

            _descriptionViewModel.RecordRequestedMicrophoneNotPluggedIn += (sender, e) =>
                {
                    //perhaps show a popup when the Record button is pressed and there is no microphone plugged in
                    MessageBox.Show("No Microphone Connected");
                    Log.Warn("No microphone connected");
                };

            //When a description is added, attach an event to the StartInVideo and EndInVideo properties
            //so when those properties change it redraws them
            _descriptionViewModel.AddDescriptionEvent += (sender, e) =>
                {
                    /* Draw the description only if the video is loaded, because there is currently
                     * an issue with the video loading after the descriptions are added from an
                     * opened project.
                     */
                    if (_videoMedia.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                        DrawDescription(e.Description);

                    e.Description.DescriptionMouseDownEvent += (sender1, e1) =>
                    {
                        //Add mouse down event on every description here
                        var e2 = (MouseEventArgs)e1;
                        if (Mouse.LeftButton == MouseButtonState.Pressed)
                        {
                            if (e.Description.IsExtendedDescription)
                                _descriptionInfoTabViewModel.ExtendedDescriptionSelectedInList = e.Description;
                            else
                                _descriptionInfoTabViewModel.RegularDescriptionSelectedInList = e.Description;

                            _originalPositionForDraggingDescription = e2.GetPosition(DescriptionCanvas).X;
                            _descriptionBeingModified = e.Description;
                            DescriptionCanvas.CaptureMouse();
                            Mouse.SetCursor(_grabbingCursor);
                            _descriptionActionState = DescriptionsActionState.Dragging;
                        }
                    };

                    e.Description.DescriptionMouseMoveEvent += (sender1, e1) => Mouse.SetCursor(_grabCursor);

                    e.Description.PropertyChanged += (sender1, e1) =>
                    {
                        if (e1.PropertyName.Equals("StartWaveFileTime"))
                        {
                            //change start in the wave file for resizing the start time
                        }
                        else if (e1.PropertyName.Equals("EndWaveFileTime"))
                        {
                            //change end in the wave file for resizing the end time
                        }
                    };
                };
            #endregion

            #region Event Listeners for SpacesViewModel

            _spacesViewModel.SpaceAddedEvent += (sender, e) =>
                {
                    //Adding a space depends on where you right clicked so we create and add it in the view
                    Space space = e.Space;

                    //Set space only if the video is loaded/playing/recording/etc
                    if (_videoMedia.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                        SetSpaceLocation(space);

                    space.SpaceMouseDownEvent += (sender1, e1) =>
                        {
                            var args = (MouseEventArgs)e1;

                            if (Mouse.LeftButton == MouseButtonState.Pressed)
                            {
                                _descriptionInfoTabViewModel.SpaceSelectedInList = space;

                                double xPos = args.GetPosition(AudioCanvas).X;

                                //prepare space for dragging
                                _originalPositionForDraggingSpace = xPos;
                                _spaceBeingModified = space;
                                AudioCanvas.CaptureMouse();

                                if (xPos > (space.X + space.Width - ResizeSpaceOffset))
                                {
                                    Mouse.SetCursor(Cursors.SizeWE);
                                    _spacesActionState = SpacesActionState.ResizingEndOfSpace;
                                }
                                else if (xPos < (space.X + ResizeSpaceOffset))
                                {
                                    Mouse.SetCursor(Cursors.SizeWE);
                                    _spacesActionState = SpacesActionState.ResizingBeginningOfSpace;
                                }
                                else
                                {
                                    Mouse.SetCursor(_grabbingCursor);
                                    _spacesActionState = SpacesActionState.Dragging;
                                }
                            }
                        };

                    space.SpaceMouseMoveEvent += (sender1, e1) =>
                        {
                            var args = (MouseEventArgs)e1;
                            double xPos = args.GetPosition(AudioCanvas).X;

                            //Changes cursor if the mouse hovers over the end or the beginning of the space
                            if (xPos > (space.X + space.Width - ResizeSpaceOffset))
                            {
                                //resizing right side of the space
                                Mouse.SetCursor(Cursors.SizeWE);
                            }
                            else if (xPos < (space.X + ResizeSpaceOffset))
                            {
                                Mouse.SetCursor(Cursors.SizeWE);
                            }
                            else
                            {
                                Mouse.SetCursor(_grabCursor);
                            }
                        };

                    space.PropertyChanged += (o, args) =>
                    {
                        if (args.PropertyName.Equals("StartInVideo") || args.PropertyName.Equals("EndInVideo"))
                            SetSpaceLocation(space);
                    };
                };

            _spacesViewModel.RequestSpaceTime += (sender, args) =>
            {
                var space = args.Space;

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

            #region Event Listeners for LoadingViewModel
            mainWindowViewModel.LoadingViewModel.PropertyChanged += (sender, e) =>
            {
                /* Set LoadingBorder to appear in front of everything when visible, otherwise put
                 * it behind everything. This allows it to sit behind in the XAML viewer.
                 */
                if (e.PropertyName.Equals("Visible"))
                {
                    if (mainWindowViewModel.LoadingViewModel.Visible)
                        Panel.SetZIndex(LoadingControl, 2);
                    else
                        Panel.SetZIndex(LoadingControl, -1);
                }
            };
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
                Dispatcher.Invoke(delegate { canvasLeft = Canvas.GetLeft(Marker); });
                ScrollRightIfCan(canvasLeft);
                Dispatcher.Invoke(delegate
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
            {
                SetTimeline();
            }
            //update marker to fit the entire AudioCanvas even when there's no video loaded
            Marker.Points[4] = new Point(Marker.Points[4].X, TimeLineScrollViewer.ActualHeight);
        }

        /// <summary>
        /// Called when mouse is up on the audio canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                AudioCanvas.ReleaseMouseCapture();
                _spacesActionState = SpacesActionState.None;
            }
        }

        /// <summary>
        /// Caled when the mouse is being dragged on the audio canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            double xPos = e.GetPosition(AudioCanvas).X;
            if (AudioCanvas.IsMouseCaptured)
            {
                if (_spacesActionState == SpacesActionState.ResizingEndOfSpace)
                {
                    ResizeEndOfSpaceBeingModified(xPos);
                }
                else if (_spacesActionState == SpacesActionState.ResizingBeginningOfSpace)
                {
                    ResizeBeginningOfSpaceBeingModified(xPos);
                }
                else if (_spacesActionState == SpacesActionState.Dragging)
                {
                    DragSpaceBeingModified(xPos);
                }
            }
        }

        private void ResizeEndOfSpaceBeingModified(double mouseXPosition)
        {
            double newWidth = _spaceBeingModified.Width + (mouseXPosition - _originalPositionForDraggingSpace);
            double lengthInMillisecondsNewWidth = (_videoDuration / AudioCanvas.Width) * newWidth;

            //bounds checking
            if (lengthInMillisecondsNewWidth < SpacesViewModel.MinSpaceLengthInMSecs)
            {
                newWidth = (AudioCanvas.Width / _videoDuration) * SpacesViewModel.MinSpaceLengthInMSecs;
                //temporary fix, have to make the cursor attached to the end of the space somehow
                AudioCanvas.ReleaseMouseCapture();
            }
            else if ((_spaceBeingModified.StartInVideo + lengthInMillisecondsNewWidth) > _videoDuration)
            {
                newWidth = (AudioCanvas.Width / _videoDuration) * (_videoDuration - _spaceBeingModified.StartInVideo);
                //temporary fix, have to make the cursor attached to the end of the space somehow
                AudioCanvas.ReleaseMouseCapture();
            }

            _spaceBeingModified.Width = newWidth;
            _originalPositionForDraggingSpace = mouseXPosition;
            _spaceBeingModified.EndInVideo = _spaceBeingModified.StartInVideo + (_videoDuration / AudioCanvas.Width) * _spaceBeingModified.Width;

            Mouse.SetCursor(Cursors.SizeWE);
        }

        private void ResizeBeginningOfSpaceBeingModified(double mouseXPosition)
        {
            //left side of space
            double newPosition = _spaceBeingModified.X + (mouseXPosition - _originalPositionForDraggingSpace);
            double newPositionMilliseconds = (_videoDuration / AudioCanvas.Width) * newPosition;

            //bounds checking
            if (newPositionMilliseconds < 0)
            {
                newPosition = 0;
                //temporary fix, have to make the cursor attached to the end of the space somehow
                AudioCanvas.ReleaseMouseCapture();
            }
            else if ((_spaceBeingModified.EndInVideo - newPositionMilliseconds) < SpacesViewModel.MinSpaceLengthInMSecs)
            {
                newPosition = (AudioCanvas.Width / _videoDuration) * (_spaceBeingModified.EndInVideo - SpacesViewModel.MinSpaceLengthInMSecs);
                //temporary fix, have to make the cursor attached to the end of the space somehow
                AudioCanvas.ReleaseMouseCapture();
            }

            _spaceBeingModified.X = newPosition;
            _spaceBeingModified.StartInVideo = (_videoDuration / AudioCanvas.Width) * newPosition;
            _spaceBeingModified.Width = (AudioCanvas.Width / _videoDuration) * (_spaceBeingModified.EndInVideo - _spaceBeingModified.StartInVideo);

            _originalPositionForDraggingSpace = mouseXPosition;
            Mouse.SetCursor(Cursors.SizeWE);
        }

        private void DragSpaceBeingModified(double mouseXPosition)
        {
            double newPosition = _spaceBeingModified.X + (mouseXPosition - _originalPositionForDraggingSpace);
            double newPositionMilliseconds = (_videoDuration / AudioCanvas.Width) * newPosition;
            double lengthOfSpaceMilliseconds = _spaceBeingModified.EndInVideo - _spaceBeingModified.StartInVideo;
            //size in pixels of the space
            double size = (AudioCanvas.Width / _videoDuration) * lengthOfSpaceMilliseconds;

            if (newPositionMilliseconds < 0)
                newPosition = 0;
            else if ((newPositionMilliseconds + lengthOfSpaceMilliseconds) > _videoDuration)
                newPosition = (AudioCanvas.Width / _videoDuration) * (_videoDuration - lengthOfSpaceMilliseconds);

            _spaceBeingModified.X = newPosition;
            _originalPositionForDraggingSpace = mouseXPosition;
            _spaceBeingModified.StartInVideo = (_videoDuration / AudioCanvas.Width) * (_spaceBeingModified.X);
            _spaceBeingModified.EndInVideo = _spaceBeingModified.StartInVideo + (_videoDuration / AudioCanvas.Width) * size;
            Mouse.SetCursor(_grabbingCursor);
        }

        /// <summary>
        /// Gets called when the mouse is down on the audio canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //if we aren't dragging a description or space, we want to unselect them out of the list
            if (_spacesActionState == SpacesActionState.None && _descriptionActionState == DescriptionsActionState.None)
            {
                _descriptionInfoTabViewModel.SpaceSelectedInList = null;
                _descriptionInfoTabViewModel.ExtendedDescriptionSelectedInList = null;
                _descriptionInfoTabViewModel.RegularDescriptionSelectedInList = null;
            }
        }

        /// <summary>
        /// Gets called on the mouse up event of the description canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DescriptionCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                //used for drag and drop, when the left click is released we want to release the mouse capture over  the description
                //and stop dragging the description
                //the mouse gets captured when a description is left clicked
                DescriptionCanvas.ReleaseMouseCapture();
                _descriptionActionState = DescriptionsActionState.None;
            }
        }

        private void DescriptionCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            //while the mouse is moving over the description canvas and if a description is clicked (DescriptionCanvas.IsMouseCaptured)
            //update the position of the description and the start and end times in the video
            if (DescriptionCanvas.IsMouseCaptured)
            {
                if (_descriptionActionState == DescriptionsActionState.Dragging)
                    DragDescriptionBeingModified(e.GetPosition(DescriptionCanvas).X);
            }
        }

        private void DragDescriptionBeingModified(double mouseXPos)
        {
            double newPosition = _descriptionBeingModified.X + (mouseXPos - _originalPositionForDraggingDescription);
            double newPositionMilliseconds = (_videoDuration / AudioCanvas.Width) * newPosition;
            double lengthOfDescriptionMilliseconds = _descriptionBeingModified.EndInVideo - _descriptionBeingModified.StartInVideo;

            //bounds checking when dragging the description
            if (newPositionMilliseconds < 0)
                newPosition = 0;
            else if ((newPositionMilliseconds + lengthOfDescriptionMilliseconds) > _videoDuration)
                newPosition = (AudioCanvas.Width / _videoDuration) * (_videoDuration - lengthOfDescriptionMilliseconds);

            _descriptionBeingModified.X = newPosition;
            _originalPositionForDraggingDescription = mouseXPos;
            _descriptionBeingModified.StartInVideo = (_videoDuration / AudioCanvas.Width) * (_descriptionBeingModified.X);
            _descriptionBeingModified.EndInVideo = _descriptionBeingModified.StartInVideo + (_descriptionBeingModified.EndWaveFileTime - _descriptionBeingModified.StartWaveFileTime);
            Mouse.SetCursor(_grabbingCursor);
        }

        private void AudioCanvas_RecordRightClickPosition(object sender, MouseButtonEventArgs e)
        {
            //record the position in which you right clicked on the canvas
            //this position is used to calculate where on the audio canvas to draw a space
            _rightClickPointOnAudioCanvas = e.GetPosition(AudioCanvas);
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
                _mediaControlViewModel.PauseCommand.Execute(this);
                var xPosition = e.GetPosition(NumberTimeline).X;
                var newValue = (xPosition / _canvasWidth) * _videoDuration;

                UpdateMarkerPosition(xPosition - MarkerOffset);
                UpdateVideoPosition((int)newValue);
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Updates the Marker Position in the timeline and sets the corresponding time in the timelabel
        /// </summary>
        /// <param name="xPos">the x position in which the marker is supposed to move</param>
        private void UpdateMarkerPosition(double xPos)
        {
            Canvas.SetLeft(Marker, xPos);
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

            Dispatcher.Invoke(delegate
            {
                singlePageWidth = TimeLineScrollViewer.ActualWidth;
                scrolledAmount = TimeLineScrollViewer.HorizontalOffset;
            });
            double scrollOffsetRight = PageScrollPercent * singlePageWidth;
            if (!(xPos - scrolledAmount >= (scrollOffsetRight))) return false;

            Dispatcher.Invoke(() => TimeLineScrollViewer.ScrollToHorizontalOffset(scrollOffsetRight + scrolledAmount));
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

            Log.Info("Drawing wave form");

            List<short> data = _mediaControlViewModel.Waveform.Data;
            double samplesPerPixel = Math.Max(data.Count / _canvasWidth, 1);
            double middle = AudioCanvas.ActualHeight / 2;
            double yscale = middle;

            AudioCanvas.Children.Clear();

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
                    AudioCanvas.Children.Add(new Line
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
            AudioCanvas.Children.Add(SpacesItemControl);

        }

        private void DrawDescription(Description description)
        {
            //set the Description values that are bound to the graphics in MainWindow.xaml
            description.X = (description.StartInVideo / _videoDuration) * AudioCanvas.Width;
            description.Y = 0;
            description.Width = (AudioCanvas.Width / _videoDuration) * (description.EndWaveFileTime -
                description.StartWaveFileTime);
            description.Height = DescriptionCanvas.ActualHeight;
        }

        /// <summary>
        /// Resizes all the descriptions height to fit the description canvas
        /// </summary>
        private void ResizeDescriptions()
        {
            foreach (Description description in _descriptionViewModel.AllDescriptions)
                description.Height = DescriptionCanvas.ActualHeight;
        }

        /// <summary>
        /// Resizes all the Spaces to fit the AudioCanvas and not overlap the NumberTimeline
        /// </summary>
        private void ResizeSpaces()
        {
            foreach (Space space in _spacesViewModel.Spaces)
                space.Height = AudioCanvas.ActualHeight;
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
            AudioCanvas.Width = _canvasWidth;
            DescriptionCanvas.Width = _canvasWidth;

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
            space.X = (AudioCanvas.Width / _videoDuration) * space.StartInVideo;
            space.Y = 0;
            space.Height = AudioCanvas.ActualHeight;
            space.Width = (AudioCanvas.Width / _videoDuration) * (space.EndInVideo - space.StartInVideo);
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
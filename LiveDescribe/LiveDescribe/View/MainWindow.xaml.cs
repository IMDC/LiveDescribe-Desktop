using System.ComponentModel;
using LiveDescribe.Converters;
using LiveDescribe.View_Model;
using LiveDescribe.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using LiveDescribe.Controls;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        private enum SpacesActionState { None, Dragging, ResizingEndOfSpace, ResizingBeginningOfSpace };

        #region Constants
        private const double DefaultSpaceLengthInMilliSeconds = 3000;
        private const double MarkerOffset = 10.0;
        /// <summary>when the marker hits 95% of the page it scrolls</summary>
        private const double PageScrollPercent = 0.95;
        /// <summary>30 seconds page time before audiocanvas  & descriptioncanvas scroll</summary>
        private const double PageTimeBeforeCanvasScrolls = 30;
        private const double LineTime = 1; //each line in the NumberTimeline appears every 1 second
        private const int LongLineTime = 5; // every 5 LineTimes, you get a Longer Line
        // resizing the space only happens at ResizeSpaceOffset amount of pixels 
        // away from the beginning and ending of a space
        private const int ResizeSpaceOffset = 10;
        #endregion

        #region Instance Variables
        
        private Description _descriptionBeingDragged;
        private Space _spaceBeingDraggedOrResized;

        /// <summary>The width of the entire canvas.</summary>
        private double _canvasWidth = 0;
        private double _videoDuration = -1;
        private readonly VideoControl _videoControl;
        private readonly SpacesViewModel _spacesViewModel;
        private readonly PreferencesViewModel _preferences;
        /// <summary>used to format a timespan object which in this case in the videoMedia.Position</summary>
        private readonly DescriptionViewModel _descriptionViewModel;
        private readonly TimeConverterFormatter _formatter;
        private readonly DescriptionInfoTabViewModel _descriptionInfoTabViewModel;
        private readonly MainControl _mainControl;
        private double _originalPositionForDraggingDescription = -1;
        private double _originalPositionForDraggingSpace = -1;
        private Point RightClickPointOnAudioCanvas;
        private SpacesActionState _spacesActionState = SpacesActionState.None;
        private Cursor _grabCursor;
        private Cursor _grabbingCursor;
        private LiveDescribeMediaPlayer VideoMedia;
        #endregion

        public MainWindow()
        {

            var splashScreen = new SplashScreen("../Images/LiveDescribe-Splashscreen.png");
            splashScreen.Show(true);
            Thread.Sleep(2000);
            InitializeComponent();

            VideoMedia = MediaControl.VideoMedia;
            
            var maincontrol = new MainControl(VideoMedia);

            DataContext = maincontrol;
            _mainControl = maincontrol;

            _videoControl = maincontrol.VideoControl;
            _preferences = maincontrol.PreferencesViewModel;
            _descriptionViewModel = maincontrol.DescriptionViewModel;
            _spacesViewModel = maincontrol.SpacesViewModel;
            _descriptionInfoTabViewModel = maincontrol.DescriptionInfoTabViewModel;

            _formatter = new TimeConverterFormatter();

            var cursfile = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Cursors/grab.cur"));
            _grabCursor = new Cursor(cursfile.Stream);

            cursfile = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Cursors/grabbing.cur"));
            _grabbingCursor = new Cursor(cursfile.Stream);

            #region TimeLine Event Listeners
            TimeLine.ScrollChanged += (sender, e) => { DrawWaveForm(); };
            #endregion

            #region Event Listeners for VideoMedia
            //if the videomedia's path changes (a video is added)
            //then play and stop the video to load the video
            //this is done because the MediaElement does not load the video until it is played
            //therefore you can't know the duration of the video or if it hasn't been loaded properly unless it fails
            //I know this is a little hackish, but it's a consequence of how the MediaElement works
            VideoMedia.PathChangedEvent += (sender, e) =>
                {
                    VideoMedia.Play();
                    VideoMedia.Pause();
                };
            #endregion

            #region Event Listeners For Main Control (Pause, Play, Mute)
            //These events are put inside the main control because they will also effect the list
            //of audio descriptions an instance of DescriptionViewModel is inside the main control
            //and the main control will take care of synchronizing the video, and the descriptions

            //listens for PlayRequested Event
            maincontrol.PlayRequested += (sender, e) =>
                {
                    //this is to recheck all the graphics states
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();

                    double position = (VideoMedia.Position.TotalMilliseconds / _videoDuration) * (AudioCanvas.Width);
                    UpdateMarkerPosition(position - MarkerOffset);
                };

            //listens for PauseRequested Event
            maincontrol.PauseRequested += (sender, e) =>
                {
                    //this is to recheck all the graphics states
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                };

            //listens for when the media has gone all the way to the end
            maincontrol.MediaEnded += (sender, e) =>
                {
                    UpdateMarkerPosition(-MarkerOffset);
                    //this is to recheck all the graphics states
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                };

            maincontrol.ProjectClosed += (sender, e) =>
            {
                AudioCanvas.Children.Clear();
                AudioCanvas.Background = null;
                NumberTimeline.Children.Clear();
                AudioCanvas.Children.Add(NumberTimelineBorder);
                AudioCanvas.Children.Add(Marker);

                UpdateMarkerPosition(-MarkerOffset);
                Marker.IsEnabled = false;
            };

            maincontrol.GraphicsTick += Play_Tick;
            #endregion

            #region Event Listeners For VideoControl

            //listens for VideoOpenedRequested event
            //this event only gets thrown when if the MediaFailed event doesn't occur
            //and as soon as the video is loaded when play is pressed
            maincontrol.VideoControl.VideoOpenedRequested += (sender, e) =>
                {
                    _videoDuration = VideoMedia.NaturalDuration.TimeSpan.TotalMilliseconds;
                    _canvasWidth = calculateWidth();
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
            maincontrol.VideoControl.OnStrippingAudioCompleted += (sender, e) =>
                {
                   SetTimeline();

                    //make this false so that the loading screen goes away after the timeline and the wave form are drawn
                    maincontrol.LoadingViewModel.Visible = false;
                };

            //captures the mouse when a mousedown request is sent to the Marker
            maincontrol.VideoControl.OnMarkerMouseDownRequested += (sender, e) =>
            {
                Marker.CaptureMouse();
            };

            //updates the video position when the mouse is released on the Marker
            maincontrol.VideoControl.OnMarkerMouseUpRequested += (sender, e) =>
                {
                    var newValue = ((Canvas.GetLeft(Marker) + MarkerOffset) / AudioCanvas.Width) * _videoDuration;

                    UpdateVideoPosition((int)newValue);
                    Marker.ReleaseMouseCapture();
                };

            //updates the canvas and video position when the Marker is moved
            maincontrol.VideoControl.OnMarkerMouseMoveRequested += (sender, e) =>
                {
                    if (!Marker.IsMouseCaptured) return;

                    if (ScrollRightIfCan(Canvas.GetLeft(Marker)))
                    {
                        Marker.ReleaseMouseCapture();
                        return;
                    }

                    var xPosition = Mouse.GetPosition(AudioCanvasBorder).X;

                    //make sure the middle of the marker doesn't go below the beginning of the canvas
                    if (xPosition < -MarkerOffset)
                    {
                        Canvas.SetLeft(Marker, -MarkerOffset);
                        UpdateVideoPosition(0);
                        return;
                    }
                    //make sure the middle of the marker doesn't go above the end of the canvas
                    else if (xPosition > AudioCanvas.Width - 1)
                    {
                        Canvas.SetLeft(Marker, AudioCanvas.Width - 1);
                    }
                    else
                        Canvas.SetLeft(Marker, xPosition - MarkerOffset);

                    var newValue = (xPosition / AudioCanvas.Width) * _videoDuration;
                    UpdateVideoPosition((int)newValue);
                };

            maincontrol.VideoControl.FastForwardEvent += (sender, e) =>
                    UpdateMarkerPosition(((_canvasWidth /_videoDuration) * VideoMedia.Position.TotalMilliseconds) - MarkerOffset);

            maincontrol.VideoControl.RewindEvent += (sender, e) =>
                    UpdateMarkerPosition(((_canvasWidth / _videoDuration) * VideoMedia.Position.TotalMilliseconds) - MarkerOffset);

            #endregion

            #region Event Listeners for DescriptionViewModel

            _descriptionViewModel.RecordRequestedMicrophoneNotPluggedIn += (sender, e) =>
                {
                    //perhaps show a popup when the Record button is pressed and there is no microphone plugged in
                    log.Warn("No microphone connected");
                };

            //When a description is added, attach an event to the StartInVideo and EndInVideo properties
            //so when those properties change it redraws them
            _descriptionViewModel.AddDescriptionEvent += (sender, e) =>
                {
                    /* Draw the description only if the video is loaded, because there is currently
                     * an issue with the video loading after the descriptions are added from an
                     * opened project.
                     */
                    if (VideoMedia.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                        DrawDescription(e.Description);

                    e.Description.DescriptionMouseDownEvent += (sender1, e1) =>
                    {
                        //Add mouse down event on every description here
                        MouseEventArgs e2 = (MouseEventArgs)e1;
                        if (Mouse.LeftButton == MouseButtonState.Pressed)
                        {
                            //If the description is already selected, deselect it and set the description selected in the list
                            //in the tab control to null so it isn't selected in the list either
                            if (e.Description.IsSelected)
                            {
                                e.Description.IsSelected = false;
                                if (e.Description.IsExtendedDescription)
                                    _descriptionInfoTabViewModel.ExtendedDescriptionSelectedInList = null;
                                else if (!e.Description.IsExtendedDescription)
                                    _descriptionInfoTabViewModel.RegularDescriptionSelectedInList = null;
                            }
                            else
                            {

                                if (e.Description.IsExtendedDescription)
                                    _descriptionInfoTabViewModel.ExtendedDescriptionSelectedInList = e.Description;
                                else
                                    _descriptionInfoTabViewModel.RegularDescriptionSelectedInList = e.Description;
                            }

                            _originalPositionForDraggingDescription = e2.GetPosition(DescriptionCanvas).X;
                            _descriptionBeingDragged = e.Description;
                            DescriptionCanvas.CaptureMouse();
                            Mouse.SetCursor(_grabbingCursor);
                        }
                    };

                    e.Description.DescriptionMouseMoveEvent += (sender1, e1) =>
                    {
                        //Add mouse move event on every description here
                        Mouse.SetCursor(_grabCursor);
                    };

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

                    //Set
                    if (VideoMedia.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
                        SetSpaceLocation(space);

                    space.SpaceMouseDownEvent += (sender1, e1) =>
                        {
                            MouseEventArgs args = (MouseEventArgs)e1;

                            if (Mouse.LeftButton == MouseButtonState.Pressed)
                            {
                                //if the space was selected already, set it to null in the list
                                //and the property to false
                                if (space.IsSelected)
                                {
                                    space.IsSelected = false;
                                    _descriptionInfoTabViewModel.SpaceSelectedInList = null;
                                }
                                else
                                {
                                    _descriptionInfoTabViewModel.SpaceSelectedInList = space;
                                }

                                double xPos = args.GetPosition(AudioCanvas).X;

                                //prepare space for dragging
                                _originalPositionForDraggingSpace = xPos;
                                _spaceBeingDraggedOrResized = space;
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
                            MouseEventArgs args = (MouseEventArgs)e1;
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
                };
            #endregion

            #region Event Listeners for LoadingViewModel
            maincontrol.LoadingViewModel.PropertyChanged += (sender, e) =>
            {
                /* Set LoadingBorder to appear in front of everything when visible, otherwise put
                 * it behind everything. This allows it to sit behind in the XAML viewer.
                 */
                if (e.PropertyName.Equals("Visible"))
                {
                    if (maincontrol.LoadingViewModel.Visible)
                        Grid.SetZIndex(LoadingControl, 2);
                    else
                        Grid.SetZIndex(LoadingControl, -1);
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
                    double position = (VideoMedia.Position.TotalMilliseconds / _videoDuration) * (AudioCanvas.Width);
                    UpdateMarkerPosition(position - MarkerOffset);
                });

            }
            catch (System.Threading.Tasks.TaskCanceledException exception)
            {
                //do nothing this exception is thrown when the application is exited
                log.Warn("Task Cancelled exception", exception);
            }
        }

        #region View Listeners
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

                //Resizing end of space
                if (_spacesActionState == SpacesActionState.ResizingEndOfSpace)
                {
                    
                    double newWidth = _spaceBeingDraggedOrResized.Width + (xPos - _originalPositionForDraggingSpace);
                    double lengthInMillisecondsNewWidth = (_videoDuration / AudioCanvas.Width) * newWidth;
                    
                    //bounds checking
                    if (lengthInMillisecondsNewWidth < SpacesViewModel.MAX_SPACE_LENGTH_IN_MILLISECONDS)
                    {
                        newWidth = (AudioCanvas.Width / _videoDuration) * SpacesViewModel.MAX_SPACE_LENGTH_IN_MILLISECONDS;
                        //temporary fix, have to make the cursor attached to the end of the space somehow
                        AudioCanvas.ReleaseMouseCapture();
                    }
                    else if ((_spaceBeingDraggedOrResized.StartInVideo + lengthInMillisecondsNewWidth) > _videoDuration)
                    {
                        newWidth = (AudioCanvas.Width / _videoDuration) * (_videoDuration - _spaceBeingDraggedOrResized.StartInVideo);
                        //temporary fix, have to make the cursor attached to the end of the space somehow
                        AudioCanvas.ReleaseMouseCapture();
                    }
                    
                    _spaceBeingDraggedOrResized.Width = newWidth;
                    _originalPositionForDraggingSpace = xPos;
                    _spaceBeingDraggedOrResized.EndInVideo = _spaceBeingDraggedOrResized.StartInVideo + (_videoDuration / AudioCanvas.Width) * _spaceBeingDraggedOrResized.Width;

                    Mouse.SetCursor(Cursors.SizeWE);
                }
                //Resizing beginning of space
                else if (_spacesActionState == SpacesActionState.ResizingBeginningOfSpace)
                {
                    //left side of space
                    double newPosition = _spaceBeingDraggedOrResized.X + (xPos - _originalPositionForDraggingSpace);
                    double newPositionMilliseconds = (_videoDuration / AudioCanvas.Width) * newPosition;
                    double lengthOfSpaceInMilliseconds = _spaceBeingDraggedOrResized.StartInVideo - _spaceBeingDraggedOrResized.EndInVideo;

                    //bounds checking
                    if (newPositionMilliseconds < 0)
                    {
                        newPosition = 0;
                        //temporary fix, have to make the cursor attached to the end of the space somehow
                        AudioCanvas.ReleaseMouseCapture();
                    }
                    else if ((_spaceBeingDraggedOrResized.EndInVideo - newPositionMilliseconds) < SpacesViewModel.MAX_SPACE_LENGTH_IN_MILLISECONDS)
                    {
                        newPosition = (AudioCanvas.Width / _videoDuration) * (_spaceBeingDraggedOrResized.EndInVideo - SpacesViewModel.MAX_SPACE_LENGTH_IN_MILLISECONDS);                      
                        //temporary fix, have to make the cursor attached to the end of the space somehow
                        AudioCanvas.ReleaseMouseCapture();
                    }

                    _spaceBeingDraggedOrResized.X = newPosition;
                    _spaceBeingDraggedOrResized.StartInVideo = (_videoDuration / AudioCanvas.Width) * newPosition;
                    _spaceBeingDraggedOrResized.Width = (AudioCanvas.Width / _videoDuration) * (_spaceBeingDraggedOrResized.EndInVideo - _spaceBeingDraggedOrResized.StartInVideo);

                    _originalPositionForDraggingSpace = xPos;
                    Mouse.SetCursor(Cursors.SizeWE);
                }
                //Dragging a Space
                else if (_spacesActionState == SpacesActionState.Dragging)
                {
                    double newPosition = _spaceBeingDraggedOrResized.X + (xPos - _originalPositionForDraggingSpace);
                    double newPositionMilliseconds = (_videoDuration / AudioCanvas.Width) * newPosition;                  
                    double lengthOfSpaceMilliseconds = _spaceBeingDraggedOrResized.EndInVideo - _spaceBeingDraggedOrResized.StartInVideo;
                    //size in pixels of the space
                    double size = (AudioCanvas.Width / _videoDuration) * lengthOfSpaceMilliseconds;

                    if (newPositionMilliseconds < 0)
                        newPosition = 0;
                    else if ((newPositionMilliseconds + lengthOfSpaceMilliseconds) > _videoDuration)
                        newPosition = (AudioCanvas.Width / _videoDuration)*(_videoDuration - lengthOfSpaceMilliseconds);

                    _spaceBeingDraggedOrResized.X = newPosition;
                    _originalPositionForDraggingSpace = xPos;
                    _spaceBeingDraggedOrResized.StartInVideo = (_videoDuration / AudioCanvas.Width) * (_spaceBeingDraggedOrResized.X);
                    _spaceBeingDraggedOrResized.EndInVideo = _spaceBeingDraggedOrResized.StartInVideo + (_videoDuration / AudioCanvas.Width) * size;
                    Mouse.SetCursor(_grabbingCursor);
                }
            }
        }

        /// <summary>
        /// Updates the canvasWidth and canvasHeight variables everytime the canvas size is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioCanvasBorder_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            log.Info("Audio Canvas Border Sized Changed");

            //Video is loaded
            if (_videoDuration != -1)
            {
                SetTimeline();
            }
            //update marker to fit the entire AudioCanvas even when there's no video loaded
            else
            {
                Marker.Points[4] = new Point(Marker.Points[4].X, AudioCanvasBorder.ActualHeight);
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
            }
        }

        private void DescriptionCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            //while the mouse is moving over the description canvas and if a description is clicked (DescriptionCanvas.IsMouseCaptured)
            //update the position of the description and the start and end times in the video
            if (DescriptionCanvas.IsMouseCaptured)
            {
                double newPosition = _descriptionBeingDragged.X + (e.GetPosition(DescriptionCanvas).X - _originalPositionForDraggingDescription);
                double newPositionMilliseconds = (_videoDuration / AudioCanvas.Width) * newPosition;
                //size in pixels of the description
                double size = (AudioCanvas.Width / _videoDuration) * (_descriptionBeingDragged.EndWaveFileTime - _descriptionBeingDragged.StartWaveFileTime);
                double lengthOfDescriptionMilliseconds = _descriptionBeingDragged.EndInVideo - _descriptionBeingDragged.StartInVideo;

                //bounds checking when dragging the description
                if (newPositionMilliseconds < 0)
                    newPosition = 0;
                else if ((newPositionMilliseconds + lengthOfDescriptionMilliseconds) > _videoDuration)
                    newPosition = (AudioCanvas.Width / _videoDuration) * (_videoDuration - lengthOfDescriptionMilliseconds);

                _descriptionBeingDragged.X = newPosition;
                _originalPositionForDraggingDescription = e.GetPosition(DescriptionCanvas).X;
                _descriptionBeingDragged.StartInVideo = (_videoDuration / AudioCanvas.Width) * (_descriptionBeingDragged.X);
                _descriptionBeingDragged.EndInVideo = _descriptionBeingDragged.StartInVideo + (_descriptionBeingDragged.EndWaveFileTime - _descriptionBeingDragged.StartWaveFileTime);
                Mouse.SetCursor(_grabbingCursor);
            }
        }

        private void AudioCanvas_RecordRightClickPosition(object sender, MouseButtonEventArgs e)
        {
            //record the position in which you right clicked on the canvas
            //this position is used to calculate where on the audio canvas to draw a space
            RightClickPointOnAudioCanvas = e.GetPosition(AudioCanvas);
        }

        /// <summary>
        /// Gets executed when the area in the NumberTimeline canvas gets clicked
        /// It changes the position of the video then redraws the marker in the correct spot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberTimeline_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //execute the pause command because we want to pause the video when someone is clicking through the video
                _videoControl.PauseCommand.Execute(this);

                var xPosition = e.GetPosition(NumberTimelineBorder).X;
                var newValue = (xPosition / AudioCanvas.Width) * _videoDuration;

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
            _videoControl.PositionTimeLabel = VideoMedia.Position;
        }

        /// <summary>
        /// Updates the video position to a spot in the video
        /// </summary>
        /// <param name="vidPos">the new position in the video</param>
        private void UpdateVideoPosition(int vidPos)
        {
            VideoMedia.Position = new TimeSpan(0, 0, 0, 0, vidPos);
            _videoControl.PositionTimeLabel = VideoMedia.Position;
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

            double width = _canvasWidth;
            double singlePageWidth = 0;
            double scrolledAmount = 0;

            Dispatcher.Invoke(delegate
            {
                singlePageWidth = TimeLine.ActualWidth;
                scrolledAmount = TimeLine.HorizontalOffset;
            });
            double scrollOffsetRight = PageScrollPercent * singlePageWidth;
            if (!(xPos - scrolledAmount >= (scrollOffsetRight))) return false;

            Dispatcher.Invoke(delegate { TimeLine.ScrollToHorizontalOffset(scrollOffsetRight + scrolledAmount); });
            return true;
        }

        /// <summary>
        /// Calculates the width required for the audioCanvas
        /// and then sets _canvasWidth to this value
        /// </summary>
        private double calculateWidth()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double staticCanvasWidth = (_videoDuration / (PageTimeBeforeCanvasScrolls * 1000)) * screenWidth;
            this.AudioCanvas.MaxWidth = staticCanvasWidth;
            return staticCanvasWidth;
        }

        #endregion

        #region graphics Functions
        /// <summary>
        /// Draws the wavform of the video audio on the canvas
        /// </summary>
        private void DrawWaveForm()
        {
            log.Info("Drawing wave form");

            double width = TimeLine.ActualWidth;

            if (_videoControl.Waveform == null || _canvasWidth == 0 || width == 0)
                return;
            List<short> data = _videoControl.Waveform.Data;

            double samplesPerPixel = data.Count / _canvasWidth;

            double soundWaveOffset = NumberTimeline.ActualHeight;
            double soundWaveHeight = AudioCanvas.ActualHeight - soundWaveOffset;
            double middle = soundWaveHeight / 2;
            double yscale = middle;

            AudioCanvas.Children.Clear();
            //Re-add Children components
            AudioCanvas.Children.Add(NumberTimelineBorder);
            
            int begin = (int)TimeLine.HorizontalOffset;
            int ratio = _videoControl.Waveform.Header.NumChannels == 2 ? 40 : 80;
            double samples_per_second =
                (_videoControl.Waveform.Header.SampleRate * (_videoControl.Waveform.Header.BlockAlign / (double)ratio));
            
            double offset_time;
            double sample_start;
            double pixel = begin;

            while ( pixel <= begin + width )
            {
                offset_time = (_videoDuration / (_canvasWidth * 1000)) * pixel;
                sample_start = samples_per_second * offset_time;

                if (sample_start + samplesPerPixel < data.Count)
                { 
                    double max = (double)data.GetRange((int)sample_start, (int)samplesPerPixel).Max() / short.MaxValue;
                    double min = (double)data.GetRange((int)sample_start, (int)samplesPerPixel).Min() / short.MaxValue;
                    AudioCanvas.Children.Add(new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Black,
                        SnapsToDevicePixels = true, //Turn off anti-aliasing effect
                        Y1 = middle + max * yscale + soundWaveOffset,
                        Y2 = middle + min * yscale + soundWaveOffset,
                        X1 = pixel,
                        X2 = pixel,
                    });
    
                }
                pixel++;
            }
            AudioCanvas.Children.Add(SpacesItemControl);
            AudioCanvas.Children.Add(Marker);
            double canvasWidth = _canvasWidth;

            //Number of lines needed for the entire video
            int numlines = (int)(_videoDuration / (LineTime * 1000));
            int beginLine = (int)((numlines / _canvasWidth) * TimeLine.HorizontalOffset);
            int endLine = beginLine + (int)((numlines / _canvasWidth) * TimeLine.ActualWidth) + 1;
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
                        X1 = canvasWidth / numlines * i,
                        X2 = canvasWidth / numlines * i,
                    });
                }
                else
                {
                    NumberTimeline.Children.Add(new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Black,
                        Y1 = 0,
                        Y2 = NumberTimeline.ActualHeight / 2,
                        X1 = canvasWidth / numlines * i,
                        X2 = canvasWidth / numlines * i
                    });
                }
            }
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
                space.Height = AudioCanvas.ActualHeight - NumberTimeline.ActualHeight;
        }

        /// <summary>
        /// Update's the instance variables that keep track of the timeline height and width, and
        /// calculates the size of the timeline if the width of the audio canvas is greater then the
        /// timeline width it automatically overflows and scrolls due to the scrollview then update
        /// the width of the marker to match the audio canvas
        /// </summary>
        private void SetTimeline()
        {
            log.Info("Setting timeline");
            NumberTimeline.Width = _canvasWidth;
            AudioCanvas.Width = _canvasWidth;
            DescriptionCanvas.Width = _canvasWidth;
            Marker.Points[4] = new Point(Marker.Points[4].X, AudioCanvasBorder.ActualHeight);

            DrawWaveForm();
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
            space.Y = NumberTimeline.ActualHeight;
            space.Height = AudioCanvas.ActualHeight - NumberTimeline.ActualHeight;
            space.Width = (AudioCanvas.Width / _videoDuration) * (space.EndInVideo - space.StartInVideo);
        }
        #endregion

        #region Control Event Handlers
        private void ContextMenu_AddSpace(object sender, RoutedEventArgs e)
        {
            var space = new Space();

            double middle = RightClickPointOnAudioCanvas.X;  // going to be the middle of the space
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

            _spacesViewModel.AddSpace(space);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            bool success = _mainControl.TryExit();

            if (!success)
                e.Cancel = true;
            else
                log.Info("Exiting program...");
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
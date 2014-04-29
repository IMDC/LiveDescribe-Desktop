using LiveDescribe.Converters;
using LiveDescribe.View_Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Constants
        private const double MarkerOffset = 10.0;
        /// <summary>when the marker hits 95% of the page it scrolls</summary>
        private const double PageScrollPercent = 0.95;
        /// <summary>30 seconds page time before audiocanvas  & descriptioncanvas scroll</summary>
        private const double PageTimeBeforeCanvasScrolls = 30;
        private const double LineTime = 1; //each line in the NumberTimeline appears every 1 second
        private const int LongLineTime = 5; // every 5 LineTimes, you get a Longer Line

        /// <summary>The width of the entire canvas.</summary>
        private double _canvasWidth = 0;
        private double _videoDuration = -1;
        private readonly VideoControl _videoControl;
        private readonly PreferencesViewModel _preferences;
        /// <summary>used to format a timespan object which in this case in the videoMedia.Position</summary>
        private readonly DescriptionViewModel _descriptionViewModel;
        private readonly TimeConverterFormatter _formatter;
        private double _originalPosition = -1;

        public MainWindow()
        {
            var splashScreen = new SplashScreen("../Images/LiveDescribe-Splashscreen.png");
            splashScreen.Show(true);
            Thread.Sleep(2000);
            InitializeComponent();

            var maincontrol = new MainControl(VideoMedia);

            DataContext = maincontrol;

            _videoControl = maincontrol.VideoControl;
            _preferences = maincontrol.PreferencesViewModel;
            _descriptionViewModel = maincontrol.DescriptionViewModel;

            _formatter = new TimeConverterFormatter();

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

            #region Event Listeners For Main Control (Pause, Play, Mute, FastForward, Rewind)
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
                CurrentTimeLabel.Text = "00:00:000";
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
                Console.WriteLine("Marker Mouse Down");
                Marker.CaptureMouse();
            };

            //updates the video position when the mouse is released on the Marker
            maincontrol.VideoControl.OnMarkerMouseUpRequested += (sender, e) =>
                {
                    Console.WriteLine("Marker Mouse Up");
                    var newValue = ((Canvas.GetLeft(Marker) + MarkerOffset) / AudioCanvas.Width) * _videoDuration;

                    UpdateVideoPosition((int)newValue);
                    Marker.ReleaseMouseCapture();
                };

            //updates the canvas and video position when the Marker is moved
            maincontrol.VideoControl.OnMarkerMouseMoveRequested += (sender, e) =>
                {
                    Console.WriteLine("Marker Mouse Move");
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
            #endregion

            #region Event Listeners for PreferencesViewModel

            //create the preferences window when the option is clicked
            _preferences.ShowPreferencesRequested += (sender, e) =>
                {
                    var preferencesWindow = new PreferencesWindow();
                    preferencesWindow.DataContext = _preferences;
                    preferencesWindow.ShowDialog();
                };

            #endregion

            #region EventListeners for DescriptionViewModel

            _descriptionViewModel.RecordRequestedMicrophoneNotPluggedIn += (sender, e) =>
                {
                    //perhaps show a popup when the Record button is pressed and there is no microphone plugged in
                    Console.WriteLine("NO MICROPHONE CONNECTED!");
                };

            //When a description is added, attach an event to the StartInVideo and EndInVideo properties
            //so when those properties change it redraws them
            _descriptionViewModel.AddDescriptionEvent += (sender, e) =>
                {
                    //set the Description values that are bound to the graphics in MainWindow.xaml
                    e.Description.X = (e.Description.StartInVideo / _videoDuration) * AudioCanvas.Width;
                    e.Description.Y = 0;
                    e.Description.Width = (AudioCanvas.Width / _videoDuration) * (e.Description.EndWaveFileTime - e.Description.StartWaveFileTime);
                    e.Description.Height = DescriptionCanvas.ActualHeight;

                    e.Description.DescriptionMouseDownEvent += (sender1, e1) =>
                    {
                        //Add mouse down event on every description here
                        MouseEventArgs e2 = (MouseEventArgs)e1;
                        _originalPosition = e2.GetPosition(DescriptionCanvas).X;
                        Console.WriteLine("Description Mouse Down");
                    };

                    e.Description.DescriptionMouseMoveEvent += (sender1, e1) =>
                    {
                        //Add mouse move event on every description here

                        MouseEventArgs e2 = (MouseEventArgs)e1;
                        if (e2.LeftButton == MouseButtonState.Pressed && _originalPosition != -1)
                        {
                            e.Description.X = e.Description.X + (e2.GetPosition(DescriptionCanvas).X - _originalPosition);
                            _originalPosition = e2.GetPosition(DescriptionCanvas).X;
                            e.Description.StartInVideo = (_videoDuration / AudioCanvas.Width) * (e.Description.X);
                            e.Description.EndInVideo = e.Description.StartInVideo + (e.Description.EndWaveFileTime - e.Description.StartWaveFileTime);
                        }
                        else
                        {
                            _originalPosition = -1;
                        }
                        Console.WriteLine("Description Mouse Move");
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
                Console.WriteLine(exception.ToString());
            }
        }

        #region View Listeners
        /// <summary>
        /// Updates the canvasWidth and canvasHeight variables everytime the canvas size is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioCanvasBorder_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            Console.WriteLine("Audio Canvas Border Sized Changed");
            if (_videoDuration != -1)
            {
                SetTimeline();
            }
        }

        /// <summary>
        /// Gets executed when the area in the NumberTimeline canvas gets clicked
        /// It changes the position of the video then redraws the marker in the correct spot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberTimeline_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //execute the pause command because we want to pause the video when someone is clicking through the video
            Console.WriteLine("Number TimeLine Mouse Down");
            _videoControl.PauseCommand.Execute(this);

            var xPosition = e.GetPosition(NumberTimelineBorder).X;
            var newValue = (xPosition / AudioCanvas.Width) * _videoDuration;

            UpdateMarkerPosition(xPosition - MarkerOffset);
            UpdateVideoPosition((int)newValue);
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
            CurrentTimeLabel.Text = (string)_formatter.Convert(VideoMedia.Position, VideoMedia.Position.GetType(),
                this, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Updates the video position to a spot in the video
        /// </summary>
        /// <param name="vidPos">the new position in the video</param>
        private void UpdateVideoPosition(int vidPos)
        {
            VideoMedia.Position = new TimeSpan(0, 0, 0, 0, vidPos);
            CurrentTimeLabel.Text = (string)_formatter.Convert(VideoMedia.Position, VideoMedia.Position.GetType(),
                this, CultureInfo.CurrentCulture);
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
            Console.WriteLine("Drawing wave form");
            List<float> data = _videoControl.AudioData;
            if (data == null)
                return;

            double width = TimeLine.ActualWidth;
            double fullWidth = _canvasWidth;
            double height = AudioCanvas.ActualHeight;
            double binSize = Math.Floor(data.Count / Math.Max(fullWidth, 1));

            AudioCanvas.Children.Clear();
            //Re-add Children components
            AudioCanvas.Children.Add(NumberTimelineBorder);
            AudioCanvas.Children.Add(Marker);

            int begin = (int)TimeLine.HorizontalOffset;
            int end = (int)(TimeLine.HorizontalOffset + width);

            for (int pixel = begin; pixel < end; pixel++)
            {
                //get min and max from bin
                List<float> bin = data.GetRange((int)(pixel * binSize), (int)binSize);

                float min = bin.Min();
                float max = bin.Max();

                double Y1 = height * min;
                double Y2 = height * max;
                double X1 = pixel;
                double X2 = pixel;

                AudioCanvas.Children.Add(new Line
                {
                    Stroke = System.Windows.Media.Brushes.Black,
                    Y1 = Y1,
                    Y2 = Y2,
                    X1 = X1,
                    X2 = X2,
                });
            }
            double pages = _videoDuration / (PageTimeBeforeCanvasScrolls * 1000);
            double canvasWidth = _canvasWidth;

            var numlines = (int)(_videoDuration / (LineTime * 1000));
            //Clear the canvas because we don't want the remaining lines due to importing a new video
            //or resizing the window
            NumberTimeline.Children.Clear();

            for (int i = 0; i < numlines; ++i)
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

        /// <summary>
        /// Update's the instance variables that keep track of the timeline height and width, and
        /// calculates the size of the timeline if the width of the audio canvas is greater then the
        /// timeline width it automatically overflows and scrolls due to the scrollview then update
        /// the width of the marker to match the audio canvas
        /// </summary>
        private void SetTimeline()
        {
            Console.WriteLine("Setting Timeline");
            NumberTimeline.Width = _canvasWidth;
            AudioCanvas.Width = _canvasWidth;
            DescriptionCanvas.Width = _canvasWidth;

            DrawWaveForm();
        }
        #endregion
    }
}
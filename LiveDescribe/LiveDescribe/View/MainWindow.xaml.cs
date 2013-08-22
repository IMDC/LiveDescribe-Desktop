using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using LiveDescribe.Converters;
using LiveDescribe.View_Model;
using System.Windows.Threading;
using System.Collections.Generic;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private double _videoDuration;
        private readonly DispatcherTimer _videoTimer;
        private const double PageTime = 30; //30 seconds page time before audiocanvas  & descriptioncanvas scroll
        private const double LineTime = 1; //each line in the NumberTimeline appears every 1 second
        private const int LongLineTime = 5; // every 5 LineTimes, you get a Longer Line
        private readonly VideoControl _videoControl;
        private readonly TimeConverterFormatter _formatter; 

        public MainWindow()
        {
            InitializeComponent();

            var mc = new MainControl(VideoMedia);
            _videoTimer = new DispatcherTimer();
            _videoTimer.Tick += Play_Tick;
            _videoTimer.Interval = new TimeSpan(0,0,0,0,1);
            DataContext = mc;
            _videoControl = mc.VideoControl;
            _formatter = new TimeConverterFormatter();

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

            VideoMedia.MediaEnded += (sender, e) =>
            {
                _videoTimer.Stop();
                VideoMedia.Stop();
                UpdateMarkerPosition(-10);
            };

            #endregion
            
            #region Event Listeners For VideoControl


            //listens for PlayRequested Event
            mc.VideoControl.PlayRequested += (sender, e) =>
                {
                    _videoTimer.Start();
                    VideoMedia.Play();
                    var newValue = ((Canvas.GetLeft(Marker) + 10) / AudioCanvas.Width) * _videoDuration;
                    UpdateVideoPosition((int)newValue);
                };

            //listens for PauseRequested Event
            mc.VideoControl.PauseRequested += (sender, e) =>
                {
                    VideoMedia.Pause();
                    _videoTimer.Stop();
                   
                };
            
            //listens for MuteRequested Event
            mc.VideoControl.MuteRequested += (sender, e) =>
                {
                    VideoMedia.IsMuted = !VideoMedia.IsMuted;
                };

            //listens for VideoOpenedRequested event
            //this event only gets thrown when if the MediaFailed event doesn't occur
            //and as soon as the video is loaded when play is pressed
            mc.VideoControl.VideoOpenedRequested += (sender, e) =>
                {
                    _videoDuration = VideoMedia.NaturalDuration.TimeSpan.TotalMilliseconds;
                    SetTimeline();
                    DrawWaveForm();
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
            double position = (VideoMedia.Position.TotalMilliseconds / _videoDuration) * (AudioCanvas.Width );
            UpdateMarkerPosition(position - 10);
        }

        #region View Listeners
        /// <summary>
        /// Updates the canvasWidth and canvasHeight variables everytime the canvas size is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioCanvasBorder_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            SetTimeline();
        }

        /// <summary>
        /// Updates the video position when the mouse is released
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="mouseButtonEventArgs">e</param>
        private void Marker_OnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var newValue = ((Canvas.GetLeft(Marker) + 10) / AudioCanvas.Width) * _videoDuration; 
          
            UpdateVideoPosition((int)newValue);
            Marker.ReleaseMouseCapture();
        }

        /// <summary>
        /// Sets the mouse to be capured when the mouse is clicked
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void Marker_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Marker.CaptureMouse();
        }

        /// <summary>
        /// Updates the Marker Position based on the Mouse x coord
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void Marker_OnMouseMove(object sender, MouseEventArgs e)
        {

            var xPosition = e.GetPosition(AudioCanvasBorder).X;

            if (!Marker.IsMouseCaptured) return;
            if (xPosition < -10)
            {
                Canvas.SetLeft(Marker, -10); 
            }
            else if (xPosition > AudioCanvas.Width - 1)
            {
                Canvas.SetLeft(Marker, AudioCanvas.Width - 1);
            }
            else
            {
                Canvas.SetLeft(Marker, xPosition - 10);
            }
           
            var newValue = (xPosition / AudioCanvas.Width) * _videoDuration;
            UpdateVideoPosition((int)newValue);
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
            _videoControl.PauseCommand.Execute(this);

            var xPosition = e.GetPosition(NumberTimelineBorder).X;
            var newValue = (xPosition / AudioCanvas.Width) * _videoDuration;

            UpdateMarkerPosition(xPosition - 10);
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
            CurrentTimeLabel.Text = (string)_formatter.Convert(VideoMedia.Position, VideoMedia.Position.GetType(), this, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Updates the video position to a spot in the video
        /// </summary>
        /// <param name="vidPos">the new position in the video</param>
        private void UpdateVideoPosition(int vidPos)
        {
            VideoMedia.Position = new TimeSpan(0, 0, 0, 0, vidPos);
            CurrentTimeLabel.Text = (string)_formatter.Convert(VideoMedia.Position, VideoMedia.Position.GetType(), this, CultureInfo.CurrentCulture);
        }
        #endregion

        #region graphics Functions
        /// <summary>
        /// Draws the wavform of the video audio on the canvas
        /// </summary>
        private void DrawWaveForm()
        {
            List<float> data = _videoControl.AudioData;
            double width = AudioCanvas.Width;
            double height = AudioCanvas.ActualHeight;
            double binSize = Math.Floor(data.Count / width);

            for (int pixel = 0; pixel < width; pixel++)
            {
                //get min and max from bin
                List<float> bin = data.GetRange((int)(pixel * binSize), (int)binSize);
            
                bin.Sort();
                float min = bin[0];
                float max = bin[bin.Count - 1];
                //Console.WriteLine("Min: " + min + " Max: " + max);

                //draw the line on this pixel
                var wavLine = new Line
                {
                    Stroke = System.Windows.Media.Brushes.Black,
                    Y1 = height * min,
                    Y2 = height * max,
                    X1 = pixel,
                    X2 = pixel
                };
                AudioCanvas.Children.Add(wavLine);
            }
        }

        /// <summary>
        ///Update's the instance variables that keep track of the timeline height and width, and calculates the size of the timeline
        ///if the width of the audio canvas is greater then the timeline width it automatically overflows and scrolls due to the scrollview
        ///then update the width of the marker to match the audio canvas
        /// </summary>
        private void SetTimeline()
        {

            double pages = _videoDuration / (PageTime * 1000);
            double width = TimeLine.ActualWidth * pages;

            var numlines = (int)(_videoDuration / (LineTime * 1000));
            //Clear the canvas because we don't want the remaining lines due to importing a new video
            //or resizing the window
            NumberTimeline.Children.Clear();

            for (int i = 0; i < numlines; ++i)
            {

                Line splitLine;

                if (i % LongLineTime == 0)
                {
                    splitLine = new Line
                    {
                        Stroke = System.Windows.Media.Brushes.Blue,
                        StrokeThickness = 1.5,
                        Y1 = 0,
                        Y2 = NumberTimeline.ActualHeight / 1.2,
                        X1 = width / numlines * i,
                        X2 = width / numlines * i,
                    };

                    NumberTimeline.Children.Add(splitLine);
                    continue;
                }

                splitLine = new Line
                {
                    Stroke = System.Windows.Media.Brushes.Black,
                    Y1 = 0,
                    Y2 = NumberTimeline.ActualHeight / 2,
                    X1 = width / numlines * i,
                    X2 = width / numlines * i
                };

                NumberTimeline.Children.Add(splitLine);
            }

            NumberTimeline.Width = width;
            AudioCanvas.Width = width;
            Marker.Points[4] = new Point(Marker.Points[4].X, AudioCanvasBorder.ActualHeight);
        }
        #endregion


    }
}

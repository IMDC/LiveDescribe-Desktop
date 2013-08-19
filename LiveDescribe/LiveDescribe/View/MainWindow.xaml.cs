using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiveDescribe.View_Model;
using System.Windows.Threading;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private double _videoDuration;
        private double _audioTimeLineHeight;
        private double _audioTimeLineWidth;
        private readonly DispatcherTimer _videoTimer;
        private const double PageTime = 30; //30 seconds page time before audiocanvas  & descriptioncanvas scroll

        public MainWindow()
        {
            InitializeComponent();

            MainControl mc = new MainControl(VideoMedia);
            _videoTimer = new DispatcherTimer();
            _videoTimer.Tick += Play_Tick;
            _videoTimer.Interval = new TimeSpan(0,0,0,0,1);
            DataContext = mc;


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
                Canvas.SetLeft(Marker, -10);
            };

            #endregion
            
            #region Event Listeners For VideoControl


            //listens for PlayRequested Event
            mc.VideoControl.PlayRequested += (sender, e) =>
                {
                    _videoTimer.Start();
                    VideoMedia.Play();
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
                   // Console.WriteLine(VideoMedia.NaturalDuration.TimeSpan.TotalMilliseconds);
                //    Marker.Maximum = VideoMedia.NaturalDuration.TimeSpan.TotalMilliseconds;
                    SetTimeline(); 
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
            double position = (VideoMedia.Position.TotalMilliseconds / _videoDuration) * (_audioTimeLineWidth);
            Canvas.SetLeft(Marker, (int)position);
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
            double newValue = (Canvas.GetLeft(Marker) / _audioTimeLineWidth) * _videoDuration;
            VideoMedia.Position = new TimeSpan(0, 0, 0, 0, (int)newValue);
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
        /// <param name="sender">sneder</param>
        /// <param name="e">e</param>
        private void Marker_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Marker.IsMouseCaptured)
            {
                if (e.GetPosition(AudioCanvasBorder).X < -10)
                {
                    Canvas.SetLeft(Marker, -10); 
                }
                else if (e.GetPosition(AudioCanvasBorder).X > _audioTimeLineWidth - 1)
                {
                    Canvas.SetLeft(Marker, _audioTimeLineWidth - 1);
                }
                else
                {
                    Canvas.SetLeft(Marker, e.GetPosition(AudioCanvasBorder).X);

                }
            }
        }
        #endregion

        #region Helper Functions
        /// <summary>
        ///Update's the instance variables that keep track of the timeline height and width, and calculates the size of the timeline
        ///if the width of the audio canvas is greater then the timeline width it automatically overflows and scrolls due to the scrollview
        ///then update the width of the marker to match the audio canvas
        /// </summary>
        private void SetTimeline()
        {
            _audioTimeLineHeight = TimeLine.ActualHeight;
            _audioTimeLineWidth = TimeLine.ActualWidth;

            double pages = _videoDuration / (PageTime * 1000);
            double width = _audioTimeLineWidth * pages;

            AudioCanvas.Width = width;

            this.Marker.Points[4] = new Point(this.Marker.Points[4].X , this.AudioCanvasBorder.ActualHeight);
        }
        #endregion

        
    }
}

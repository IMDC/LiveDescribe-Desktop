using System;
using System.Windows;
using LiveDescribe.View_Model;
using System.Windows.Threading;
using Microsoft.TeamFoundation.Controls.WPF;

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

        private const double PageTime = 30; //30 seconds (in Milliseconds) page time before audiocanvas  & descriptioncanvas scroll

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
                    VideoMedia.Stop();
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
                    _videoDuration = VideoMedia.NaturalDuration.TimeSpan.TotalSeconds;
                   // Console.WriteLine(VideoMedia.NaturalDuration.TimeSpan.TotalMilliseconds);
                    Marker.Maximum = VideoMedia.NaturalDuration.TimeSpan.TotalMilliseconds;
                    SetTimeline(); 
                };

            #endregion
            
            
         /*   #region Event Listeners for Marker
            Marker.Thumb.DragCompleted += (sender, e) =>
                {
                    VideoMedia.Position = new TimeSpan(0, 0, 0, 0, (int)Marker.Value);
                };
            #endregion*/
        }

        private void Play_Tick(object sender, EventArgs e)
        {
            //update marker position from the video controller
            
           // int position = (int) Math.Round((VideoMedia.Position.Seconds / _videoDuration) * _audioTimeLineWidth);
           // double initialPos = Canvas.GetLeft(Marker);
           // double newPos = initialPos + 1;
           // Canvas.SetLeft(Marker, newPos);
            Console.WriteLine((VideoMedia.Position.Seconds * 1000) + VideoMedia.Position.Milliseconds);
            Marker.Value = (VideoMedia.Position.Seconds * 1000) + VideoMedia.Position.Milliseconds;

            // Console.WriteLine("Position: " + position);
            //draw marker at correct position
        }

        #region View Listeners
        /// <summary>
        /// Updates the canvasWidth and canvasHeight variables everytime the canvas size is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timeLine_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            SetTimeline();
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

            double pages = _videoDuration / PageTime;
            double width = _audioTimeLineWidth * pages;

            AudioCanvas.Width = width;
            Marker.Width = width;
        }
        #endregion

    }
}

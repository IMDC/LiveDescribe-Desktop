using System;
using System.Windows;
using System.Windows.Controls;
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

        private int pageTime = 30; //30 seconds page time before audiocanvas  & descriptioncanvas scroll

        public MainWindow()
        {
            InitializeComponent();

            MainControl mc = new MainControl();
   
            _videoTimer = new DispatcherTimer();
            _videoTimer.Tick += Play_Tick;
            _videoTimer.Interval = new TimeSpan(0, 0, 0, 1);

            DataContext = mc;
           

            #region Event Listeners
            //listens for PlayRequested Event
            mc.VideoControl.PlayRequested += (sender, e) =>
                {
                    _videoTimer.Start();
                  //  this.storyBoard.Begin(this);
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

            mc.VideoControl.VideoOpenedRequested += (sender, e) =>
                {
                    _videoDuration = VideoMedia.NaturalDuration.TimeSpan.TotalSeconds;
                    Console.WriteLine("DURATION: " + _videoDuration);
                };
            #endregion
        }

        private void Play_Tick(object sender, EventArgs e)
        {
            //update marker position from the video controller
            //something like this mc.VideoControl.CalculateMarkerPosition(TimeToMoveMarkerTo, pageSizeTime)
            
            int position = (int) Math.Round((VideoMedia.Position.Seconds / _videoDuration) * _audioTimeLineWidth);
            double initialPos = Canvas.GetLeft(Marker);
            double newPos = initialPos + 1;
            Canvas.SetLeft(Marker, newPos);

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
            _audioTimeLineHeight = TimeLine.ActualHeight;
            _audioTimeLineWidth = TimeLine.ActualWidth;
            Console.WriteLine("WIDTH: " + _audioTimeLineWidth);
            //Console.WriteLine("HEIGHT: " + this.audioCanvasHeight);
            SetPaging();

            //the 4th point is the bottom point of the marker, adjusting the y value of this point
            Marker.Points[4] = new Point(Marker.Points[4].X, AudioCanvasBorder.ActualHeight);
        }
        #endregion

        #region Helper Functions
        /// <summary>
        /// Sets the width of the audio canvas in the scrollview based off the pageTime and the total duration of the audio, if the AudioCanvas Width is greater
        /// then the ActualWidth of the AudioCanvas then it scrolls to compensate for the additional width
        /// </summary>
        private void SetPaging()
        {
            double pages = (4.24 * 60) / pageTime;
           // double width = AudioCanvasBorder.ActualWidth * pages;
            double width = _audioTimeLineWidth*pages;
            //Console.WriteLine("Width: " + this.audioCanvasBorder.ActualWidth);
            AudioCanvas.Width = width;
        }
        #endregion
       
    }
}

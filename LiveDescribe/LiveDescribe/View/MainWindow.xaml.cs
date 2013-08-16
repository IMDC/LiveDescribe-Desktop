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
        private double _audioCanvasHeight;
        private double _audioCanvasWidth;
        private DispatcherTimer videoTimer;

        private int pageTime = 30; //30 seconds page time before audiocanvas  & descriptioncanvas scroll

        public MainWindow()
        {
            InitializeComponent();

            MainControl mc = new MainControl();
   
            videoTimer = new DispatcherTimer();
            videoTimer.Tick += Play_Tick;
            //videoTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            DataContext = mc;
           

            #region Event Listeners
            //listens for PlayRequested Event
            mc.VideoControl.PlayRequested += (sender, e) =>
                {
                    videoTimer.Start();
                  //  this.storyBoard.Begin(this);
                    VideoMedia.Play();
                };

            //listens for PauseRequested Event
            mc.VideoControl.PauseRequested += (sender, e) =>
                {
                    VideoMedia.Pause();
                    videoTimer.Stop();
                   
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
            
            int position = (int) Math.Round((VideoMedia.Position.Seconds / _videoDuration) * _audioCanvasWidth);
            double initialPos = Canvas.GetLeft(Marker);
            double newPos = initialPos + 1;
            Canvas.SetLeft(Marker, newPos);

           // Console.WriteLine("Position: " + position);
            //draw marker at correct position
        }


        /// <summary>
        /// Updates the canvasWidth and canvasHeight variables everytime the canvas size is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timeLine_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            _audioCanvasHeight = AudioCanvasBorder.ActualHeight;
            _audioCanvasWidth = AudioCanvasBorder.ActualWidth;
            //  Console.WriteLine("WIDTH: " + this.audioCanvasWidth);
            //  Console.WriteLine("HEIGHT: " + this.audioCanvasHeight);
            setPaging();

            //the 4th point is the bottom point of the marker, adjusting the y value of this point
            Marker.Points[4] = new Point(Marker.Points[4].X, _audioCanvasHeight);
        }

        #region Helper Functions

        /// <summary>
        /// 
        /// </summary>
        private void setPaging()
        {
            double pages = 30.093 / pageTime;
            double width;
            width = AudioCanvasBorder.ActualWidth * pages;
            //Console.WriteLine("Width: " + this.audioCanvasBorder.ActualWidth);
            AudioCanvas.Width = width;
            
        }


        #endregion
       
    }
}

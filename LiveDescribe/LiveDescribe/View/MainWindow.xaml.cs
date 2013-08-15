using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            videoTimer.Tick += new EventHandler(Play_Tick);
            //videoTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            DataContext = mc;
           

            #region Event Listeners
            //listens for PlayRequested Event
            mc.VideoControl.PlayRequested += (sender, e) =>
                {
                    this.videoTimer.Start();
                  //  this.storyBoard.Begin(this);
                    this.VideoMedia.Play();
                };

            //listens for PauseRequested Event
            mc.VideoControl.PauseRequested += (sender, e) =>
                {
                    this.VideoMedia.Pause();
                    this.videoTimer.Stop();
                   
                };
            
            //listens for MuteRequested Event
            mc.VideoControl.MuteRequested += (sender, e) =>
            {
                this.VideoMedia.IsMuted = !this.VideoMedia.IsMuted;
            };

            mc.VideoControl.VideoOpenedRequested += (sender, e) =>
                {
                    this._videoDuration = VideoMedia.NaturalDuration.TimeSpan.TotalSeconds;
                    Console.WriteLine("DURATION: " + this._videoDuration);
                };
            #endregion
        }

        private void Play_Tick(object sender, EventArgs e)
        {
            //update marker position from the video controller
            //something like this mc.VideoControl.CalculateMarkerPosition(TimeToMoveMarkerTo, pageSizeTime)
            
            int position = (int) Math.Round((this.VideoMedia.Position.Seconds / this._videoDuration) * this._audioCanvasWidth);
            double initialPos = Canvas.GetLeft(this.Marker);
            double newPos = initialPos + 1;
            Canvas.SetLeft(this.Marker, newPos);

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
            this._audioCanvasHeight = this.AudioCanvasBorder.ActualHeight;
            this._audioCanvasWidth = this.AudioCanvasBorder.ActualWidth;
            //  Console.WriteLine("WIDTH: " + this.audioCanvasWidth);
            //  Console.WriteLine("HEIGHT: " + this.audioCanvasHeight);
            setPaging();

            //the 4th point is the bottom point of the marker, adjusting the y value of this point
            this.Marker.Points[4] = new Point(this.Marker.Points[4].X, this._audioCanvasHeight);
        }

        #region Helper Functions

        /// <summary>
        /// 
        /// </summary>
        private void setPaging()
        {
            double pages = 30.093 / this.pageTime;
            double width;
            width = this.AudioCanvasBorder.ActualWidth * pages;
            //Console.WriteLine("Width: " + this.audioCanvasBorder.ActualWidth);
            this.AudioCanvas.Width = width;
            
        }


        #endregion
       
    }
}

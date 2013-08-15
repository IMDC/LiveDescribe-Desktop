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
using LiveDescribe.Graphics;
using System.Windows.Threading;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private double videoDuration;
        private double audioCanvasHeight;
        private double audioCanvasWidth;
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
                    this.videoMedia.Play();
                };

            //listens for PauseRequested Event
            mc.VideoControl.PauseRequested += (sender, e) =>
                {
                    this.videoMedia.Pause();
                    this.videoTimer.Stop();
                   
                };
            
            //listens for MuteRequested Event
            mc.VideoControl.MuteRequested += (sender, e) =>
            {
                this.videoMedia.IsMuted = !this.videoMedia.IsMuted;
            };

            mc.VideoControl.VideoOpenedRequested += (sender, e) =>
                {
                    this.videoDuration = videoMedia.NaturalDuration.TimeSpan.TotalSeconds;
                    Console.WriteLine("DURATION: " + this.videoDuration);
                };
            #endregion
        }

        private void Play_Tick(object sender, EventArgs e)
        {
            //update marker position from the video controller
            //something like this mc.VideoControl.CalculateMarkerPosition(TimeToMoveMarkerTo, pageSizeTime)
            
            int position = (int) Math.Round((this.videoMedia.Position.Seconds / this.videoDuration) * this.audioCanvasWidth);
            double initialPos = Canvas.GetLeft(this.marker);
            double newPos = initialPos + 1;
            Canvas.SetLeft(this.marker, newPos);

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
            this.audioCanvasHeight = this.audioCanvasBorder.ActualHeight;
            this.audioCanvasWidth = this.audioCanvasBorder.ActualWidth;
            //  Console.WriteLine("WIDTH: " + this.audioCanvasWidth);
            //  Console.WriteLine("HEIGHT: " + this.audioCanvasHeight);
            setPaging();

            //the 4th point is the bottom point of the marker, adjusting the y value of this point
            this.marker.Points[4] = new Point(this.marker.Points[4].X, this.audioCanvasHeight);
        }

        #region Helper Functions

        /// <summary>
        /// 
        /// </summary>
        private void setPaging()
        {
            double pages = 30.093 / this.pageTime;
            double width;
            width = this.audioCanvasBorder.ActualWidth * pages;
            //Console.WriteLine("Width: " + this.audioCanvasBorder.ActualWidth);
            this.audioCanvas.Width = width;
            
        }


        #endregion
       
    }
}

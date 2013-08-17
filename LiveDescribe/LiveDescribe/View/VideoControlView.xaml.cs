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
using LiveDescribe.Controls;
using LiveDescribe.Interfaces;
using System.Windows.Threading;
using LiveDescribe.View_Model;

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for VideoControlView.xaml
    /// </summary>
    public partial class VideoControlView : UserControl
    {

       
        private readonly DispatcherTimer _videoTimer;
        private double _videoDuration;


        public VideoControlView()
        {
            InitializeComponent();
            VideoControl videoControl = new VideoControl(VideoMedia);
            DataContext = videoControl;

            _videoTimer = new DispatcherTimer();
           // _videoTimer.Tick += Play_Tick;
          //  _videoTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            #region Event Listeners
            //listens for PlayRequested Event
            videoControl.PlayRequested += (sender, e) =>
            {
            //    _videoTimer.Start();
                //  this.storyBoard.Begin(this);
                VideoMedia.Play();
            };

            //listens for PauseRequested Event
            videoControl.PauseRequested += (sender, e) =>
            {
                VideoMedia.Pause();
              //  _videoTimer.Stop();

            };

            //listens for MuteRequested Event
            videoControl.MuteRequested += (sender, e) =>
            {
                VideoMedia.IsMuted = !VideoMedia.IsMuted;
            };

            videoControl.VideoOpenedRequested += (sender, e) =>
            {
                _videoDuration = VideoMedia.NaturalDuration.TimeSpan.TotalSeconds;
               // Console.WriteLine("DURATION: " + _videoDuration);
            }; 
            #endregion
        }
    }
}

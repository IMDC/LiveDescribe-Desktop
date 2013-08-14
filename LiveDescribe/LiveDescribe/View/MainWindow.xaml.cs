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
        private Marker marker;
        private double audioCanvasHeight;
        private double audioCanvasWidth;

        public MainWindow()
        {
            InitializeComponent();

            VideoControl vc = new VideoControl(this.videoMedia);
            marker = new Marker(audioCanvas);
            DataContext = vc;

            #region Event Listeners
            //listens for PlayRequested Event
            vc.PlayRequested += (sender, e) =>
                {
                    this.videoMedia.Play();
                };

            //listens for PauseRequested Event
            vc.PauseRequested += (sender, e) =>
                {
                    this.videoMedia.Pause();
                };

            //listens for MuteRequested Event
            vc.MuteRequested += (sender, e) =>
            {
                this.videoMedia.IsMuted = !this.videoMedia.IsMuted;
            };
            #endregion
        }

        /// <summary>
        /// Updates the canvasWidth and canvasHeight variables everytime the canvas size is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvasBorder_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            this.audioCanvasHeight = this.audioCanvasBorder.ActualHeight;
            this.audioCanvasWidth = this.audioCanvasBorder.ActualWidth;
            marker.draw(50, this.audioCanvasHeight, this.audioCanvasWidth);
        }
    }
}

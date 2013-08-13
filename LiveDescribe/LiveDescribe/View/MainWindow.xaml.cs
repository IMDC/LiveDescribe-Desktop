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

namespace LiveDescribe.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            VideoControl vc = new VideoControl();
            DataContext = vc;

            #region Event Listeners
            //listens for PlayRequested Event
            vc.PlayRequested += (sender, e) =>
                {
                    this.playControl.IsEnabled = false;
                    this.pauseControl.IsEnabled = true;
                    this.videoMedia.Play();
                };

            //listens for PauseRequested Event
            vc.PauseRequested += (sender, e) =>
                {
                    this.pauseControl.IsEnabled = false;
                    this.playControl.IsEnabled = true;
                    this.videoMedia.Pause();
                };

            //listens for MuteRequested Event
            vc.MuteRequested += (sender, e) =>
            {
                this.videoMedia.IsMuted = !this.videoMedia.IsMuted;
            };
            #endregion
        }


    }
}

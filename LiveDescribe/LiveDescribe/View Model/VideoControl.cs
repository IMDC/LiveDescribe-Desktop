using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Model;
using Microsoft.TeamFoundation.MVVM;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LiveDescribe.View_Model
{
    class VideoControl
    {
        #region Instance Variables
        private Video _video;
        private MediaElement _videoMedia;
        private DispatcherTimer _videoTimer;
        #endregion

        #region Event Handlers
        public event EventHandler PlayRequested;
        public event EventHandler PauseRequested;
        public event EventHandler MuteRequested;
        #endregion

        #region Constructors
        public VideoControl(MediaElement videoMedia)
        { 
            _video = new Video(@"C:\Users\Public\Videos\Sample Videos\Wildlife.wmv");
            _video.CurrentState = Video.States.Paused;

            _videoTimer = new DispatcherTimer();
            _videoTimer.Tick += new EventHandler(Play_Tick);
            _videoTimer.Interval = new TimeSpan(0, 0, 1);

             PlayCommand = new RelayCommand(Play, PlayCheck);
             PauseCommand = new RelayCommand(Pause, PauseCheck);
             MuteCommand = new RelayCommand(Mute, (object param) => true);

             this._videoMedia = videoMedia;

             if (this._videoMedia.NaturalDuration.HasTimeSpan)
                this._video.Duration = this._videoMedia.NaturalDuration.TimeSpan.TotalSeconds;
        }
        #endregion

        #region RelayCommands
        /// <summary>
        /// Setter and Getter for PlayCommand
        /// </summary>
        public RelayCommand PlayCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Setter and Getter for PauseCommand
        /// </summary>
        public RelayCommand PauseCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Setter and Getter for MuteCommand
        /// </summary>
        public RelayCommand MuteCommand
        {
            get;
            private set;
        }
        #endregion

        #region Video Controls

        /// <summary>
        /// EventHandler for the _videoTimer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Play_Tick(object sender, EventArgs e)
        {

            this._video.CurrentTime = this._videoMedia.Position.Seconds;

            Console.WriteLine("Current Time: " + _video.CurrentTime);
        }

        /// <summary>
        /// Plays the video
        /// </summary>
        /// <param name="param">params</param>
        public void Play(object param)
        {
            Console.WriteLine("Play");

            EventHandler handler = PlayRequested;

            if (handler != null)
            {
                _video.CurrentState = Video.States.Playing;
                _videoTimer.Start();
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Pauses the video
        /// </summary>
        /// <param name="param">params</param>
        public void Pause(object param)
        {
            Console.WriteLine("Pause");

            EventHandler handler = PauseRequested;

            if (handler != null)
            {
                _video.CurrentState = Video.States.Paused;
                _videoTimer.Stop();
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Fastforwards the video
        /// </summary>
        public void FastForward()
        { 
        }

        /// <summary>
        /// Rewinds the video
        /// </summary>
        public void Rewind()
        { 
        }

        /// <summary>
        /// Records user audio
        /// </summary>
        public void Record()
        {
            _video.CurrentState = Video.States.Recording;
        }

        /// <summary>
        /// Mutes the video's audio
        /// </summary>
        /// <param name="param">params</param>
        public void Mute(object param)
        {
            Console.WriteLine("Mute");

            EventHandler handler = MuteRequested;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
        #endregion

        #region State Checks
        /// <summary>
        /// Used for the RelayCommand "PlayCommand" to check whether the play button can be pressed or not
        /// </summary>
        /// <param name="param">param</param>
        /// <returns>true if button can be enabled</returns>
        public bool PlayCheck(object param)
        {
            if (_video.CurrentState == Video.States.Playing || _video.CurrentState == Video.States.Recording)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Used for the RelayCommand "PauseCommand" to check whether the pause button can be presse or not
        /// </summary>
        /// <param name="param">param</param>
        /// <returns>true if button can be enabled</returns>
        public bool PauseCheck(object param)
        {
            if (_video.CurrentState == Video.States.Paused)
                return false;
            else
                return true;
        }
        #endregion

    }

    /// <summary>
    /// Custom EventArgs class to send the new position of the marker
    /// </summary>
    public class PositionUpdateArgs : EventArgs
    {
        private double m_X;
        public PositionUpdateArgs(double X)
        {
            this.m_X = X;
        }

        public double X { get { return m_X; } }
    }
}

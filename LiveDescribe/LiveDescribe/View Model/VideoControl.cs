using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Model;
using Microsoft.TeamFoundation.MVVM;
using System.Windows.Media;

namespace LiveDescribe.View_Model
{
    class VideoControl
    {
        #region Instance Variables
        private Video _video;
        #endregion

        #region Event Handlers
        public event EventHandler PlayRequested;
        public event EventHandler PauseRequested;
        public event EventHandler MuteRequested;
        #endregion

        #region Constructors
        public VideoControl()
        { 
            _video = new Video(@"C:\Users\Public\Videos\Sample Videos\Wildlife.wmv");
             PlayCommand = new RelayCommand(Play, (object param) => true);
             PauseCommand = new RelayCommand(Pause, (object param) => true);
             MuteCommand = new RelayCommand(Mute, (object param) => true);
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
        /// Plays the video
        /// </summary>
        /// <param name="param">params</param>
        public void Play(object param)
        {
            Console.WriteLine("Play");

            EventHandler handler = PlayRequested;

            if (handler != null)
            {
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
    }
}

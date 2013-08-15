using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Model;
using Microsoft.TeamFoundation.MVVM;
using System.ComponentModel;
using Microsoft.Win32;

namespace LiveDescribe.View_Model
{
    class VideoControl : ViewModelBase, INotifyPropertyChanged
    {
        #region Instance Variables
        private Video _video;
       // private MediaElement _videoMedia;
      //  private DispatcherTimer _videoTimer;
        #endregion

        #region Event Handlers
        public event EventHandler PlayRequested;
        public event EventHandler PauseRequested;
        public event EventHandler MuteRequested;
        #endregion

        #region Constructors
        public VideoControl()
        {
            _video = new Video();
             PlayCommand = new RelayCommand(Play, PlayCheck);
             PauseCommand = new RelayCommand(Pause, PauseCheck);
             MuteCommand = new RelayCommand(Mute, (object param) => true);
             FastForwardCommand = new RelayCommand(FastForward, FastForwardCheck);
             RewindCommand = new RelayCommand(Rewind, RewindCheck);
             RecordCommand = new RelayCommand(Record, RecordCheck);

             ImportVideoCommand = new RelayCommand(OpenVideo, ImportCheck);
             this.AddDependencySource("Path", this);
        }
        #endregion

        #region Commands
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

        public RelayCommand ImportVideoCommand
        {
            get;
            private set;
        }

        public RelayCommand FastForwardCommand
        {
            get;
            private set;
        }

        public RelayCommand RewindCommand
        {
            get;
            private set;
        }

        public RelayCommand RecordCommand
        {
            get;
            private set;
        }
        #endregion

        #region Binding Functions

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
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Fastforwards the video
        /// </summary>
        public void FastForward(object param)
        { 
        }

        /// <summary>
        /// Rewinds the video
        /// </summary>
        public void Rewind(object param)
        { 
        }

        /// <summary>
        /// Records user audio
        /// </summary>
        public void Record(object param)
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

        /// <summary>
        /// Open a dialog box for the user to choose what file they want to open
        /// </summary>
        /// <param name="param"></param>
        public void OpenVideo(object param)
        {
            OpenFileDialog dialogBox = new OpenFileDialog();
            bool? userClickedOk = dialogBox.ShowDialog();

            if (userClickedOk == true)
            {
                Path = dialogBox.FileName;
            }

            _video.CurrentState = Video.States.Paused;
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
            if (_video.CurrentState == Video.States.Playing || _video.CurrentState == Video.States.Recording || _video.CurrentState == Video.States.NotLoaded)
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
            if (_video.CurrentState == Video.States.Paused || _video.CurrentState == Video.States.NotLoaded)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Used for the RelayCommand "RewindCommand" to check whether the rewind button can be pressed or not
        /// </summary>
        /// <param name="param">param</param>
        /// <returns>true if the button can be enabled</returns>
        public bool RewindCheck(object param)
        {
            if (_video.CurrentState == Video.States.NotLoaded)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Used for the RelayCommand "FastForwardCommand" to check whether the fastforward button can be pressed or not
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool FastForwardCheck(object param)
        {
            if (_video.CurrentState == Video.States.NotLoaded)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Used for the RelayCommand "RecordCommand" to check whether the record button can be pressed or not
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool RecordCheck(object param)
        {
            if (_video.CurrentState == Video.States.NotLoaded)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Used for the RelayCommand "ImportCommand" to check whether importing will work or not
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool ImportCheck(object param)
        {
            if (_video.CurrentState != Video.States.NotLoaded)
                return false;
            else
                return true;
        }
        #endregion

        #region Binding Properties
        /// <summary>
        /// Path set in the "MediaElement" source property
        /// </summary>
        public string Path
        {
            set
            {
                this._video.Path = value;
                RaisePropertyChanged("Path");
            }
            get
            {
                return _video.Path;
            }

        }

        #endregion

    }
}

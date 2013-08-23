using System;
using System.ComponentModel;
using LiveDescribe.Interfaces;
using Microsoft.TeamFoundation.MVVM;
using Microsoft.Win32;
using LiveDescribe.Utilities;
using System.Collections.Generic;

namespace LiveDescribe.View_Model
{
    class VideoControl : ViewModelBase
    {
        #region Instance Variables
        private readonly ILiveDescribePlayer _mediaVideo;
        private AudioUtility _audioOperator;
        private List<float> _waveFormData;
        private bool _busyStrippingAudio;
        private readonly BackgroundWorker _stripAudioWorker;
        #endregion

        #region Event Handlers
        public event EventHandler PlayRequested;
        public event EventHandler PauseRequested;
        public event EventHandler MuteRequested;
        public event EventHandler VideoOpenedRequested;
        public event EventHandler MediaFailedEvent;
        public event EventHandler OnStrippingAudioCompleted;
        #endregion

        #region Constructors
        public VideoControl(ILiveDescribePlayer mediaVideo)
        {
           // _video = new Video();
            _mediaVideo = mediaVideo;
            
            PlayCommand = new RelayCommand(Play, PlayCheck);
            PauseCommand = new RelayCommand(Pause, PauseCheck);
            MuteCommand = new RelayCommand(Mute, param => true);
            FastForwardCommand = new RelayCommand(FastForward, FastForwardCheck);
            RewindCommand = new RelayCommand(Rewind, RewindCheck);
            RecordCommand = new RelayCommand(Record, RecordCheck);

            //bound to when the video loads and is opened via the mediaelement
            VideoOpenedCommand = new RelayCommand(VideoOpen, param => true);
            MediaFailedCommand = new RelayCommand(MediaFailed, param => true);
            //bound to Menu->file->Import Video
            ImportVideoCommand = new RelayCommand(ImportVideo, ImportCheck);
            IsBusyStrippingAudio = false;
            _stripAudioWorker = new BackgroundWorker();
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

        public RelayCommand VideoOpenedCommand
        {
            get;
            private set;
        }

        public RelayCommand MediaFailedCommand
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
            _mediaVideo.CurrentState = LiveDescribeStates.PlayingVideo;
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
            _mediaVideo.CurrentState = LiveDescribeStates.PausedVideo;
            if (handler != null)
            {
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
        /// This event is thrown when the video is opened or "loaded"
        /// </summary>
        /// <param name="param"></param>
        public void VideoOpen(object param)
        {
            EventHandler handler = VideoOpenedRequested;
            Console.WriteLine("LOADED");
            _mediaVideo.CurrentState = LiveDescribeStates.VideoLoaded;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }


        /// <summary>
        /// Records user audio
        /// </summary>
        public void Record(object param)
        {
            _mediaVideo.CurrentState = LiveDescribeStates.RecordingDescription;
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
        /// What happens when Import Video option is select, in this case open up a file dialogbox get the file then strip the audio
        /// </summary>
        /// <param name="param"></param>
        public void ImportVideo(object param)
        {
            var dialogBox = new OpenFileDialog();
            bool? userClickedOk = dialogBox.ShowDialog();

            Console.WriteLine("OPENVIDEO");
            if (userClickedOk == true)
            {
                Path = dialogBox.FileName;
                //create a new background worker to strip the audio and set BusyStrippingAudio to true
                //it does not get set to false in this class because the view is meant to take care of what they want to do with the stripped audio when it is completed for example
                //create a wave form
                //the variable IsBusyStrippingAudio gets binded to the view (a loading screen visibility to be exact) and when set to false will get rid of the loading screen
                _stripAudioWorker.DoWork += StripAudio;
                _stripAudioWorker.RunWorkerCompleted += OnFinishedStrippingAudio;
                IsBusyStrippingAudio = true;
                _stripAudioWorker.RunWorkerAsync();
            }

            _mediaVideo.CurrentState = LiveDescribeStates.PausedVideo;
        }

        /// <summary>
        /// throws an event when the media fails to load an example of this happening would be if the
        /// video format is not supported
        /// </summary>
        /// <param name="param"></param>
        public void MediaFailed(object param)
        {
            EventHandler handler = MediaFailedEvent;
            Console.WriteLine("Media Failed to load...");
            _mediaVideo.CurrentState = LiveDescribeStates.VideoNotLoaded;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        #endregion

        #region Background Workers
        /// <summary>
        /// this method works in the background to strip the audio from the current video
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StripAudio(object sender, DoWorkEventArgs e)
        {
            _audioOperator = new AudioUtility(_mediaVideo.Path);
            _audioOperator.stripAudio();
            _waveFormData = _audioOperator.readWavData();           
        }

        /// <summary>
        /// Gets called when the background worker is finished stripping the audio
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnFinishedStrippingAudio(object sender, RunWorkerCompletedEventArgs e)
        {
            EventHandler handler = OnStrippingAudioCompleted;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
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
            if (_mediaVideo.CurrentState == LiveDescribeStates.PlayingVideo || _mediaVideo.CurrentState == LiveDescribeStates.RecordingDescription ||
                _mediaVideo.CurrentState == LiveDescribeStates.VideoNotLoaded)
                return false;
            return true;
        }

        /// <summary>
        /// Used for the RelayCommand "PauseCommand" to check whether the pause button can be presse or not
        /// </summary>
        /// <param name="param">param</param>
        /// <returns>true if button can be enabled</returns>
        public bool PauseCheck(object param)
        {
            if (_mediaVideo.CurrentState == LiveDescribeStates.PausedVideo || _mediaVideo.CurrentState == LiveDescribeStates.VideoNotLoaded
                || _mediaVideo.CurrentState == LiveDescribeStates.VideoLoaded)
                return false;
            return true;
        }

        /// <summary>
        /// Used for the RelayCommand "RewindCommand" to check whether the rewind button can be pressed or not
        /// </summary>
        /// <param name="param">param</param>
        /// <returns>true if the button can be enabled</returns>
        public bool RewindCheck(object param)
        {
            if (_mediaVideo.CurrentState == LiveDescribeStates.VideoNotLoaded)
                return false;
            return true;
        }

        /// <summary>
        /// Used for the RelayCommand "FastForwardCommand" to check whether the fastforward button can be pressed or not
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool FastForwardCheck(object param)
        {
            if (_mediaVideo.CurrentState == LiveDescribeStates.VideoNotLoaded)
                return false;
            return true;
        }

        /// <summary>
        /// Used for the RelayCommand "RecordCommand" to check whether the record button can be pressed or not
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool RecordCheck(object param)
        {
            if (_mediaVideo.CurrentState == LiveDescribeStates.VideoNotLoaded)
                return false;
            return true;
        }

        /// <summary>
        /// Used for the RelayCommand "ImportCommand" to check whether importing will work or not
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool ImportCheck(object param)
        {
            if (_mediaVideo.CurrentState != LiveDescribeStates.VideoNotLoaded)
                return false;
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
                _mediaVideo.Path = value;
                RaisePropertyChanged("Path");
            }
            get
            {
                return _mediaVideo.Path;
            }

        }

        public bool IsBusyStrippingAudio
        {
            set
            {
                _busyStrippingAudio = value;
                RaisePropertyChanged("IsBusyStrippingAudio");
            }
            get
            {
                return _busyStrippingAudio;
            }
        }
        #endregion

        #region setters / getters
        /// <summary>
        /// Get the wavform Data
        /// </summary>
        public List<float> AudioData
        {
            set{}
            get { return this._waveFormData; }
        }
        #endregion
    }
}

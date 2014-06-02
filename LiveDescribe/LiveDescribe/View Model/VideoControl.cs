using System;
using System.ComponentModel;
using LiveDescribe.Graphics;
using LiveDescribe.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Model;
using Microsoft.Win32;
using LiveDescribe.Utilities;
using System.Collections.Generic;

namespace LiveDescribe.View_Model
{
    public class VideoControl : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constants
        /// <summary>
        /// How much the volume gets reduced to when a description plays
        /// </summary>
        public const double VolumeReductionFactor = 0.2;
        #endregion

        #region Instance Variables
        private readonly ILiveDescribePlayer _mediaVideo;
        private AudioUtility _audioOperator;
        private LoadingViewModel _loadingViewModel;
        private List<Space> _spaceData;
        private TimeSpan _positionTimeLabel;
        private double _originalVolume;
        private Waveform _waveform;

        public Project Project { get; set; }
        #endregion

        #region Event Handlers
        public event EventHandler PlayRequested;
        public event EventHandler PauseRequested;
        public event EventHandler MuteRequested;
        public event EventHandler VideoOpenedRequested;
        public event EventHandler MediaFailedEvent;
        public event EventHandler MediaEndedEvent;
        public event EventHandler OnStrippingAudioCompleted;

        //Event handlers for the Marker on the timeline
        public event EventHandler OnMarkerMouseDownRequested;
        public event EventHandler OnMarkerMouseUpRequested;
        public event EventHandler OnMarkerMouseMoveRequested;
        #endregion

        #region Constructors
        public VideoControl(ILiveDescribePlayer mediaVideo, LoadingViewModel loadingViewModel)
        {
            _mediaVideo = mediaVideo;
            _loadingViewModel = loadingViewModel;
            PlayCommand = new RelayCommand(Play, PlayCheck);
            PauseCommand = new RelayCommand(Pause, PauseCheck);
            MuteCommand = new RelayCommand(Mute, () => true);
            FastForwardCommand = new RelayCommand(FastForward, FastForwardCheck);
            RewindCommand = new RelayCommand(Rewind, RewindCheck);

            //Marker commands {
            MarkerMouseDownCommand = new RelayCommand(OnMarkerMouseDown, () => true);
            MarkerMouseUpCommand = new RelayCommand(OnMarkerMouseUp, () => true);
            MarkerMouseMoveCommand = new RelayCommand(OnMarkerMouseMove, () => true);
            //}

            //bound to when the video loads and is opened via the mediaelement
            VideoOpenedCommand = new RelayCommand(VideoOpen, () => true);
            MediaFailedCommand = new RelayCommand(MediaFailed, () => true);

            MediaEndedCommand = new RelayCommand(MediaEnded, () => true);
        }
        #endregion

        #region Commands
        /// <summary>
        /// Setter and getter for MediaEndedCommand
        /// </summary>
        public RelayCommand MediaEndedCommand { get; private set; }

        /// <summary>
        /// Setter and getter for MarkerMouseMoveCommand
        /// </summary>
        public RelayCommand MarkerMouseMoveCommand { get; private set; }

        /// <summary>
        /// Setter and getter for MarkerMouseUpCommand
        /// </summary>
        public RelayCommand MarkerMouseUpCommand { get; private set; }

        /// <summary>
        /// Setter and getter for MarkerMouseDownCommand
        /// </summary>
        public RelayCommand MarkerMouseDownCommand { get; private set; }

        /// <summary>
        /// Setter and Getter for PlayCommand
        /// </summary>
        public RelayCommand PlayCommand  { get; private set; }

        /// <summary>
        /// Setter and Getter for PauseCommand
        /// </summary>
        public RelayCommand PauseCommand  { get; private set; }

        /// <summary>
        /// Setter and Getter for MuteCommand
        /// </summary>
        public RelayCommand MuteCommand { get; private set; }

        public RelayCommand FastForwardCommand  { get; private set; }

        public RelayCommand RewindCommand  { get; private set; }

        public RelayCommand RecordCommand  { get; private set; }

        public RelayCommand VideoOpenedCommand  { get; private set; }

        public RelayCommand MediaFailedCommand  { get; private set; }
        #endregion

        #region Binding Functions

        /// <summary>
        /// what happens when the marker is moved using the mouse
        /// </summary>
        /// <param name="param"></param>
        public void OnMarkerMouseMove()
        {
            EventHandler handler = OnMarkerMouseMoveRequested;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// What happens when the mouse is released on the marker
        /// </summary>
        /// <param name="param"></param>
        public void OnMarkerMouseUp()
        {
            EventHandler handler = OnMarkerMouseUpRequested;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// What hapens when the marker is pressed down
        /// </summary>
        /// <param name="param"></param>
        public void OnMarkerMouseDown()
        {
            this.PauseCommand.Execute(this);
            EventHandler handler = OnMarkerMouseDownRequested;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }
        /// <summary>
        /// Plays the video
        /// </summary>
        /// <param name="param">params</param>
        public void Play()
        {
            log.Info("Play video");

            EventHandler handler = PlayRequested;
            _mediaVideo.CurrentState = LiveDescribeVideoStates.PlayingVideo;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Pauses the video
        /// </summary>
        /// <param name="param">params</param>
        public void Pause()
        {
            log.Info("Pause video");

            EventHandler handler = PauseRequested;
            _mediaVideo.CurrentState = LiveDescribeVideoStates.PausedVideo;

            if (handler == null) return;
            handler(this, EventArgs.Empty);

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
        /// This event is thrown when the video is opened or "loaded"
        /// </summary>
        /// <param name="param"></param>
        public void VideoOpen()
        {
            EventHandler handler = VideoOpenedRequested;
            log.Info("Video loaded");
            _mediaVideo.CurrentState = LiveDescribeVideoStates.VideoLoaded;

            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Mutes the video's audio
        /// </summary>
        /// <param name="param">params</param>
        public void Mute()
        {
            log.Info("Video muted");

            EventHandler handler = MuteRequested;

            if (handler == null) return;
            handler(this, EventArgs.Empty);

        }

        /// <summary>
        /// throws an event when the media fails to load an example of this happening would be if the
        /// video format is not supported
        /// </summary>
        /// <param name="param"></param>
        public void MediaFailed()
        {
            EventHandler handler = MediaFailedEvent;
            log.Warn("Media Failed to load...");
            _mediaVideo.CurrentState = LiveDescribeVideoStates.VideoNotLoaded;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// thros an event when the media is finished
        /// </summary>
        /// <param name="param"></param>
        public void MediaEnded()
        {
            EventHandler handler = MediaEndedEvent;
            log.Info("Video has ended");
            //Changing state back to video loaded because it is starting from the beginning
            _mediaVideo.CurrentState = LiveDescribeVideoStates.VideoLoaded;
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
        public bool PlayCheck()
        {
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.PlayingVideo || _mediaVideo.CurrentState == LiveDescribeVideoStates.RecordingDescription ||
                _mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return false;
            return true;
        }

        /// <summary>
        /// Used for the RelayCommand "PauseCommand" to check whether the pause button can be presse or not
        /// </summary>
        /// <param name="param">param</param>
        /// <returns>true if button can be enabled</returns>
        public bool PauseCheck()
        {
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.PausedVideo || _mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded
                || _mediaVideo.CurrentState == LiveDescribeVideoStates.VideoLoaded || _mediaVideo.CurrentState == LiveDescribeVideoStates.RecordingDescription)
                return false;
            return true;
        }

        /// <summary>
        /// Used for the RelayCommand "RewindCommand" to check whether the rewind button can be pressed or not
        /// </summary>
        /// <param name="param">param</param>
        /// <returns>true if the button can be enabled</returns>
        public bool RewindCheck()
        {
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded || _mediaVideo.CurrentState == LiveDescribeVideoStates.RecordingDescription)
                return false;
            return true;
        }

        /// <summary>
        /// Used for the RelayCommand "FastForwardCommand" to check whether the fastforward button can be pressed or not
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool FastForwardCheck()
        {
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded || _mediaVideo.CurrentState == LiveDescribeVideoStates.RecordingDescription)
                return false;
            return true;
        }

        /// <summary>
        /// Used for the RelayCommand "RecordCommand" to check whether the record button can be pressed or not
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool RecordCheck()
        {
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return false;
            return true;
        }
        #endregion

        #region Binding Properties
        /// <summary>
        /// Bound to the LiveDescribeMediaPlayer (_mediaVideo) Source property
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

        /// <summary>
        /// Position in the video that gets shown to the user
        /// </summary>
        public TimeSpan PositionTimeLabel
        {
            set
            {
                _positionTimeLabel = value;
                RaisePropertyChanged("PositionTimeLabel");
            }
            get
            {
                return _positionTimeLabel;
            }
        }
        #endregion

        #region Accessors

        //Contains data relevant to a waveform.
        public Waveform Waveform
        {
            set
            {
                _waveform = value;
                RaisePropertyChanged("Waveform");
            }
            get { return _waveform; }
        }

        /// <summary>
        /// Get the space data 
        /// </summary>
        public List<Space> Spaces
        {
            get { return this._spaceData; }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// This function is to close the video control, it is called by the main control
        /// </summary>
        public void CloseVideoControl()
        {
            _audioOperator = null;
            _waveform = null;
            _mediaVideo.Path = null;
            _mediaVideo.Stop();
            _mediaVideo.Close();
            _mediaVideo.CurrentState = LiveDescribeVideoStates.VideoNotLoaded;
        }

        /// <summary>
        /// This function is used to setup all the events for the background worker and to run it
        /// to strip the audio from the video
        /// </summary>
        /// <param name="path"></param>
        public void SetupAndStripAudio(Project p)
        {
            //changes the Path variable that is binded to the media element
            Path = p.Files.Video;
            Project = p;

            var worker = new BackgroundWorker { WorkerReportsProgress = true, };

            //Strip the audio from the given project video
            worker.DoWork += (sender, args) =>
            {
                _audioOperator = new AudioUtility(Project);
                _audioOperator.StripAudio(worker);
                var waveFormData = _audioOperator.ReadWavData(worker);
                var audioHeader = _audioOperator.Header;
                _waveform = new Waveform(audioHeader, waveFormData);
                _spaceData = AudioAnalyzer.FindSpaces(_waveform);
            };

            //Notify subscribers of stripping completion
            worker.RunWorkerCompleted += (sender, args) =>
            {
                EventHandler handler = OnStrippingAudioCompleted;
                if (handler == null) return;
                handler(this, EventArgs.Empty);
            };

            worker.ProgressChanged += (sender, args) =>
            {
                _loadingViewModel.SetProgress("Importing Video", args.ProgressPercentage);
            };

            _loadingViewModel.SetProgress("Importing Video", 0);
            _loadingViewModel.Visible = true;
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Reduces the volume to VolumeReductionFactor % of the original volume.
        /// </summary>
        public void ReduceVolume()
        {
            _originalVolume = _mediaVideo.Volume;
            _mediaVideo.Volume *= VolumeReductionFactor;
        }

        /// <summary>
        /// Restores the volume to its original volume level.
        /// </summary>
        public void RestoreVolume()
        {
            _mediaVideo.Volume = _originalVolume;
        }
        #endregion
    }
}

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Extensions;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Model;
using System;

namespace LiveDescribe.ViewModel
{
    public class MediaControlViewModel : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
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
        //private List<Space> _spaceData;
        private TimeSpan _positionTimeLabel;
        private double _originalVolume;
        private Waveform _waveform;
        private RelayCommand _playPauseButtonClickCommand;

        //public Project Project { get; set; }
        #endregion

        #region Event Handlers
        public event EventHandler PlayRequested;
        public event EventHandler PauseRequested;
        public event EventHandler MuteRequested;
        public event EventHandler VideoOpenedRequested;
        public event EventHandler MediaFailedEvent;
        public event EventHandler MediaEndedEvent;
        public event EventHandler OnStrippingAudioCompleted;
        public event EventHandler OnPausedForExtendedDescription;

        //Event handlers for the Marker on the timeline
        public event EventHandler OnMarkerMouseDownRequested;
        public event EventHandler OnMarkerMouseUpRequested;
        public event EventHandler OnMarkerMouseMoveRequested;
        #endregion

        #region Constructors
        public MediaControlViewModel(ILiveDescribePlayer mediaVideo)
        {
            _mediaVideo = mediaVideo;
            PlayCommand = new RelayCommand(Play, PlayCheck);
            PauseCommand = new RelayCommand(Pause, PauseCheck);
            MuteCommand = new RelayCommand(Mute, () => true);
            PauseForExtendedDescriptionCommand = new RelayCommand(PauseForExtendedDescription, () => true);

            //Marker commands {
            MarkerMouseDownCommand = new RelayCommand(OnMarkerMouseDown, () => true);
            MarkerMouseUpCommand = new RelayCommand(OnMarkerMouseUp, () => true);
            MarkerMouseMoveCommand = new RelayCommand(OnMarkerMouseMove, () => true);
            //}

            //bound to when the video loads and is opened via the mediaelement
            VideoOpenedCommand = new RelayCommand(VideoOpen, () => true);
            MediaFailedCommand = new RelayCommand(MediaFailed, () => true);

            MediaEndedCommand = new RelayCommand(MediaEnded, () => true);
            _mediaVideo.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName.Equals("CurrentState"))
                {
                    VideoState = _mediaVideo.CurrentState;
                    RaisePropertyChanged("VideoState");
                }
            };

            ProjectManager.Instance.ProjectLoaded += (sender, args) =>
            {
                Waveform = args.Value.Waveform;
            };
        }
        #endregion

        #region Commands

        public RelayCommand PauseForExtendedDescriptionCommand { get; private set; }

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
        public RelayCommand PlayCommand { get; set; }

        /// <summary>
        /// Setter and Getter for PauseCommand
        /// </summary>
        public RelayCommand PauseCommand { get; set; }

        /// <summary>
        /// Setter and Getter for MuteCommand
        /// </summary>
        public RelayCommand MuteCommand { get; private set; }

        public RelayCommand VideoOpenedCommand { get; private set; }

        public RelayCommand MediaFailedCommand { get; private set; }

        public RelayCommand PlayPauseButtonClickCommand
        {
            get { return _playPauseButtonClickCommand ?? (_playPauseButtonClickCommand = PlayCommand); }
            set
            {
                _playPauseButtonClickCommand = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Binding Functions

        /// <summary>
        /// what happens when the marker is moved using the mouse
        /// </summary>
        private void OnMarkerMouseMove()
        {
            EventHandler handler = OnMarkerMouseMoveRequested;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// What happens when the mouse is released on the marker
        /// </summary>
        private void OnMarkerMouseUp()
        {
            EventHandler handler = OnMarkerMouseUpRequested;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// What hapens when the marker is pressed down
        /// </summary>
        private void OnMarkerMouseDown()
        {
            PauseCommand.Execute();
            EventHandler handler = OnMarkerMouseDownRequested;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }
        /// <summary>
        /// Plays the video
        /// </summary>
        private void Play()
        {
            Log.Info("Play video");

            EventHandler handler = PlayRequested;
            _mediaVideo.CurrentState = LiveDescribeVideoStates.PlayingVideo;

            if (handler != null) handler(this, EventArgs.Empty);
            PlayPauseButtonClickCommand = PauseCommand;
        }

        /// <summary>
        /// Pauses the video
        /// </summary>
        private void Pause()
        {
            Log.Info("Pause video");
            EventHandler handler = PauseRequested;
            _mediaVideo.CurrentState = LiveDescribeVideoStates.PausedVideo;

            if (handler != null) handler(this, EventArgs.Empty);
            PlayPauseButtonClickCommand = PlayCommand;
        }

        /// <summary>
        /// This event is thrown when the video is opened or "loaded"
        /// </summary>
        private void VideoOpen()
        {
            EventHandler handler = VideoOpenedRequested;
            Log.Info("Video loaded");
            _mediaVideo.CurrentState = LiveDescribeVideoStates.VideoLoaded;

            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Mutes the video's audio
        /// </summary>
        private void Mute()
        {
            Log.Info(_mediaVideo.IsMuted ? "Video UnMuted" : "Video Muted");

            _mediaVideo.IsMuted = !_mediaVideo.IsMuted;
            EventHandler handler = MuteRequested;

            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// throws an event when the media fails to load an example of this happening would be if
        /// the video format is not supported
        /// </summary>
        private void MediaFailed()
        {
            EventHandler handler = MediaFailedEvent;
            Log.Warn("Media Failed to load...");
            _mediaVideo.CurrentState = LiveDescribeVideoStates.VideoNotLoaded;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// thros an event when the media is finished
        /// </summary>
        private void MediaEnded()
        {
            Log.Info("Video has ended");
            //Changing state back to video loaded because it is starting from the beginning
            _mediaVideo.CurrentState = LiveDescribeVideoStates.VideoLoaded;
            EventHandler handler = MediaEndedEvent;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
            PlayPauseButtonClickCommand = PlayCommand;
        }

        private void PauseForExtendedDescription()
        {
            _mediaVideo.CurrentState = LiveDescribeVideoStates.PlayingExtendedDescription;
            EventHandler handler = OnPausedForExtendedDescription;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }
        #endregion

        #region State Checks
        /// <summary>
        /// Used for the RelayCommand "PlayCommand" to check whether the play button can be pressed
        /// or not
        /// </summary>
        /// <returns>true if button can be enabled</returns>
        public bool PlayCheck()
        {
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.PlayingVideo || _mediaVideo.CurrentState == LiveDescribeVideoStates.RecordingDescription ||
                _mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return false;
            return true;
        }

        /// <summary>
        /// Used for the RelayCommand "PauseCommand" to check whether the pause button can be presse
        /// or not
        /// </summary>
        /// <returns>true if button can be enabled</returns>
        public bool PauseCheck()
        {
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.PausedVideo || _mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded
                || _mediaVideo.CurrentState == LiveDescribeVideoStates.VideoLoaded || _mediaVideo.CurrentState == LiveDescribeVideoStates.RecordingDescription
                || _mediaVideo.CurrentState == LiveDescribeVideoStates.PlayingExtendedDescription)
                return false;
            return true;
        }

        /// <summary>
        /// Used for the RelayCommand "RecordCommand" to check whether the record button can be
        /// pressed or not
        /// </summary>
        /// <returns></returns>
        public bool RecordCheck()
        {
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return false;
            return true;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Bound to the LiveDescribeMediaPlayer (_mediaVideo) Source property
        /// </summary>
        public string Path
        {
            set
            {
                _mediaVideo.Path = value;
                RaisePropertyChanged();
            }
            get { return _mediaVideo.Path; }
        }

        /// <summary>
        /// Position in the video that gets shown to the user
        /// </summary>
        public TimeSpan PositionTimeLabel
        {
            set
            {
                _positionTimeLabel = value;
                RaisePropertyChanged();
            }
            get { return _positionTimeLabel; }
        }

        public LiveDescribeVideoStates VideoState { get; set; }

        public ILiveDescribePlayer MediaVideo { get { return _mediaVideo; } }
        #endregion

        #region Accessors

        //Contains data relevant to a waveform.
        public Waveform Waveform
        {
            set
            {
                _waveform = value;
                RaisePropertyChanged();
            }
            get { return _waveform; }
        }

        #endregion

        #region Methods
        /// <summary>
        /// This function is to close the video control, it is called by the main control
        /// </summary>
        public void CloseMediaControlViewModel()
        {
            _waveform = null;
            _mediaVideo.Path = null;
            _mediaVideo.Stop();
            _mediaVideo.Close();
            _mediaVideo.CurrentState = LiveDescribeVideoStates.VideoNotLoaded;
        }

        public void LoadVideo(string videoPath)
        {
            Path = videoPath;
            _mediaVideo.CurrentState = LiveDescribeVideoStates.PausedVideo;
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

        /// <summary>
        /// Restores the media back to its original state before playing the given description.
        /// </summary>
        /// <param name="description"></param>
        public void ResumeFromDescription(Description description)
        {
            //if the description is an extended description, we want to move the video forward to get out of the interval of
            //where the extended description will play
            //then we want to replay the video
            if (description.IsExtendedDescription)
            {
                double offset = _mediaVideo.Position.TotalMilliseconds - description.StartInVideo;
                //+1 so we are out of the interval and it doesn't repeat the description
                int newStartInVideo = (int)(_mediaVideo.Position.TotalMilliseconds
                    + (LiveDescribeConstants.ExtendedDescriptionStartIntervalMax - offset + 1));
                _mediaVideo.Position = new TimeSpan(0, 0, 0, 0, newStartInVideo);
                PlayCommand.Execute();
                Log.Info("Extended description finished");
            }
            else
                RestoreVolume();
        }
        #endregion
    }
}

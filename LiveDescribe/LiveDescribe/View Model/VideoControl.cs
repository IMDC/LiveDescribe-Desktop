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
        #region Instance Variables
        private readonly ILiveDescribePlayer _mediaVideo;
        private AudioUtility _audioOperator;
        private List<short> _waveFormData;
        private readonly BackgroundWorker _stripAudioWorker;
        private LoadingViewModel _loadingViewModel;
        private Header _audioHeader;

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
            
            _stripAudioWorker = new BackgroundWorker();
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
            Console.WriteLine("Marker MouseUp");
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// What hapens when the marker is pressed down
        /// </summary>
        /// <param name="param"></param>
        public void OnMarkerMouseDown()
        {
            Console.WriteLine("Marker was pressed down");
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
            Console.WriteLine("Play");

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
            Console.WriteLine("Pause");

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
            Console.WriteLine("LOADED");
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
            Console.WriteLine("Mute");

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
            Console.WriteLine("Media Failed to load...");
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
            Console.WriteLine("Media Finished");
            //Changing state back to video loaded because it is starting from the beginning
            _mediaVideo.CurrentState = LiveDescribeVideoStates.VideoLoaded;
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
            _audioOperator = new AudioUtility(Project);
            _audioOperator.StripAudio(_stripAudioWorker);
            _waveFormData = _audioOperator.ReadWavData(_stripAudioWorker);
            _audioHeader = _audioOperator.Header;
            //_audioOperator.DeleteAudioFile();
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

        /// <summary>
        /// Method that gets bounded to the _stripAudioWorker.ProgressChanged
        /// everytime the progress changes in the audio worker it updates the property CurrentProgressAudioStripping
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">progresschangedeventargs</param>
        public void StrippingAudioProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _loadingViewModel.SetProgress("Importing Video", e.ProgressPercentage);
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
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
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
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
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
        #endregion

        #region setters / getters
        /// <summary>
        /// Get the wavform Data
        /// </summary>
        public List<short> AudioData
        {
            set { _waveFormData = value; }
            get { return this._waveFormData; }
        }

        /// <summary>
        /// Get the audio header
        /// </summary>
        public Header Header
        {
            set { _audioHeader = value; }
            get { return this._audioHeader; }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// This function is to close the video control, it is called by the main control
        /// </summary>
        public void CloseVideoControl()
        {
            _audioOperator = null;
            _waveFormData = null;
            _mediaVideo.Path = null;
            _mediaVideo.Stop();
            _mediaVideo.Close();
            _mediaVideo.CurrentState = LiveDescribeVideoStates.VideoNotLoaded;

            //Unhook events from _stripAudioWorker
            _stripAudioWorker.DoWork -= StripAudio;
            _stripAudioWorker.RunWorkerCompleted -= OnFinishedStrippingAudio;
            _stripAudioWorker.ProgressChanged -= StrippingAudioProgressChanged;
        }

        /// <summary>
        /// This function is used to setup all the events for the background worker and to run it
        /// to strip the audio from the video
        /// </summary>
        /// <param name="path"></param>
        public void SetupAndStripAudio(Project p)
        {
            //changes the Path variable that is binded to the media element
            Path = p.VideoFile;
            Project = p;
            _stripAudioWorker.DoWork += StripAudio;
            _stripAudioWorker.RunWorkerCompleted += OnFinishedStrippingAudio;
            _stripAudioWorker.ProgressChanged += StrippingAudioProgressChanged;
            _stripAudioWorker.WorkerReportsProgress = true;
            _loadingViewModel.SetProgress("Importing Video", 0);
            _loadingViewModel.Visible = true;
            _stripAudioWorker.RunWorkerAsync();
        }
        #endregion
    }
}

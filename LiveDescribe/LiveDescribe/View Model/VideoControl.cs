using System;
using System.ComponentModel;
using LiveDescribe.Graphics;
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
        private double _currentprogressaudiostripping;
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
        public VideoControl(ILiveDescribePlayer mediaVideo)
        {
            _mediaVideo = mediaVideo;
            
            PlayCommand = new RelayCommand(Play, PlayCheck);
            PauseCommand = new RelayCommand(Pause, PauseCheck);
            MuteCommand = new RelayCommand(Mute, param => true);
            FastForwardCommand = new RelayCommand(FastForward, FastForwardCheck);
            RewindCommand = new RelayCommand(Rewind, RewindCheck);

            //Marker commands {
            MarkerMouseDownCommand = new RelayCommand(OnMarkerMouseDown, param=> true);
            MarkerMouseUpCommand = new RelayCommand(OnMarkerMouseUp, param => true);
            MarkerMouseMoveCommand = new RelayCommand(OnMarkerMouseMove, param => true);
            //}

            //bound to when the video loads and is opened via the mediaelement
            VideoOpenedCommand = new RelayCommand(VideoOpen, param => true);
            MediaFailedCommand = new RelayCommand(MediaFailed, param => true);

            MediaEndedCommand = new RelayCommand(MediaEnded, param => true);
            //bound to Menu->file->Import Video
            ImportVideoCommand = new RelayCommand(ImportVideo, ImportCheck);
            
            IsBusyStrippingAudio = false;
            _stripAudioWorker = new BackgroundWorker();
        }
        #endregion

        #region Commands
        /// <summary>
        /// Setter and getter for MediaEndedCommand
        /// </summary>
        public RelayCommand MediaEndedCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Setter and getter for MarkerMouseMoveCommand
        /// </summary>
        public RelayCommand MarkerMouseMoveCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Setter and getter for MarkerMouseUpCommand
        /// </summary>
        public RelayCommand MarkerMouseUpCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Setter and getter for MarkerMouseDownCommand
        /// </summary>
        public RelayCommand MarkerMouseDownCommand
        {
            get;
            private set;
        }

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
        /// what happens when the marker is moved using the mouse
        /// </summary>
        /// <param name="param"></param>
        public void OnMarkerMouseMove(object param)
        {
            EventHandler handler = OnMarkerMouseMoveRequested;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// What happens when the mouse is released on the marker
        /// </summary>
        /// <param name="param"></param>
        public void OnMarkerMouseUp(object param)
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
        public void OnMarkerMouseDown(object param)
        {
            Console.WriteLine("Marker was pressed down");
            this.PauseCommand.Execute(param);
            EventHandler handler = OnMarkerMouseDownRequested;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }
        /// <summary>
        /// Plays the video
        /// </summary>
        /// <param name="param">params</param>
        public void Play(object param)
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
        public void Pause(object param)
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
            _mediaVideo.CurrentState = LiveDescribeVideoStates.VideoLoaded;

            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Mutes the video's audio
        /// </summary>
        /// <param name="param">params</param>
        public void Mute(object param)
        {
            Console.WriteLine("Mute");

            EventHandler handler = MuteRequested;

            if (handler == null) return;
            handler(this, EventArgs.Empty);

        }

        /// <summary>
        /// What happens when Import Video option is select, in this case open up a file dialogbox get the file then strip the audio
        /// </summary>
        /// <param name="param"></param>
        public void ImportVideo(object param)
        {
            //importing a new video so reset the settings file
            //possibly save the old settings value in the livedescribe file? or make a class where we can just serialize it and save that to the file?
            Properties.Settings.Default.Reset();
            var dialogBoxChooseVideo = new OpenFileDialog();
            var dialogBoxChooseWorkingDirectory = new System.Windows.Forms.FolderBrowserDialog();

            bool? userClickedOkChooseVideo = dialogBoxChooseVideo.ShowDialog();

            //handle what happens when the user clicks cancel when choosing a video
            if (userClickedOkChooseVideo == false)
                return;

            System.Windows.Forms.DialogResult userClickedOkChooseWorkingDirectory = dialogBoxChooseWorkingDirectory.ShowDialog();

            //handle what happens when the user clicks cancel when choosing a directory
            if (userClickedOkChooseWorkingDirectory == System.Windows.Forms.DialogResult.Cancel)
                return;

            Console.WriteLine("OPENVIDEO");
            if (userClickedOkChooseVideo == true && userClickedOkChooseWorkingDirectory == System.Windows.Forms.DialogResult.OK)
            {
                Path = dialogBoxChooseVideo.FileName;

                //set the settings file to be the new working directory
                Properties.Settings.Default.WorkingDirectory = dialogBoxChooseWorkingDirectory.SelectedPath + "\\";
                //create a new background worker to strip the audio and set IsBusyStrippingAudio to true
                //it does not get set to false in this class because the view is meant to take care of what they want to do with the stripped audio when it is completed for example
                //create a wave form
                //the variable IsBusyStrippingAudio gets binded to the view (LoadingBorder Visibility property) and when set to false will get rid of the loading screen
                _stripAudioWorker.DoWork += StripAudio;
                _stripAudioWorker.RunWorkerCompleted += OnFinishedStrippingAudio;
                _stripAudioWorker.ProgressChanged += StrippingAudioProgressChanged;
                _stripAudioWorker.WorkerReportsProgress = true;
                IsBusyStrippingAudio = true;
                _stripAudioWorker.RunWorkerAsync();
            }

            _mediaVideo.CurrentState = LiveDescribeVideoStates.PausedVideo;
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
            _mediaVideo.CurrentState = LiveDescribeVideoStates.VideoNotLoaded;
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// thros an event when the media is finished
        /// </summary>
        /// <param name="param"></param>
        public void MediaEnded(object param)
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
            _audioOperator = new AudioUtility(_mediaVideo.Path);
            _audioOperator.StripAudio(_stripAudioWorker);
            _waveFormData = _audioOperator.ReadWavData(_stripAudioWorker);          
           
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
            CurrentProgressAudioStripping = e.ProgressPercentage;
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
        public bool PauseCheck(object param)
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
        public bool RewindCheck(object param)
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
        public bool FastForwardCheck(object param)
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
        public bool RecordCheck(object param)
        {
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
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
            if (_mediaVideo.CurrentState != LiveDescribeVideoStates.VideoNotLoaded)
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

        /// <summary>
        /// Property that gets bound to the LoadingBorder visibility with a converter 
        /// to decide whether the loading border should be seen or not
        /// </summary>
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

        /// <summary>
        /// Property that get's bound to the ImportVideoProgressbar Value property
        /// </summary>
        public double CurrentProgressAudioStripping
        {
            set
            {
                _currentprogressaudiostripping = value;
                RaisePropertyChanged("CurrentProgressAudioStripping");
            }
            get
            {
                return _currentprogressaudiostripping;
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

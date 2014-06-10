using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using LiveDescribe.Events;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;

namespace LiveDescribe.View_Model
{
    public class DescriptionViewModel : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Instance Variables
        private ObservableCollection<Description> _alldescriptions;      //this list contains all the descriptions both regular and extended
        private ObservableCollection<Description> _extendedDescriptions; //this list only contains the extended description this list should be used to bind to the list view of extended descriptions
        private ObservableCollection<Description> _regularDescriptions;  //this list only contains all the regular descriptions this list should only be used to bind to the list of regular descriptions
        private WaveIn _microphonestream;
        private WaveFileWriter _waveWriter;
        private readonly ILiveDescribePlayer _mediaVideo;
        private readonly bool _usingExistingMicrophone;
        /// <summary>Keeps track of the starting time of a description on recording.</summary>
        private double _descriptionStartTime;
        private bool _isRecording;

        private bool _recordingExtendedDescription;

        //this variable should be used as little as possible in this class
        //most interactions between the  descriptionviewmodel and the MediaControlViewModel should be in the MainWindowViewModel
        private readonly MediaControlViewModel _mediaControlViewModel;

        private LiveDescribeVideoStates _previousVideoState; //used to restore the previous video state after it's finished recording

        public Project Project { get; set; }
        #endregion

        #region Event Handlers
        public event EventHandler<DescriptionEventArgs> AddDescriptionEvent;
        public event EventHandler RecordRequested;
        public event EventHandler RecordRequestedMicrophoneNotPluggedIn;
        #endregion

        #region Constructors
        public DescriptionViewModel(ILiveDescribePlayer mediaVideo, MediaControlViewModel mediaControlViewModel)
        {
            _isRecording = false;
            _waveWriter = null;
            RecordCommand = new RelayCommand(Record, RecordStateCheck);
            _mediaVideo = mediaVideo;
            _mediaControlViewModel = mediaControlViewModel;

            Project = null;

            //check if the current microphone exists in the settings
            //if it doesn't use the first one or else get the one stored in the settings
            if (Properties.Settings.Default.Microphone == null)
            {
                _usingExistingMicrophone = false;
            }
            else
            {
                _usingExistingMicrophone = true;
                MicrophoneStream = Properties.Settings.Default.Microphone;
            }
            AllDescriptions = new ObservableCollection<Description>();
            RegularDescriptions = new ObservableCollection<Description>();
            ExtendedDescriptions = new ObservableCollection<Description>();
        }
        #endregion

        #region Commands
        /// <summary>
        /// Setter and getter for RecordCommand
        /// gets bound to the record button
        /// </summary>
        public RelayCommand RecordCommand { private set; get; }
        #endregion

        #region Binding Functions

        /// <summary>
        /// Records the description
        /// </summary>
        /// <param name="param"></param>
        public void Record()
        {
            log.Info("Beginning to record audio");
            //if the button was clicked once already it is in the RecordingDescription State
            //so end the recording because it is the second click
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.RecordingDescription)
            {
                FinishRecordingDescription();
                return;
            }
                                 
            // if we don't have an existing microphone we try to create a new one with the first available microphone
            // if no microphone exists an exception is thrown and we throw the event "RecordRequestedMicrophoneNotPluggedIn
            if (!_usingExistingMicrophone)
            {
                try
                {
                    MicrophoneStream = new WaveIn
                    {
                        DeviceNumber = 0,
                        WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(0).Channels)
                    };
                    log.Info("Product Name of Microphone: " + WaveIn.GetCapabilities(0).ProductName);
                }
                catch (NAudio.MmException e)
                {
                    //Microphone not plugged in
                    log.Warn("Microphone not found");
                    HandleNoMicrophoneException(e);
                    return;
                }
            }
            log.Info("Recording...");
         
            string path = Project.GenerateDescriptionFilePath();
            _waveWriter = new WaveFileWriter(path, MicrophoneStream.WaveFormat);
            MicrophoneStream.DataAvailable += MicrophoneSteam_DataAvailable;
  
            try
            {
                _descriptionStartTime = _mediaVideo.Position.TotalMilliseconds;
                MicrophoneStream.StartRecording();
            }
            catch (NAudio.MmException e)
            {
                //Microphone not plugged in
                log.Error("Previous Microphone was found then unplugged (No Microphone) Exception...");
                HandleNoMicrophoneException(e);
                return;
            }
            //save the current state so when the button is pressed again you can restore it back to that state
            _previousVideoState = _mediaVideo.CurrentState;

            if (_mediaVideo != null) IsRecording = true;
            OnRecordRequested();
        }
        #endregion

        #region BindingProperties

        /// <summary>
        /// property to set the Microphonestream
        /// </summary>
        public WaveIn MicrophoneStream
        {
            set
            {
                _microphonestream = value;
                RaisePropertyChanged();
            }
            get
            {
                return _microphonestream;
            }
        }

        /// <summary>
        /// Property to set and get the ObservableCollection containing all of the descriptions
        /// </summary>
        public ObservableCollection<Description> AllDescriptions
        {
            set
            {
                _alldescriptions = value;
                RaisePropertyChanged();
            }
            get
            {
                return _alldescriptions;
            }
        }

        /// <summary>
        /// Property to set and get the collection with all the extended descriptions
        /// should be bound to the extended description list
        /// </summary>
        public ObservableCollection<Description> ExtendedDescriptions
        {
            set
            {
                _extendedDescriptions = value;
                RaisePropertyChanged();
            }
            get
            {
                return _extendedDescriptions;
            }
        }

        /// <summary>
        /// Property to set and get the collection with all the regular descriptions
        /// should be bound to the regular description list
        /// </summary>
        public ObservableCollection<Description> RegularDescriptions
        {
            set
            {
                _regularDescriptions = value;
                RaisePropertyChanged();
            }
            get
            {
                return _regularDescriptions;
            }
        }

        /// <summary>
        /// Property that gets set when the extended description checkbox is checked or unchecked
        /// </summary>
        public bool ExtendedIsChecked
        {
            set
            {
                _recordingExtendedDescription = value;
                RaisePropertyChanged();
            }
            get
            {
                return _recordingExtendedDescription;
            }
        }

        public bool IsRecording
        {
            set
            {
                _mediaVideo.CurrentState = value ? LiveDescribeVideoStates.RecordingDescription : _previousVideoState;
                _isRecording = value;
                RaisePropertyChanged();
            }
            get
            {
                return _isRecording;
            }
        }
        #endregion

        #region State Checks
        /// <summary>
        /// method to check whether the record command can be executed or not
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool RecordStateCheck()
        {
            return Project != null && _mediaVideo.CurrentState != LiveDescribeVideoStates.VideoNotLoaded;
        }

        /// <summary>
        /// method to check whether the extended description checkbox can be checked or not
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool ExtendedDescriptionCheckboxStateCheck(object param)
        {
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded || _mediaVideo.CurrentState == LiveDescribeVideoStates.RecordingDescription)
                return false;

            return true;
        }

        #endregion

        #region Private Handler Event Methods
        /// <summary>
        /// Write to the wave file, on data available in the microphone stream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MicrophoneSteam_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_waveWriter == null) return;

            _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            _waveWriter.Flush();
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Stops recording a description, and sets the correct state of the media video
        /// it also adds the description that was recorded to the list of descriptions
        /// </summary>
        private void FinishRecordingDescription()
        {
            log.Info("Finished Recording");
            MicrophoneStream.StopRecording();
            string filepath = _waveWriter.Filename;
            _waveWriter.Dispose();
            _waveWriter = null;
            var read = new WaveFileReader(filepath);

            AddDescription(filepath, 0, read.TotalTime.TotalMilliseconds, _descriptionStartTime, ExtendedIsChecked);
            read.Dispose();
            //have to change the state of recording
            IsRecording = false;
        }

        private void HandleNoMicrophoneException(NAudio.MmException e)
        {
            log.Error("No microphone", e);
            OnRecordRequestedMicrophoneNotPluggedIn();
        }

        /// <summary>
        /// Method to add a description to the list and throw an event, whenever you are adding a description to the list you should use this method
        /// </summary>
        /// <param name="filename">Filename of the description</param>
        /// <param name="startwavefiletime">The start time in the wav file of the description</param>
        /// <param name="endwavefiletime">The end time in the wav file of the description</param>
        /// <param name="startinvideo">The time in the video the description should start playing</param>
        /// <param name="isExtendedDescription">Whether it is an extended description or not</param>
        public void AddDescription(string filename, double startwavefiletime, double endwavefiletime,
            double startinvideo, bool isExtendedDescription)
        {
            AddDescription(new Description(filename, startwavefiletime, endwavefiletime, startinvideo,
                isExtendedDescription));
        }

        public void AddDescription(Description desc)
        {
            if (desc.IsExtendedDescription)
                ExtendedDescriptions.Add(desc);
            else
                RegularDescriptions.Add(desc);

            SetupEventsOnDescription(desc);

            AllDescriptions.Add(desc);
            OnAddDescription(desc);
        }

        /// <summary>
        /// Method to setup events on a descriptions no graphics setup should be included in here, that should be in the view
        /// </summary>
        /// <param name="desc">The description to setup the events on</param>
        private void SetupEventsOnDescription(Description desc)
        {
            //this method is called when a description is finished playing
            desc.DescriptionFinishedPlaying += (sender1, e1) =>
                {
                //if the description is an extended description, we want to move the video forward to get out of the interval of
                //where the extended description will play
                //then we want to replay the video
                    if (desc.IsExtendedDescription)
                    {
                        double offset = _mediaVideo.Position.TotalMilliseconds - desc.StartInVideo;
                        //+1 so we are out of the interval and it doesn't repeat the description
                        int newStartInVideo = (int)(_mediaVideo.Position.TotalMilliseconds
                            + (LiveDescribeConstants.ExtendedDescriptionStartIntervalMax - offset + 1)); 
                        _mediaVideo.Position = new TimeSpan(0,0,0,0,newStartInVideo);
                        _mediaControlViewModel.PlayCommand.Execute(this);
                        log.Info("Extended description finished");
                    }
                    else
                    {
                        DispatcherHelper.UIDispatcher.Invoke(() => _mediaControlViewModel.RestoreVolume());
                    }

                };

            //this method gets called when a description is deleted
            desc.DescriptionDeleteEvent += (sender1, e1) =>
                {
                    //remove description from appropriate lists
                    if (desc.IsExtendedDescription)
                        ExtendedDescriptions.Remove(desc);
                    else if (!desc.IsExtendedDescription)
                        RegularDescriptions.Remove(desc);

                    AllDescriptions.Remove(desc);
                };
        }
        #endregion

        #region Functions Called by MainWindowViewModel
        /// <summary>
        /// This function closes everything necessary to start fresh
        /// </summary>
        public void CloseDescriptionViewModel()
        {
            AllDescriptions.Clear();
            ExtendedDescriptions.Clear();
            RegularDescriptions.Clear();
            _waveWriter = null;
        }
        #endregion

        #region Event Invokation Methods

        private void OnAddDescription(Description desc)
        {
            EventHandler<DescriptionEventArgs> addDescriptionHandler = AddDescriptionEvent;
            if (addDescriptionHandler != null)
                addDescriptionHandler(this, new DescriptionEventArgs(desc));
        }

        private void OnRecordRequested()
        {
            EventHandler handlerRecordRequested = RecordRequested;
            if (handlerRecordRequested != null)
                handlerRecordRequested(this, EventArgs.Empty);
        }

        private void OnRecordRequestedMicrophoneNotPluggedIn()
        {
            EventHandler handlerNotPluggedIn = RecordRequestedMicrophoneNotPluggedIn;
            if (handlerNotPluggedIn != null)
                RecordRequestedMicrophoneNotPluggedIn(this, EventArgs.Empty);
        }
        #endregion
    }
}

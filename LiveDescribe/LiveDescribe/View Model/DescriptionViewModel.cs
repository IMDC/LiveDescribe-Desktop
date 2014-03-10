using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Interfaces;
using NAudio.Wave;
using System.IO;
using LiveDescribe.Model;
using LiveDescribe.Events;
using System.Windows.Data;

namespace LiveDescribe.View_Model
{
    public class DescriptionViewModel : ViewModelBase
    {

        #region Instance Variables
        private ObservableCollection<Description> _alldescriptions;      //this list contains all the descriptions both regular and extended
        private ObservableCollection<Description> _extendedDescriptions; //this list only contains the extended description this list should be used to bind to the list view of extended descriptions
        private ObservableCollection<Description> _regularDescriptions;  //this list only contains all the regular descriptions this list should only be used to bind to the list of regular descriptions
        private NAudio.Wave.WaveIn _microphonestream;
        private NAudio.Wave.WaveFileWriter _waveWriter;
        private readonly ILiveDescribePlayer _mediaVideo;
        private bool _usingExistingMicrophone;
        private double _descriptionStartTime;
        private Description _descriptionSelectedInList;
        private String _descriptionText;

        //this variable should be used as little as possible in this class
        //most interactions between the  descriptionviewmodel and the videocontrol should be in the maincontrol
        private VideoControl _videoControl;

        private LiveDescribeVideoStates _previousVideoState; //used to restore the previous video state after it's finished recording
        #endregion

        #region Event Handlers
        public EventHandler<DescriptionEventArgs> AddDescriptionEvent;
        public EventHandler RecordRequested;
        public EventHandler RecordRequestedMicrophoneNotPluggedIn;
        public bool _recordingExtendedDescription;
        #endregion

        #region Constructors
        public DescriptionViewModel(ILiveDescribePlayer mediaVideo, VideoControl videoControl)
        {
            _waveWriter = null;
            DescriptionSelectedInList = null;
            DescriptionText = "Enter Description Text";
            RecordCommand = new RelayCommand(Record, RecordStateCheck);
            SaveDescriptionTextCommand = new RelayCommand(SaveDescriptionText,SaveTextStateCheck);
            ClearDescriptionTextCommand = new RelayCommand(ClearDescriptionText, ()=>true);
            _mediaVideo = mediaVideo;
            _videoControl = videoControl;

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

        /// <summary>
        /// Command to save description text to the selected description
        /// </summary>
        public RelayCommand SaveDescriptionTextCommand { private set; get; }

        /// <summary>
        /// Command to clear the textbox of the description
        /// </summary>
        public RelayCommand ClearDescriptionTextCommand { private set; get; }
        #endregion

        #region Binding Functions

        /// <summary>
        /// Clears the description text
        /// </summary>
        public void ClearDescriptionText()
        {
            DescriptionText = "";
        }

        /// <summary>
        /// Changes the description text of the selected description
        /// </summary>
        public void SaveDescriptionText()
        {
            DescriptionSelectedInList.DescriptionText = DescriptionText;
        }

        /// <summary>
        /// Records the description
        /// </summary>
        /// <param name="param"></param>
        public void Record()
        {
            Console.WriteLine("----------------------");
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
                    MicrophoneStream = new NAudio.Wave.WaveIn
                    {
                        DeviceNumber = 0,
                        WaveFormat = new NAudio.Wave.WaveFormat(44100, NAudio.Wave.WaveIn.GetCapabilities(0).Channels)
                    };
                    Console.WriteLine("Product Name of Microphone: " + NAudio.Wave.WaveIn.GetCapabilities(0).ProductName);
                }
                catch (NAudio.MmException e)
                {
                    //Microphone not plugged in
                    Console.WriteLine("Creating new microphone (No Microphone) Exception....");
                    HandleNoMicrophoneException(e);
                    return;
                }
            }
            Console.WriteLine("Recording..");
         
            // get a random guid to name the wave file
            // there is an EXTREMELY small chance that the guid used has been used before
            Guid g = Guid.NewGuid();
            _waveWriter = new NAudio.Wave.WaveFileWriter(Properties.Settings.Default.WorkingDirectory + g.ToString() + ".wav", MicrophoneStream.WaveFormat);          
            MicrophoneStream.DataAvailable += new EventHandler<NAudio.Wave.WaveInEventArgs>(MicrophoneSteam_DataAvailable);
  
            try
            {
                _descriptionStartTime = _mediaVideo.Position.TotalMilliseconds;
                MicrophoneStream.StartRecording();
            }
            catch (NAudio.MmException e)
            {
                //Microphone not plugged in
                Console.WriteLine("Previous Microphone was found then unplugged (No Microphone) Exception...");
                HandleNoMicrophoneException(e);
                return;
            }
            //save the current state so when the button is pressed again you can restore it back to that state
            _previousVideoState = _mediaVideo.CurrentState;

            EventHandler handlerRecordRequested = RecordRequested;
            if (_mediaVideo != null) _mediaVideo.CurrentState = LiveDescribeVideoStates.RecordingDescription;
            if (handlerRecordRequested == null) return;
            handlerRecordRequested(this, EventArgs.Empty);
        }
        #endregion

        #region BindingProperties
        /// <summary>
        /// This value gets set when a description gets selected in the List of descriptions in the tabs
        /// </summary>
        public Description DescriptionSelectedInList
        {
            set
            {
                _descriptionSelectedInList = value;
                //possibly make a property to change the description's behaviour depending on if it was set through the list
                //for example _descriptionSelectedInList.WasSelectedInList = true;
                RaisePropertyChanged("DescriptionSelectedInList");
            }
            get
            {
                return _descriptionSelectedInList;
            }
        
        }

        /// <summary>
        /// This property sets the description text, that is used to change the DescriptionSelectedInList's _descriptionText value
        /// </summary>
        public String DescriptionText
        {
            set
            {
                _descriptionText = value;
                RaisePropertyChanged("DescriptionText");
            }
            get
            {
                return _descriptionText;
            }
        }

        /// <summary>
        /// property to set the Microphonestream
        /// </summary>
        public NAudio.Wave.WaveIn MicrophoneStream
        {
            set
            {
                _microphonestream = value;
                RaisePropertyChanged("MicrophoneStream");
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
                RaisePropertyChanged("AllDescriptions");
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
                RaisePropertyChanged("ExtendedDescriptions");
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
                RaisePropertyChanged("RegularDescriptions");
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
                RaisePropertyChanged("RecordingExtendedDescription");
            }
            get
            {
                return _recordingExtendedDescription;
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
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return false;
            return true;
        }

        /// <summary>
        /// method to check whether the save text button can be clicked or not
        /// </summary>
        /// <returns></returns>
        public bool SaveTextStateCheck()
        {
            if (DescriptionSelectedInList == null)
                return false;
            return true;
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

        #region Private Event Methods
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
            Console.WriteLine("Finished Recording");
            MicrophoneStream.StopRecording();
            string filename = _waveWriter.Filename;
            _waveWriter.Dispose();
            _waveWriter = null;
            NAudio.Wave.WaveFileReader read = new NAudio.Wave.WaveFileReader(filename);
            AddDescription(filename, 0, read.TotalTime.TotalMilliseconds, _descriptionStartTime, ExtendedIsChecked);
            read.Dispose();
            //have to change the state of recording
            _mediaVideo.CurrentState = _previousVideoState;
        }

        private void HandleNoMicrophoneException(NAudio.MmException e)
        {
            Console.WriteLine(e.StackTrace);
            EventHandler handlerNotPluggedIn = RecordRequestedMicrophoneNotPluggedIn;
            if (handlerNotPluggedIn == null) return;
            RecordRequestedMicrophoneNotPluggedIn(this, EventArgs.Empty);
        }

        /// <summary>
        /// Method to add a description to the list and throw an event, whenever you are adding a description to the list you should use this method
        /// </summary>
        /// <param name="filename">Filename of the description</param>
        /// <param name="startwavefiletime">The start time in the wav file of the description</param>
        /// <param name="endwavefiletime">The end time in the wav file of the description</param>
        /// <param name="startinvideo">The time in the video the description should start playing</param>
        /// <param name="isExtendedDescription">Whether it is an extended description or not</param>
        public void AddDescription(string filename, double startwavefiletime, double endwavefiletime, double startinvideo, bool isExtendedDescription)
        {
            Description desc = new Description(filename, startwavefiletime, endwavefiletime, startinvideo, isExtendedDescription);
            if (isExtendedDescription)
                ExtendedDescriptions.Add(desc);
            else
                RegularDescriptions.Add(desc);

            SetupEventsOnDescription(desc);

            AllDescriptions.Add(desc);
            EventHandler<DescriptionEventArgs> addDescriptionHandler = AddDescriptionEvent;
            if (addDescriptionHandler == null) return;
            addDescriptionHandler(this, new DescriptionEventArgs(desc));
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
                        int newStartInVideo = (int)(_mediaVideo.Position.TotalMilliseconds + (LiveDescribeConstants.EXTENDED_DESCRIPTION_START_INTERVAL_MAX - offset + 1)); 
                        _mediaVideo.Position = new TimeSpan(0,0,0,0,newStartInVideo);
                        _videoControl.PlayCommand.Execute(this);
                        Console.WriteLine("Extended Description Finished!");
                    }

                };

            //this method gets called when a description is deleted
            desc.DescriptionDeleteEvent += (sender1, e1) =>
                {
                    //remove description from appropriate lists
                };
        }
        #endregion

        #region Functions Called by MainControl
        /// <summary>
        /// This function closes everything necessary to start fresh
        /// </summary>
        public void CloseDescriptionViewModel()
        {
            AllDescriptions = null;
            ExtendedDescriptions = null;
            RegularDescriptions = null;
            _waveWriter = null;
            AllDescriptions = new ObservableCollection<Description>();
            ExtendedDescriptions = new ObservableCollection<Description>();
            RegularDescriptions = new ObservableCollection<Description>();
        }
        #endregion
    }
}

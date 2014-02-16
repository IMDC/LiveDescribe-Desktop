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

namespace LiveDescribe.View_Model
{
    public class DescriptionViewModel : ViewModelBase
    {

        #region Instance Variables
        //this list contains all the descriptions both regular and extended
        private ObservableCollection<Description> _alldescriptions;
        //this list only contains the extended description this list should be used to bind to the list view of extended descriptions
        private ObservableCollection<Description> _extendedDescriptions;
        //this list only contains all the regular descriptions this list should only be used to bind to the list of regular descriptions
        private ObservableCollection<Description> _regularDescriptions;
        private NAudio.Wave.WaveIn _microphonestream;
        private NAudio.Wave.WaveFileWriter _waveWriter;
        private readonly ILiveDescribePlayer _mediaVideo;
        private bool _usingExistingMicrophone;
        private double _descriptionStartTime;
        private Description _descriptionSelectedInList;

        //used to restore the previous video state after it's finished recording
        private LiveDescribeVideoStates _previousVideoState;
        #endregion

        #region Event Handlers
        public EventHandler<DescriptionEventArgs> AddDescriptionEvent;
        public EventHandler RecordRequested;
        public EventHandler RecordRequestedMicrophoneNotPluggedIn;
        public bool _recordingExtendedDescription;
        #endregion

        #region Constructors
        public DescriptionViewModel(ILiveDescribePlayer mediaVideo)
        {
            _waveWriter = null;
            RecordCommand = new RelayCommand(Record, RecordStateCheck);
            _mediaVideo = mediaVideo;

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
        public RelayCommand RecordCommand
        {
            private set;
            get;
        }
        #endregion

        #region Binding Functions

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
                return;
            }
            
            EventHandler handlerRecordRequested = RecordRequested;
            EventHandler handlerNotPluggedIn = RecordRequestedMicrophoneNotPluggedIn;
            
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
                    Console.WriteLine("Creating new microphone Exception....");
                    Console.WriteLine(e.StackTrace);

                    if (handlerNotPluggedIn == null) return;
                    RecordRequestedMicrophoneNotPluggedIn(this, EventArgs.Empty);
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
                _descriptionStartTime = _mediaVideo.CurrentPosition.TotalMilliseconds;
                MicrophoneStream.StartRecording();
            }
            catch (NAudio.MmException e)
            {
                Console.WriteLine("Previous Microphone Exception...");
                Console.WriteLine(e.StackTrace);

                if (handlerNotPluggedIn == null) return;
                RecordRequestedMicrophoneNotPluggedIn(this, EventArgs.Empty);
                return;
            }

            //save the current state so when the button is pressed again you can restore it back to that state
            _previousVideoState = _mediaVideo.CurrentState;

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

        private void MicrophoneSteam_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_waveWriter == null) return;

            _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            _waveWriter.Flush();
        }

        #endregion

        #region Helper Methods
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

            AllDescriptions.Add(desc);
            EventHandler<DescriptionEventArgs> addDescriptionHandler = AddDescriptionEvent;
            if (addDescriptionHandler == null) return;
            addDescriptionHandler(this, new DescriptionEventArgs(desc));
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

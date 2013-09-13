using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer.Framework;
using Microsoft.TeamFoundation.MVVM;
using LiveDescribe.Interfaces;
using NAudio.Wave;

namespace LiveDescribe.View_Model
{
    public class DescriptionViewModel : ViewModelBase
    {

        #region Instance Variables
        private NAudio.Wave.WaveIn _microphonestream;
        private NAudio.Wave.WaveFileWriter waveWriter;
        private readonly ILiveDescribePlayer _mediaVideo;
        private bool _usingExistingMicrophone;

        //used to restore the previous video state after it's finished recording
        private LiveDescribeVideoStates _previousVideoState;
        #endregion

        #region Event Handlers
        public EventHandler RecordRequested;
        public EventHandler RecordRequestedMicrophoneNotPluggedIn;
        #endregion

        #region Constructors

        public DescriptionViewModel(ILiveDescribePlayer mediaVideo)
        {
            waveWriter = null;
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
        }
        #endregion

        #region Commands

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
        public void Record(object param)
        {
            Console.WriteLine("----------------------");
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.RecordingDescription)
            {
                //have to change the state of recording
                _mediaVideo.CurrentState = _previousVideoState;
                Console.WriteLine("_mediaVideo == Recording");
                MicrophoneStream.StopRecording();
                waveWriter.Dispose();
                waveWriter = null;
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
            waveWriter = new NAudio.Wave.WaveFileWriter("C:\\Users\\imdc\\Desktop\\" + g.ToString() + ".wav", MicrophoneStream.WaveFormat);
            
            MicrophoneStream.DataAvailable += new EventHandler<NAudio.Wave.WaveInEventArgs>(MicrophoneSteam_DataAvailable);
           // var waveIn = new NAudio.Wave.WaveInProvider(MicrophoneStream);
  
            try
            {
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

            _previousVideoState = _mediaVideo.CurrentState;
            if (_mediaVideo != null) _mediaVideo.CurrentState = LiveDescribeVideoStates.RecordingDescription;
            if (handlerRecordRequested == null) return;
            handlerRecordRequested(this, EventArgs.Empty);
        }

        
        #endregion

        #region BindingProperties
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
        #endregion

        #region State Checks

        public bool RecordStateCheck(object param)
        {
            if (_mediaVideo.CurrentState == LiveDescribeVideoStates.VideoNotLoaded)
                return false;
            return true;
        }

        #endregion

        #region Private Event Methods

        private void MicrophoneSteam_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveWriter == null) return;

            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();
        }

        #endregion
    }
}

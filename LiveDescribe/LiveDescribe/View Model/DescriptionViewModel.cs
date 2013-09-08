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
        private readonly ILiveDescribePlayer _mediaVideo;
        private bool _usingExistingMicrophone;
        #endregion

        #region Event Handlers
        public EventHandler RecordRequested;
        public EventHandler RecordRequestedMicrophoneNotPluggedIn;
        #endregion

        #region Constructors

        public DescriptionViewModel(ILiveDescribePlayer mediaVideo)
        {
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
            if (_mediaVideo != null) _mediaVideo.CurrentState = LiveDescribeStates.RecordingDescription;

            EventHandler handler = RecordRequested;
            EventHandler handler1 = RecordRequestedMicrophoneNotPluggedIn;
            
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
                    Console.WriteLine("Creating new microphone....");
                    Console.WriteLine(e.StackTrace);

                    if (handler1 == null) return;
                    RecordRequestedMicrophoneNotPluggedIn(this, EventArgs.Empty);
                    return;
                }
            }

            var waveOut = new NAudio.Wave.DirectSoundOut();
            var waveIn = new NAudio.Wave.WaveInProvider(MicrophoneStream);
            waveOut.Init(waveIn);
  
            try
            {
                MicrophoneStream.StartRecording();
            }
            catch (NAudio.MmException e)
            {
                Console.WriteLine("Previous Microphone...");
                Console.WriteLine(e.StackTrace);

                if (handler1 == null) return;
                RecordRequestedMicrophoneNotPluggedIn(this, EventArgs.Empty);

                return;
            }
            
            waveOut.Play();
            if (handler == null) return;
            handler(this, EventArgs.Empty);
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
            if (_mediaVideo.CurrentState == LiveDescribeStates.VideoNotLoaded)
                return false;
            return true;
        }
        #endregion
    }
}

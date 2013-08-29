using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer.Framework;
using Microsoft.TeamFoundation.MVVM;
using LiveDescribe.Interfaces;

namespace LiveDescribe.View_Model
{
    public class DescriptionViewModel : ViewModelBase
    {

        #region Instance Variables
        private NAudio.Wave.WaveIn _microphonestream;
        private readonly ILiveDescribePlayer _mediaVideo;
        #endregion

        #region Event Handlers
        public EventHandler RecordRequested;
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
                MicrophoneStream = new NAudio.Wave.WaveIn
                {
                    DeviceNumber = 0,
                    WaveFormat = new NAudio.Wave.WaveFormat(44100, NAudio.Wave.WaveIn.GetCapabilities(0).Channels)
                };
                Console.WriteLine("Product Name of Microphone: " + NAudio.Wave.WaveIn.GetCapabilities(0).ProductName);
                //this would save the first microphone to the project settings
                //Properties.Settings.Default.Microphone = MicroPhoneStream;
               // Properties.Settings.Default.Save();
            }
            else
            {
                MicrophoneStream = Properties.Settings.Default.Microphone;
                Console.WriteLine("Product Name of Saved Microphone: " + NAudio.Wave.WaveIn.GetCapabilities(MicrophoneStream.DeviceNumber).ProductName);
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
            _mediaVideo.CurrentState = LiveDescribeStates.RecordingDescription;

            EventHandler handler = RecordRequested;
            var waveOut = new NAudio.Wave.DirectSoundOut();
            var waveIn = new NAudio.Wave.WaveInProvider(MicrophoneStream);
            waveOut.Init(waveIn);
            
            MicrophoneStream.StartRecording();
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

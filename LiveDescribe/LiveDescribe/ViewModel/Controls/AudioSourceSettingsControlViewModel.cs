using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace LiveDescribe.ViewModel.Controls
{
    public class AudioSourceSettingsControlViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<AudioSourceInfo> _sources;
        private AudioSourceInfo _selectedsource;
        private short _microphoneReceiveLevel;
        private WaveIn _microphoneRecorder;
        private BufferedWaveProvider _microphoneBuffer;
        private WaveOut _microphonePlayer;
        #endregion

        #region Constructor
        public AudioSourceSettingsControlViewModel()
        {
            _sources = new ObservableCollection<AudioSourceInfo>();

            InitCommands();
        }

        private void InitCommands()
        {
            TestMicrophone = new RelayCommand(
                canExecute: () => SelectedAudioSource != null,
                execute: () =>
                {
                    if (_microphoneRecorder == null)
                        StartMicrophoneTest();
                    else
                        StopMicrophoneTest();
                });
        }
        #endregion

        #region Commands
        public ICommand TestMicrophone { get; private set; }
        #endregion

        #region Properties
        /// <summary>
        /// Collection that holds all the AudioSourceInfo for every microphone available
        /// </summary>
        public ObservableCollection<AudioSourceInfo> Sources
        {
            get { return _sources; }
        }

        /// <summary>
        /// The object in the AudioSourceInfo Collection that is selected in the preferences window
        /// </summary>
        public AudioSourceInfo SelectedAudioSource
        {
            set
            {
                _selectedsource = value;
                RaisePropertyChanged();
            }
            get { return _selectedsource; }
        }

        public short MicrophoneReceiveLevel
        {
            set
            {
                _microphoneReceiveLevel = value;
                RaisePropertyChanged();
            }
            get { return _microphoneReceiveLevel; }
        }
        #endregion

        #region Methods
        private void StopMicrophoneTest()
        {
            if (_microphonePlayer == null)
                return;

            _microphoneRecorder.StopRecording();
            _microphonePlayer.Stop();

            _microphoneRecorder.Dispose();
            _microphoneRecorder = null;

            _microphonePlayer.Dispose();
            _microphonePlayer = null;

            _microphoneBuffer.ClearBuffer();

            MicrophoneReceiveLevel = 0;
        }

        private void StartMicrophoneTest()
        {
            var recordFormat = new WaveFormat(44100, SelectedAudioSource.Source.Channels);
            _microphoneRecorder = new WaveIn
            {
                DeviceNumber = SelectedAudioSource.DeviceNumber,
                WaveFormat = recordFormat,
            };
            _microphoneRecorder.DataAvailable += MicrophoneRecorderOnDataAvailable;

            _microphoneBuffer = new BufferedWaveProvider(recordFormat);

            _microphonePlayer = new WaveOut();
            _microphonePlayer.Init(_microphoneBuffer);

            _microphoneRecorder.StartRecording();
            _microphonePlayer.Play();
        }

        /// <summary>
        /// used to initialize the Collection of all the microphones available
        /// </summary>
        public void InitializeAudioSourceInfo()
        {
            for (int i = 0; i < WaveIn.DeviceCount; ++i)
            {
                var capability = WaveIn.GetCapabilities(i);
                var audioSource = new AudioSourceInfo(capability.ProductName,
                    capability.Channels.ToString(CultureInfo.InvariantCulture), capability, i);
                if (!Sources.Contains(audioSource))
                    Sources.Add(audioSource);
            }

            if (Settings.Default.Microphone != null && 0 < Sources.Count)
                SelectedAudioSource = Sources.First(audioSourceInfo =>
                    audioSourceInfo.DeviceNumber == Settings.Default.Microphone.DeviceNumber);
            else if (0 < Sources.Count)
                SelectedAudioSource = Sources[0];
            else
                SelectedAudioSource = null;
        }

        /// <summary>
        /// used to save the selected microphone to the settings
        /// </summary>
        public void SaveAudioSourceInfo()
        {
            if (SelectedAudioSource == null)
                return;

            var sourceStream = new WaveIn
            {
                DeviceNumber = SelectedAudioSource.DeviceNumber,
                WaveFormat = new WaveFormat(44100, SelectedAudioSource.Source.Channels)
            };

            Settings.Default.Microphone = sourceStream;
            Settings.Default.Save();
        }

        /// <summary>
        /// Stops any recorders/players for window closing.
        /// </summary>
        public void StopForClose()
        {
            StopMicrophoneTest();
        }
        #endregion

        #region EventHandlers
        /// <summary>
        /// Collects sample information from the mic and sets the receive level to the max volume
        /// amount found.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Args.</param>
        private void MicrophoneRecorderOnDataAvailable(object sender, WaveInEventArgs args)
        {
            const int bytesPerSample = 40;
            short max = 0;
            for (int i = 0; i + 4 < args.BytesRecorded; i += bytesPerSample)
            {
                short leftValue = BitConverter.ToInt16(args.Buffer, i);
                short rightValue = BitConverter.ToInt16(args.Buffer, i + 2);

                max = Math.Max(max, Math.Max(leftValue, rightValue));
            }
            MicrophoneReceiveLevel = max;
            _microphoneBuffer.AddSamples(args.Buffer, 0, args.BytesRecorded);
        }
        #endregion
    }
}

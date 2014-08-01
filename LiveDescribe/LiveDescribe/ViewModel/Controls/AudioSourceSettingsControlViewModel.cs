using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using NAudio.Mixer;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LiveDescribe.ViewModel.Controls
{
    public class AudioSourceSettingsControlViewModel : ViewModelBase, ISettingsViewModel
    {
        #region Fields
        private readonly ObservableCollection<AudioSourceInfo> _sources;
        private AudioSourceInfo _selectedsource;
        private short _microphoneReceiveLevel;
        private WaveIn _microphoneRecorder;
        private BufferedWaveProvider _microphoneBuffer;
        private WaveOut _microphonePlayer;
        private UnsignedMixerControl _microphoneVolumeControl;
        private double _microphoneVolume;
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
                    if (_microphonePlayer == null)
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

        public double MicrophoneVolume
        {
            set
            {
                _microphoneVolume = value;
                RaisePropertyChanged();
            }
            get { return _microphoneVolume; }
        }

        #endregion

        #region Methods
        public void RetrieveApplicationSettings()
        {
            Sources.Clear();

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

        public void SetApplicationSettings()
        {
            if (SelectedAudioSource == null)
                return;

            var sourceStream = new WaveIn
            {
                DeviceNumber = SelectedAudioSource.DeviceNumber,
                WaveFormat = new WaveFormat(44100, SelectedAudioSource.Source.Channels)
            };

            Settings.Default.Microphone = sourceStream;
        }

        private void SetMicrophoneRecorder()
        {
            //Cleanup
            if(_microphoneRecorder != null)
            {
                _microphoneRecorder.StopRecording();
                _microphoneRecorder.Dispose();
            }

            _microphoneRecorder = new WaveIn
            {
                DeviceNumber = SelectedAudioSource.DeviceNumber,
                WaveFormat = new WaveFormat(44100, SelectedAudioSource.Source.Channels),
            };
            _microphoneRecorder.DataAvailable += MicrophoneRecorderOnDataAvailable;

            _microphoneRecorder.StartRecording();

            TryGetVolumeControl();
        }

        private void StartMicrophoneTest()
        {
            _microphoneBuffer = new BufferedWaveProvider(_microphoneRecorder.WaveFormat);

            _microphonePlayer = new WaveOut();
            _microphonePlayer.Init(_microphoneBuffer);

            _microphonePlayer.Play();
        }

        /// <summary>
        /// Attempts to get a volume control for the current microphone and sets the volume.
        /// </summary>
        private void TryGetVolumeControl()
        {
            if (_microphoneRecorder == null)
                return;
            //Older than Vista
            if (Environment.OSVersion.Version.Major < 6)
                return;

            var mixerLine = _microphoneRecorder.GetMixerLine();
            foreach (var control in mixerLine.Controls)
            {
                if (control.ControlType == MixerControlType.Volume)
                {
                    _microphoneVolumeControl = control as UnsignedMixerControl;
                    if (_microphoneVolumeControl != null)
                        MicrophoneVolume = _microphoneVolumeControl.Percent;
                    break;
                }
            }
        }

        private void StopMicrophoneTest()
        {
            if (_microphonePlayer == null)
                return;

            _microphonePlayer.Stop();

            _microphonePlayer.Dispose();
            _microphonePlayer = null;

            _microphoneBuffer = null;
        }

        /// <summary>
        /// Stops any recorders/players for window closing.
        /// </summary>
        public void StopForClose()
        {
            StopMicrophoneTest();
        }

        protected override void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);

            if (propertyName == "SelectedAudioSource")
                SetMicrophoneRecorder();
            if (propertyName == "MicrophoneVolume" && _microphoneVolumeControl != null)
                _microphoneVolumeControl.Percent = MicrophoneVolume;
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

            if (_microphoneBuffer != null)
                _microphoneBuffer.AddSamples(args.Buffer, 0, args.BytesRecorded);
        }
        #endregion
    }
}

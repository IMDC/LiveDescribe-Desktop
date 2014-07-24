using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Extensions;
using LiveDescribe.Factories;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using LiveDescribe.Resources.UiStrings;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    public class PreferencesViewModel : ViewModelBase
    {
        #region Inner Classes

        public class AudioSourceInfo : ISerializable
        {
            public WaveInCapabilities Source { set; get; }
            public string Name { set; get; }
            public string Channels { set; get; }
            public int DeviceNumber { set; get; }
            public AudioSourceInfo(string name, string channels, WaveInCapabilities source, int deviceNumber)
            {
                Name = name;
                Channels = channels;
                Source = source;
                DeviceNumber = deviceNumber;
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("source", Source, typeof(WaveInCapabilities));
            }

            public AudioSourceInfo(SerializationInfo info, StreamingContext context)
            {
                Source =
                    (WaveInCapabilities)info.GetValue("source", typeof(WaveInCapabilities));
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                var info = obj as AudioSourceInfo;
                if (info == null)
                    return false;

                return (Name == info.Name)
                    && (Channels == info.Channels)
                    && (DeviceNumber == info.DeviceNumber);
            }

            public bool Equals(AudioSourceInfo info)
            {
                if (info == null)
                    return false;

                return (Name == info.Name)
                    && (Channels == info.Channels)
                    && (DeviceNumber == info.DeviceNumber);
            }

            public override int GetHashCode()
            {
                String str = Name + Channels + DeviceNumber;
                return str.GetHashCode();
            }
        }
        #endregion

        #region Instance Variables
        private ObservableCollection<AudioSourceInfo> _sources;
        private AudioSourceInfo _selectedsource;
        private ColourScheme _colourScheme;
        #endregion

        #region Events
        public event EventHandler ApplyRequested;
        public event EventHandler RequestClose;
        #endregion

        #region Constructors
        public PreferencesViewModel()
        {
            _sources = new ObservableCollection<AudioSourceInfo>();
            ColourScheme = new ColourScheme(ColourScheme.DefaultColourScheme);

            InitCommands();
        }

        private void InitCommands()
        {
            AcceptChanges = new RelayCommand(
                canExecute: () => true,
                execute: () =>
                {
                    SaveAudioSourceInfo();
                    OnApplyRequested();
                });

            AcceptChangesAndClose = new RelayCommand(
                canExecute: () => AcceptChanges.CanExecute(),
                execute: () =>
                {
                    AcceptChanges.Execute();
                    OnRequestClose();
                });

            CancelChanges = new RelayCommand(
                canExecute: () => true,
                execute: OnRequestClose);

            ResetColourScheme = new RelayCommand(
                canExecute: () => true,
                execute: () =>
                {
                    var result = MessageBoxFactory.ShowWarningQuestion(UiStrings.MessageBox_ResetColourSchemeWarning);

                    if (result == MessageBoxResult.Yes)
                        ColourScheme = new ColourScheme(ColourScheme.DefaultColourScheme);
                });
        }

        #endregion

        #region Commands

        /// <summary>
        /// called when the preferences should be saved and applied to the settings
        /// </summary>
        public ICommand AcceptChanges { get; private set; }
        public ICommand AcceptChangesAndClose { get; private set; }
        public ICommand CancelChanges { get; private set; }
        public ICommand ResetColourScheme { get; private set; }
        #endregion

        #region Properties

        /// <summary>
        /// Collection that holds all the AudioSourceInfo for every microphone available
        /// </summary>
        public ObservableCollection<AudioSourceInfo> Sources
        {
            private set
            {
                _sources = value;
                RaisePropertyChanged();
            }
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

        public ColourScheme ColourScheme
        {
            get { return _colourScheme; }
            set
            {
                _colourScheme = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

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
        private void SaveAudioSourceInfo()
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
        #endregion

        #region Event Invokation
        private void OnApplyRequested()
        {
            var handler = ApplyRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Makes a request to the view to close itself.
        /// </summary>
        private void OnRequestClose()
        {
            var handler = RequestClose;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion
    }
}

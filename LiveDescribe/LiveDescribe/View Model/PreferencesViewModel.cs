using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.View;
using NAudio.Wave;

namespace LiveDescribe.View_Model
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
                    (WaveInCapabilities) info.GetValue("source", typeof (WaveInCapabilities));
            }
        }
        #endregion

        #region Instance Variables
        private ObservableCollection<AudioSourceInfo> _sources;
        private AudioSourceInfo _selectedsource;
        #endregion

        #region EventHandlers
        public EventHandler ShowPreferencesRequested;
        public EventHandler ApplyRequested;
         #endregion

        #region Constructors
        public PreferencesViewModel()
        {
            _sources = new ObservableCollection<AudioSourceInfo>();
            ApplyCommand = new RelayCommand(Apply, () => true);
        }
        #endregion

        #region Commands

        public RelayCommand ApplyCommand { get; private set; }

        #endregion

        #region Binding Functions

        /// <summary>
        /// called when the preferences should be saved and applied to the settings
        /// </summary>
        /// <param name="obj"></param>
        private void Apply()
        {
            EventHandler handler = ApplyRequested;
            SaveAudioSourceInfo();
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }
        #endregion

        #region Binding Properties

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
            get
            {
                return _sources;
            }
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
            get
            {
                return _selectedsource;
            }
        }
        #endregion

        #region Helper Functions

        /// <summary>
        /// used to initialize the Collection of all the microphones available
        /// </summary>
        public void InitializeAudioSourceInfo()
        {
            for (int i = 0; i < WaveIn.DeviceCount; ++i)
            {
                var capability = WaveIn.GetCapabilities(i);
                Sources.Add(new AudioSourceInfo(capability.ProductName, capability.Channels.ToString(CultureInfo.InvariantCulture), capability,i));
            }
        }

        /// <summary>
        /// used to save the selected microphone to the settings
        /// </summary>
        private void SaveAudioSourceInfo()
        {
            var sourceStream = new WaveIn
            {
                DeviceNumber = SelectedAudioSource.DeviceNumber,
                WaveFormat = new WaveFormat(44100, SelectedAudioSource.Source.Channels)
            };

            Settings.Default.Microphone = sourceStream;
            Settings.Default.Save();
        }
        #endregion
    }
}

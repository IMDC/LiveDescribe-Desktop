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
using Microsoft.TeamFoundation.MVVM;
using NAudio.Wave;

namespace LiveDescribe.View_Model
{
    public class PreferencesViewModel : ViewModelBase
    {

        #region Inner Classes

        public class AudioSourceInfo : ISerializable
        {
            public NAudio.Wave.WaveInCapabilities Source { set; get; }
            public string Name { set; get; }
            public string Channels { set; get; }
            public int DeviceNumber { set; get; }
            public AudioSourceInfo(string name, string channels, NAudio.Wave.WaveInCapabilities source, int deviceNumber)
            {
                Name = name;
                Channels = channels;
                Source = source;
                DeviceNumber = deviceNumber;
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("source", Source, typeof(NAudio.Wave.WaveInCapabilities));
            }

            public AudioSourceInfo(SerializationInfo info, StreamingContext context)
            {
                Source =
                    (NAudio.Wave.WaveInCapabilities) info.GetValue("source", typeof (NAudio.Wave.WaveInCapabilities));
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
            ShowPreferencesCommand = new RelayCommand(ShowPreferences, param=>true);
            _sources = new ObservableCollection<AudioSourceInfo>();
            ApplyCommand = new RelayCommand(Apply, param=>true);
        }
        #endregion

        #region Commands

        public RelayCommand ApplyCommand
        {
            private set;
            get;
        }

        public RelayCommand ShowPreferencesCommand
        {
            private set;
            get;
        }
        #endregion

        #region Binding Functions
        /// <summary>
        /// Called when you want the show preferences window to come up
        /// </summary>
        /// <param name="obj"></param>
        private void ShowPreferences(object obj)
        {
            EventHandler handler = ShowPreferencesRequested;
            InitializeAudioSourceInfo();
            if (handler == null) return;
            handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// called when the preferences should be saved and applied to the settings
        /// </summary>
        /// <param name="obj"></param>
        private void Apply(object obj)
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
                RaisePropertyChanged("Sources");
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
                RaisePropertyChanged("SelectedAudioSource");
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
        private void InitializeAudioSourceInfo()
        {
            for (int i = 0; i < NAudio.Wave.WaveIn.DeviceCount; ++i)
            {
                var capability = NAudio.Wave.WaveIn.GetCapabilities(i);
                Sources.Add(new AudioSourceInfo(capability.ProductName, capability.Channels.ToString(CultureInfo.InvariantCulture), capability,i));
            }
        }

        /// <summary>
        /// used to save the selected microphone to the settings
        /// </summary>
        private void SaveAudioSourceInfo()
        {
            var sourceStream = new NAudio.Wave.WaveIn();
            sourceStream.DeviceNumber = SelectedAudioSource.DeviceNumber;
            sourceStream.WaveFormat = new NAudio.Wave.WaveFormat(44100, SelectedAudioSource.Source.Channels);

            Properties.Settings.Default.Microphone = sourceStream;
            Properties.Settings.Default.Save();
        }
        #endregion
    }
}

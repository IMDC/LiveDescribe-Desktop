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
using System.Windows;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    public class PreferencesViewModel : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Instance Variables
        private readonly ObservableCollection<AudioSourceInfo> _sources;
        private AudioSourceInfo _selectedsource;
        private ColourScheme _colourScheme;
        #endregion

        #region Events
        public event EventHandler RequestClose;
        #endregion

        #region Constructors
        public PreferencesViewModel()
        {
            _sources = new ObservableCollection<AudioSourceInfo>();

            RetrieveApplicationSettings();

            InitCommands();
        }

        private void InitCommands()
        {
            AcceptChanges = new RelayCommand(
                canExecute: () => true,
                execute: SaveApplicationSettings);

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
                        ColourScheme = ColourScheme.DefaultColourScheme.DeepCopy();
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
        /// Gets all relevant data from Application settings and sets the relevant properties with
        /// it.
        /// </summary>
        public void RetrieveApplicationSettings()
        {
            ColourScheme = (Settings.Default.ColourScheme != null)
                ? Settings.Default.ColourScheme.DeepCopy()
                : ColourScheme.DefaultColourScheme.DeepCopy();

            Log.Info("Application settings loaded");
        }

        public void SaveApplicationSettings()
        {
            Settings.Default.ColourScheme = ColourScheme;
            Settings.Default.Save();

            SaveAudioSourceInfo();

            Log.Info("Application settings saved");
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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Extensions;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using LiveDescribe.ViewModel.Controls;
using System;
using System.ComponentModel;
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
        private bool _settingsChanged;
        private readonly AudioSourceSettingsControlViewModel _audioSourceSettingsControlViewModel;
        private readonly ColourSchemeSettingsControlViewModel _colourSchemeSettingsControlViewModel;
        private readonly GeneralSettingsControlViewModel _generalSettingsControlViewModel;
        #endregion

        #region Events
        public event EventHandler RequestClose;
        #endregion

        #region Constructors
        public PreferencesViewModel()
        {
            _audioSourceSettingsControlViewModel = new AudioSourceSettingsControlViewModel();
            _audioSourceSettingsControlViewModel.PropertyChanged += CheckForViewModelSettingsChanged;

            _colourSchemeSettingsControlViewModel = new ColourSchemeSettingsControlViewModel();
            _colourSchemeSettingsControlViewModel.PropertyChanged += CheckForViewModelSettingsChanged;

            RetrieveApplicationSettings();

            InitCommands();
        }

        private void InitCommands()
        {
            AcceptChanges = new RelayCommand(
                canExecute: () => SettingsChanged,
                execute: SaveApplicationSettings);

            AcceptChangesAndClose = new RelayCommand(
                canExecute: () => true,
                execute: () =>
                {
                    AcceptChanges.ExecuteIfCan();
                    OnRequestClose();
                });

            CancelChanges = new RelayCommand(
                canExecute: () => true,
                execute: OnRequestClose);
        }
        #endregion

        #region Commands

        /// <summary>
        /// called when the preferences should be saved and applied to the settings
        /// </summary>
        public ICommand AcceptChanges { get; private set; }
        public ICommand AcceptChangesAndClose { get; private set; }
        public ICommand CancelChanges { get; private set; }
        #endregion

        #region Properties

        public bool SettingsChanged
        {
            set
            {
                _settingsChanged = value;
                RaisePropertyChanged();
            }
            get { return _settingsChanged; }
        }

        public ColourSchemeSettingsControlViewModel ColourSchemeSettingsControlViewModel
        {
            get { return _colourSchemeSettingsControlViewModel; }
        }

        public AudioSourceSettingsControlViewModel AudioSourceSettingsControlViewModel
        {
            get { return _audioSourceSettingsControlViewModel; }
        }

        public GeneralSettingsControlViewModel GeneralSettingsControlViewModel
        {
            get { return _generalSettingsControlViewModel; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all relevant data from Application settings and sets the relevant properties with
        /// it.
        /// </summary>
        public void RetrieveApplicationSettings()
        {
            ColourSchemeSettingsControlViewModel.ColourScheme = (Settings.Default.ColourScheme != null)
                ? Settings.Default.ColourScheme.DeepCopy()
                : ColourScheme.DefaultColourScheme.DeepCopy();

            AudioSourceSettingsControlViewModel.InitializeAudioSourceInfo();

            SettingsChanged = false;
            Log.Info("Application settings loaded");
        }

        public void SaveApplicationSettings()
        {
            Settings.Default.ColourScheme = ColourSchemeSettingsControlViewModel.ColourScheme;
            Settings.Default.Save();

            AudioSourceSettingsControlViewModel.SaveAudioSourceInfo();

            SettingsChanged = false;
            Log.Info("Application settings saved");
        }

        private void CheckForViewModelSettingsChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case "SelectedAudioSource":
                case "MicrophoneVolume":
                    SettingsChanged = true;
                    break;
                case "ColourScheme":
                    ColourSchemeSettingsControlViewModel.ColourScheme.PropertyChanged += (csSender, csArgs) =>
                    {
                        if (args.PropertyName.Contains("Colour"))
                            SettingsChanged = true;
                    };
                    break;
            }
        }

        public void CloseCleanup()
        {
            AudioSourceSettingsControlViewModel.StopForClose();
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

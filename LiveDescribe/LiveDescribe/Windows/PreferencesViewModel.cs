using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Controls;
using LiveDescribe.Controls.UserControls;
using LiveDescribe.Extensions;
using LiveDescribe.Interfaces;
using LiveDescribe.Properties;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace LiveDescribe.Windows
{
    public class PreferencesViewModel : ViewModelBase, ISettingsViewModel
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Instance Variables
        private bool _settingsChanged;
        private readonly AudioSourceSettingsViewModel _audioSourceSettingsViewModel;
        private readonly ColourSchemeSettingsViewModel _colourSchemeSettingsViewModel;
        private readonly GeneralSettingsViewModel _generalSettingsViewModel;
        #endregion

        #region Events
        public event EventHandler RequestClose;
        #endregion

        #region Constructors
        public PreferencesViewModel()
        {
            _audioSourceSettingsViewModel = new AudioSourceSettingsViewModel();
            _audioSourceSettingsViewModel.PropertyChanged += CheckForViewModelSettingsChanged;

            _colourSchemeSettingsViewModel = new ColourSchemeSettingsViewModel();
            _colourSchemeSettingsViewModel.PropertyChanged += CheckForViewModelSettingsChanged;

            _generalSettingsViewModel = new GeneralSettingsViewModel();
            _generalSettingsViewModel.PropertyChanged += CheckForViewModelSettingsChanged;

            RetrieveApplicationSettings();

            InitCommands();
        }

        private void InitCommands()
        {
            AcceptChanges = new RelayCommand(
                canExecute: () => SettingsChanged,
                execute: SetApplicationSettings);

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

        public ColourSchemeSettingsViewModel ColourSchemeSettingsViewModel
        {
            get { return _colourSchemeSettingsViewModel; }
        }

        public AudioSourceSettingsViewModel AudioSourceSettingsViewModel
        {
            get { return _audioSourceSettingsViewModel; }
        }

        public GeneralSettingsViewModel GeneralSettingsViewModel
        {
            get { return _generalSettingsViewModel; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all relevant data from Application settings and sets the relevant properties with
        /// it.
        /// </summary>
        public void RetrieveApplicationSettings()
        {
            AudioSourceSettingsViewModel.RetrieveApplicationSettings();
            ColourSchemeSettingsViewModel.RetrieveApplicationSettings();
            GeneralSettingsViewModel.RetrieveApplicationSettings();

            SettingsChanged = false;
            Log.Info("Application settings loaded");
        }

        public void SetApplicationSettings()
        {
            AudioSourceSettingsViewModel.SetApplicationSettings();
            ColourSchemeSettingsViewModel.SetApplicationSettings();
            GeneralSettingsViewModel.SetApplicationSettings();

            Settings.Default.Save();
            SettingsChanged = false;
            Log.Info("Application settings saved");
        }

        private void CheckForViewModelSettingsChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case "SelectedAudioSource":
                case "MicrophoneVolume":
                case "AutoGenerateSpaces":
                    SettingsChanged = true;
                    break;
                case "ColourScheme":
                    ColourSchemeSettingsViewModel.ColourScheme.PropertyChanged += (csSender, csArgs) =>
                    {
                        if (args.PropertyName.Contains("Colour"))
                            SettingsChanged = true;
                    };
                    SettingsChanged = true;
                    break;
            }
        }

        public void CloseCleanup()
        {
            AudioSourceSettingsViewModel.StopForClose();
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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Extensions;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using LiveDescribe.ViewModel.Controls;
using System;
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
        private readonly AudioSourceSettingsControlViewModel _audioSourceSettingsControlViewModel;
        private readonly ColourSchemeSettingsControlViewModel _colourSchemeSettingsControlViewModel;
        #endregion

        #region Events
        public event EventHandler RequestClose;
        #endregion

        #region Constructors
        public PreferencesViewModel()
        {
            _audioSourceSettingsControlViewModel = new AudioSourceSettingsControlViewModel();
            _colourSchemeSettingsControlViewModel = new ColourSchemeSettingsControlViewModel();

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


        public ColourSchemeSettingsControlViewModel ColourSchemeSettingsControlViewModel
        {
            get { return _colourSchemeSettingsControlViewModel; }
        }

        public AudioSourceSettingsControlViewModel AudioSourceSettingsControlViewModel
        {
            get { return _audioSourceSettingsControlViewModel; }
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

            Log.Info("Application settings loaded");
        }

        public void SaveApplicationSettings()
        {
            Settings.Default.ColourScheme = ColourSchemeSettingsControlViewModel.ColourScheme;
            Settings.Default.Save();

            AudioSourceSettingsControlViewModel.SaveAudioSourceInfo();

            Log.Info("Application settings saved");
        }
        #endregion

        #region Event Invokation
        /// <summary>
        /// Makes a request to the view to close itself.
        /// </summary>
        private void OnRequestClose()
        {
            AudioSourceSettingsControlViewModel.StopForClose();

            var handler = RequestClose;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion
    }
}

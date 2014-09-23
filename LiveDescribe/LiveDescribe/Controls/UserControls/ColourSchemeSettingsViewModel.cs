using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Factories;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using LiveDescribe.Properties;
using LiveDescribe.Resources.UiStrings;

namespace LiveDescribe.Controls.UserControls
{
    public class ColourSchemeSettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        private ColourScheme _colourScheme;

        public ColourSchemeSettingsViewModel()
        {
            InitCommands();
        }

        private void InitCommands()
        {
            ResetColourScheme = new RelayCommand(
                canExecute: () => true,
                execute: () =>
                {
                    var result = MessageBoxFactory.ShowWarningQuestion(UiStrings.MessageBox_ResetColourSchemeWarning);

                    if (result == MessageBoxResult.Yes)
                        ColourScheme = ColourScheme.DefaultColourScheme.DeepCopy();
                });
        }

        public ICommand ResetColourScheme { get; private set; }

        public ColourScheme ColourScheme
        {
            set
            {
                _colourScheme = value;
                RaisePropertyChanged();
            }
            get { return _colourScheme; }
        }

        public void RetrieveApplicationSettings()
        {
            ColourScheme = Settings.Default.ColourScheme.DeepCopy();
        }

        public void SetApplicationSettings()
        {
            Settings.Default.ColourScheme = ColourScheme;
        }
    }
}

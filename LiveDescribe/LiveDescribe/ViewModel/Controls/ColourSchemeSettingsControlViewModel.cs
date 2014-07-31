using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Factories;
using LiveDescribe.Model;
using LiveDescribe.Resources.UiStrings;
using System.Windows;
using System.Windows.Input;

namespace LiveDescribe.ViewModel.Controls
{
    public class ColourSchemeSettingsControlViewModel : ViewModelBase
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        private ColourScheme _colourScheme;

        public ColourSchemeSettingsControlViewModel()
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
    }
}

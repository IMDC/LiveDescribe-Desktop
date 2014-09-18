using GalaSoft.MvvmLight;
using LiveDescribe.Interfaces;
using LiveDescribe.Properties;

namespace LiveDescribe.Controls.UserControls
{
    public class GeneralSettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private bool _autoGenerateSpaces;

        public bool AutoGenerateSpaces
        {
            set
            {
                _autoGenerateSpaces = value;
                RaisePropertyChanged();
            }
            get { return _autoGenerateSpaces; }
        }

        public void RetrieveApplicationSettings()
        {
            AutoGenerateSpaces = Settings.Default.AutoGenerateSpaces;
        }

        public void SetApplicationSettings()
        {
            Settings.Default.AutoGenerateSpaces = AutoGenerateSpaces;
        }
    }
}

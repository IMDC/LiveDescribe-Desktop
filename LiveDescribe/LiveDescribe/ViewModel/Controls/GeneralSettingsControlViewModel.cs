using GalaSoft.MvvmLight;
using LiveDescribe.Interfaces;
using LiveDescribe.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveDescribe.ViewModel.Controls
{
    public class GeneralSettingsControlViewModel : ViewModelBase, ISettingsViewModel
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

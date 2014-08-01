using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveDescribe.ViewModel.Controls
{
    public class GeneralSettingsControlViewModel : ViewModelBase
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
    }
}

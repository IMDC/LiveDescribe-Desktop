using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace LiveDescribe.View_Model
{
    public class MarkingSpacesControlViewModel : ViewModelBase
    {
        #region Instance Variables
        private String _beginHours;
        private String _beginMins;
        private String _beginSeconds;
        private String _beginMilliseconds;

        private String _endHours;
        private String _endMins;
        private String _endSeconds;
        private String _endMilliseconds;

        private SpacesViewModel _spacesViewModel;
        #endregion

        #region Constructors
        public MarkingSpacesControlViewModel(SpacesViewModel SpacesViewModel)
        {
            _spacesViewModel = SpacesViewModel;
        }
        #endregion

        #region Commands
        public RelayCommand CreateSpaceCommand { get; private set; }
        #endregion

        #region Binding Properties
        public String BeginHours
        {
            set
            {
                _beginHours = value;
                RaisePropertyChanged("BeginHours");
            }
            get { return _beginHours; }
        }

        public String BeginMins
        {
            set
            {
                _beginMins = value;
                RaisePropertyChanged("BeginMins");
            }
            get { return _beginMins; }
        }

        public String BeginSeconds
        {
            set
            {
                _beginSeconds = value;
                RaisePropertyChanged("BeginSeconds");
            }
            get { return _beginSeconds; }
        }

        public String BeginMilliseconds
        {
            set
            {
                _beginMilliseconds = value;
                RaisePropertyChanged("BeginMilliseconds");
            }
            get { return _beginMilliseconds; }
        }

        public String EndHours
        {
            set
            {
                _endHours = value;
                RaisePropertyChanged("EndHours");
            }
            get { return _endHours; }
        }

        public String EndMins
        {
            set
            {
                _endMins = value;
                RaisePropertyChanged("EndMins");
            }
            get { return _endMins; }
        }

        public String EndSeconds
        {
            set
            {
                _endSeconds = value;
                RaisePropertyChanged("BeginSeconds");
            }
            get { return _endSeconds; }
        }

        public String EndMilliseconds
        {
            set
            {
                _endMilliseconds = value;
                RaisePropertyChanged("BeginMilliseconds");
            }
            get { return _endMilliseconds; }
        }
        #endregion

        #region Binding Functions
        #endregion
    }
}

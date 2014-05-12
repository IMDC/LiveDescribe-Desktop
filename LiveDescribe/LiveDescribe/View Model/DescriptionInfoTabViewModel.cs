using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Model;

namespace LiveDescribe.View_Model
{
    class DescriptionInfoTabViewModel : ViewModelBase
    {
        private const int REGULAR_DESCRIPTION_TAB = 1;
        private const int EXTENDED_DESCRIPTION_TAB = 2;
        private DescriptionViewModel _descriptionViewModel;
        private Description _regularDescriptionSelectedInList;
        private Description _extendedDescriptionSelectedInList;
        private String _descriptionText;
        private int _tabSelectedIndex;

        public DescriptionInfoTabViewModel(DescriptionViewModel DescriptionViewModel)
        {
            this._descriptionViewModel = DescriptionViewModel;

            SaveDescriptionTextCommand = new RelayCommand(SaveDescriptionText, SaveDescriptionTextStateCheck);
            ClearDescriptionTextCommand = new RelayCommand(ClearDescriptionText, () => true);

            RegularDescriptionSelectedInList = null;
            ExtendedDescriptionSelectedInList = null;
            DescriptionText = null;
        }

        #region Commands

        public RelayCommand SaveDescriptionTextCommand { private set; get; }

        public RelayCommand ClearDescriptionTextCommand { private set; get; }

        #endregion

        #region Binding Properties
        /// <summary>
        /// Sets or gets the regular description selected in the tab control
        /// </summary>
        public Description RegularDescriptionSelectedInList
        {
            set
            {
                _regularDescriptionSelectedInList = value;
                RaisePropertyChanged("RegularDescriptionSelectedInList");
            }
            get
            {
                return _regularDescriptionSelectedInList;
            }
        }

        /// <summary>
        /// Sets or gets the extended description selected in the tab control
        /// </summary>
        public Description ExtendedDescriptionSelectedInList
        {
            set
            {
                _extendedDescriptionSelectedInList = value;
                RaisePropertyChanged("ExtendedDescriptionSelectedInList");
            }
            get
            {
                return _extendedDescriptionSelectedInList;
            }
        }

        /// <summary>
        /// The index of the current tab selected in the tab control
        /// </summary>
        public int TabSelectedIndex
        {
            set
            {
                _tabSelectedIndex = value;
                RaisePropertyChanged("TabSelectedIndex");
            }
            get
            {
                return _tabSelectedIndex;
            }
        }

        /// <summary>
        /// The description text to be saved to the selected description
        /// </summary>
        public string DescriptionText
        {
            set
            {
                _descriptionText = value;
                RaisePropertyChanged("DescriptionText");
            }
            get
            {
                return _descriptionText;
            }
        }

        /// <summary>
        /// Returns a list of all the extended descriptions to be viewed in the list view inside the tab control
        /// for extended descriptions
        /// </summary>
        public ObservableCollection<Description> ExtendedDescriptions
        {
            get
            {
                return _descriptionViewModel.ExtendedDescriptions;
            }
        }

        /// <summary>
        /// Returns a list of all the regular descriptions to be viewed in the list view inside the tab control
        /// for regular descriptions
        /// </summary>
        public ObservableCollection<Description> RegularDescriptions
        {
            get
            {
                return _descriptionViewModel.RegularDescriptions;
            }
        }
        #endregion

        #region Binding Functions

        /// <summary>
        /// Clears the description text
        /// </summary>
        public void ClearDescriptionText()
        {
            DescriptionText = "";
        }
        /// <summary>
        /// Depending on which tab is selected it will overwrite the appropriate description text
        /// </summary>
        public void SaveDescriptionText()
        {
            if (TabSelectedIndex == REGULAR_DESCRIPTION_TAB)
                RegularDescriptionSelectedInList.DescriptionText = DescriptionText;
            else if (TabSelectedIndex == EXTENDED_DESCRIPTION_TAB)
                ExtendedDescriptionSelectedInList.DescriptionText = DescriptionText;

        }
        #endregion

        #region State Checks
        /// <summary>
        /// Returns the state of whether the save text button can be shown or not
        /// </summary>
        /// <returns></returns>
        public bool SaveDescriptionTextStateCheck()
        {
            if (ExtendedDescriptionSelectedInList != null && TabSelectedIndex == EXTENDED_DESCRIPTION_TAB)
                return true;
            else if (RegularDescriptionSelectedInList != null && TabSelectedIndex == REGULAR_DESCRIPTION_TAB)
                return true;

            return false;
        }
        #endregion
    }
}

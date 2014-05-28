using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Model;
using LiveDescribe.Interfaces;

namespace LiveDescribe.View_Model
{
    class DescriptionInfoTabViewModel : ViewModelBase
    {
        #region Constants
        private const int SPACE_TAB = 0;
        private const int REGULAR_DESCRIPTION_TAB = 1;
        private const int EXTENDED_DESCRIPTION_TAB = 2;
        #endregion

        #region Instance Variables
        private DescriptionViewModel _descriptionViewModel;
        private SpacesViewModel _spacesViewModel;
        private Description _regularDescriptionSelectedInList;
        private Description _extendedDescriptionSelectedInList;
        private Space _spaceSelectedInList;
        private String _descriptionAndSpaceText;
        private int _tabSelectedIndex;
        #endregion

        public DescriptionInfoTabViewModel(DescriptionViewModel DescriptionViewModel, SpacesViewModel SpaceViewModel)
        {
            this._descriptionViewModel = DescriptionViewModel;
            this._spacesViewModel = SpaceViewModel;

            SaveDescriptionTextCommand = new RelayCommand(SaveDescriptionText, SaveDescriptionTextStateCheck);
            ClearDescriptionTextCommand = new RelayCommand(ClearDescriptionText, () => true);

            RegularDescriptionSelectedInList = null;
            ExtendedDescriptionSelectedInList = null;
            SpaceSelectedInList = null;
            DescriptionAndSpaceText = null;
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
                DeSelectPreviousDescscriptionsAndSpaces();

                _regularDescriptionSelectedInList = value;

                if (_regularDescriptionSelectedInList != null)
                    _regularDescriptionSelectedInList.IsSelected = true;

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
                DeSelectPreviousDescscriptionsAndSpaces();

                _extendedDescriptionSelectedInList = value;

                if (_extendedDescriptionSelectedInList != null)
                    _extendedDescriptionSelectedInList.IsSelected = true;

                RaisePropertyChanged("ExtendedDescriptionSelectedInList");
            }
            get
            {
                return _extendedDescriptionSelectedInList;
            }
        }

        /// <summary>
        /// Sets or gets the space selected in the tab control
        /// </summary>
        public Space SpaceSelectedInList
        {
            set
            {
                DeSelectPreviousDescscriptionsAndSpaces();

                _spaceSelectedInList = value;

                if (_spaceSelectedInList != null)
                    _spaceSelectedInList.IsSelected = true;
                RaisePropertyChanged("SpaceSelectedInList");
            }
            get
            {
                return _spaceSelectedInList;
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
        public string DescriptionAndSpaceText
        {
            set
            {
                _descriptionAndSpaceText = value;
                RaisePropertyChanged("DescriptionAndSpaceText");
            }
            get
            {
                return _descriptionAndSpaceText;
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

        /// <summary>
        /// returns a list of all the spaces to be viewed in the tab control
        /// </summary>
        public ObservableCollection<Space> Spaces
        {
            get
            {
                return _spacesViewModel.Spaces;
            }
        }

        #endregion

        #region Binding Functions

        /// <summary>
        /// Clears the description text
        /// </summary>
        public void ClearDescriptionText()
        {
            DescriptionAndSpaceText = "";
        }
        /// <summary>
        /// Depending on which tab is selected it will overwrite the appropriate description text
        /// </summary>
        public void SaveDescriptionText()
        {
            if (TabSelectedIndex == REGULAR_DESCRIPTION_TAB)
                RegularDescriptionSelectedInList.DescriptionText = DescriptionAndSpaceText;
            else if (TabSelectedIndex == EXTENDED_DESCRIPTION_TAB)
                ExtendedDescriptionSelectedInList.DescriptionText = DescriptionAndSpaceText;
            else if (TabSelectedIndex == SPACE_TAB)
                SpaceSelectedInList.SpaceText = DescriptionAndSpaceText;

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
            else if (SpaceSelectedInList != null && TabSelectedIndex == SPACE_TAB)
                return true;

            return false;
        }
        #endregion

        #region Helper Functions
        private void DeSelectPreviousDescscriptionsAndSpaces()
        {
            if (_regularDescriptionSelectedInList != null)
            {          
                _regularDescriptionSelectedInList.IsSelected = false;
                _regularDescriptionSelectedInList = null;
            }

            if (_extendedDescriptionSelectedInList != null)
            {
                _extendedDescriptionSelectedInList.IsSelected = false;
                _extendedDescriptionSelectedInList = null;
            }

            if (_spaceSelectedInList != null)
            {
                _spaceSelectedInList.IsSelected = false;
                _spaceSelectedInList = null;
            }
        }
        #endregion
    }
}

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Model;
using System;
using System.Collections.ObjectModel;

namespace LiveDescribe.View_Model
{
    public class DescriptionInfoTabViewModel : ViewModelBase
    {
        #region Constants
        private const int SpaceTab = 0;
        private const int RegularDescriptionTab = 1;
        private const int ExtendedDescriptionTab = 2;
        #endregion

        #region Instance Variables
        private readonly DescriptionViewModel _descriptionViewModel;
        private readonly SpacesViewModel _spacesViewModel;
        private Description _regularDescriptionSelectedInList;
        private Description _extendedDescriptionSelectedInList;
        private Space _spaceSelectedInList;
        private String _descriptionAndSpaceText;
        private int _tabSelectedIndex;
        #endregion

        public DescriptionInfoTabViewModel(DescriptionViewModel descriptionViewModel, SpacesViewModel spaceViewModel)
        {
            _descriptionViewModel = descriptionViewModel;
            _spacesViewModel = spaceViewModel;

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
                if (_regularDescriptionSelectedInList != null && value == null)
                    _regularDescriptionSelectedInList.IsSelected = false;


                if (_regularDescriptionSelectedInList != null)
                {
                    _regularDescriptionSelectedInList.IsSelected = false;
                    _regularDescriptionSelectedInList = null;
                }

                if (ExtendedDescriptionSelectedInList != null)
                {
                    ExtendedDescriptionSelectedInList.IsSelected = false;
                    ExtendedDescriptionSelectedInList = null;
                }

                if (SpaceSelectedInList != null)
                {
                    SpaceSelectedInList.IsSelected = false;
                    SpaceSelectedInList = null;
                }

                _regularDescriptionSelectedInList = value;

                if (_regularDescriptionSelectedInList != null && _regularDescriptionSelectedInList.IsSelected == false)
                    _regularDescriptionSelectedInList.IsSelected = true;

                RaisePropertyChanged();
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

                if (_extendedDescriptionSelectedInList != null && value == null)
                    _extendedDescriptionSelectedInList.IsSelected = false;


                if (RegularDescriptionSelectedInList != null)
                {
                    RegularDescriptionSelectedInList.IsSelected = false;
                    RegularDescriptionSelectedInList = null;
                }

                if (_extendedDescriptionSelectedInList != null)
                {
                    _extendedDescriptionSelectedInList.IsSelected = false;
                    _extendedDescriptionSelectedInList = null;
                }

                if (SpaceSelectedInList != null)
                {
                    SpaceSelectedInList.IsSelected = false;
                    SpaceSelectedInList = null;
                }

                _extendedDescriptionSelectedInList = value;

                if (_extendedDescriptionSelectedInList != null)
                    _extendedDescriptionSelectedInList.IsSelected = true;

                RaisePropertyChanged();
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
                if (_spaceSelectedInList != null && value == null)
                    _spaceSelectedInList.IsSelected = false;

                if (RegularDescriptionSelectedInList != null)
                {
                    RegularDescriptionSelectedInList.IsSelected = false;
                    RegularDescriptionSelectedInList = null;
                }

                if (ExtendedDescriptionSelectedInList != null)
                {
                    ExtendedDescriptionSelectedInList.IsSelected = false;
                    ExtendedDescriptionSelectedInList = null;
                }

                if (_spaceSelectedInList != null)
                {
                    _spaceSelectedInList.IsSelected = false;
                    _spaceSelectedInList = null;
                }

                _spaceSelectedInList = value;

                if (_spaceSelectedInList != null)
                    _spaceSelectedInList.IsSelected = true;
                RaisePropertyChanged();
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
                RaisePropertyChanged();
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
                RaisePropertyChanged();
            }
            get
            {
                return _descriptionAndSpaceText;
            }
        }

        /// <summary>
        /// Returns a list of all the extended descriptions to be viewed in the list view inside the
        /// tab control for extended descriptions
        /// </summary>
        public ObservableCollection<Description> ExtendedDescriptions
        {
            get
            {
                return _descriptionViewModel.ExtendedDescriptions;
            }
        }

        /// <summary>
        /// Returns a list of all the regular descriptions to be viewed in the list view inside the
        /// tab control for regular descriptions
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
            if (TabSelectedIndex == RegularDescriptionTab)
                RegularDescriptionSelectedInList.DescriptionText = DescriptionAndSpaceText;
            else if (TabSelectedIndex == ExtendedDescriptionTab)
                ExtendedDescriptionSelectedInList.DescriptionText = DescriptionAndSpaceText;
            else if (TabSelectedIndex == SpaceTab)
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
            if (ExtendedDescriptionSelectedInList != null && TabSelectedIndex == ExtendedDescriptionTab)
                return true;
            if (RegularDescriptionSelectedInList != null && TabSelectedIndex == RegularDescriptionTab)
                return true;
            if (SpaceSelectedInList != null && TabSelectedIndex == SpaceTab)
                return true;

            return false;
        }
        #endregion

        #region Helper Functions
        #endregion
    }
}

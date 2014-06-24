using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Factories;
using LiveDescribe.Model;
using LiveDescribe.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
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
        private Description _selectedRegularDescription;
        private Description _selectedExtendedDescription;
        private Space _selectedSpace;
        private String _descriptionAndSpaceText;
        private int _tabSelectedIndex;
        #endregion

        public DescriptionInfoTabViewModel(DescriptionViewModel descriptionViewModel, SpacesViewModel spaceViewModel)
        {
            _descriptionViewModel = descriptionViewModel;
            _spacesViewModel = spaceViewModel;

            SaveDescriptionTextCommand = new RelayCommand(SaveDescriptionText, SaveDescriptionTextStateCheck);
            ClearDescriptionTextCommand = new RelayCommand(ClearDescriptionText, () => true);

            RecordInSpace = new RelayCommand(
                canExecute: () => SelectedSpace != null,
                execute: () =>
                {
                    var viewModel = DialogShower.SpawnSpaceRecordingView(SelectedSpace, _descriptionViewModel.Project);

                    if (viewModel.DialogResult == true)
                        _descriptionViewModel.AddDescription(viewModel.Description);
                });

            SelectedRegularDescription = null;
            SelectedExtendedDescription = null;
            SelectedSpace = null;
            DescriptionAndSpaceText = null;
        }

        #region Commands

        public RelayCommand SaveDescriptionTextCommand { private set; get; }

        public RelayCommand ClearDescriptionTextCommand { private set; get; }

        public ICommand RecordInSpace { private set; get; }

        #endregion

        #region Binding Properties
        /// <summary>
        /// Sets or gets the regular description selected in the tab control
        /// </summary>
        public Description SelectedRegularDescription
        {
            set
            {
                if (_selectedRegularDescription != null && value == null)
                {
                    _selectedRegularDescription.IsSelected = false;
                    _selectedRegularDescription.PropertyChanged -= DescriptionFinishedPlaying;
                }

                // Unselect previous descriptions and spaces selected
                if (_selectedRegularDescription != null)
                {
                    _selectedRegularDescription.IsSelected = false;
                    _selectedRegularDescription = null;
                }

                if (SelectedExtendedDescription != null)
                {
                    SelectedExtendedDescription.IsSelected = false;
                    SelectedExtendedDescription = null;
                }

                if (SelectedSpace != null)
                {
                    SelectedSpace.IsSelected = false;
                    SelectedSpace = null;
                }

                _selectedRegularDescription = value;

                if (_selectedRegularDescription != null)
                {
                    _selectedRegularDescription.IsSelected = true;
                    TabSelectedIndex = RegularDescriptionTab;
                    DescriptionAndSpaceText = _selectedRegularDescription.DescriptionText;
                    _selectedRegularDescription.PropertyChanged += DescriptionFinishedPlaying;
                }

                RaisePropertyChanged();
            }
            get
            {
                return _selectedRegularDescription;
            }
        }

        /// <summary>
        /// Sets or gets the extended description selected in the tab control
        /// </summary>
        public Description SelectedExtendedDescription
        {
            set
            {
                if (_selectedExtendedDescription != null && value == null)
                {
                    _selectedExtendedDescription.IsSelected = false;
                    _selectedExtendedDescription.PropertyChanged -= DescriptionFinishedPlaying;
                }

                // Unselect previous descriptions and spaces selected
                if (SelectedRegularDescription != null)
                {
                    SelectedRegularDescription.IsSelected = false;
                    SelectedRegularDescription = null;
                }

                if (_selectedExtendedDescription != null)
                {
                    _selectedExtendedDescription.IsSelected = false;
                    _selectedExtendedDescription = null;
                }

                if (SelectedSpace != null)
                {
                    SelectedSpace.IsSelected = false;
                    SelectedSpace = null;
                }

                _selectedExtendedDescription = value;

                if (_selectedExtendedDescription != null)
                {
                    _selectedExtendedDescription.IsSelected = true;
                    //we don't want the text to appear in the textbox if a description is playing
                    DescriptionAndSpaceText = _selectedExtendedDescription.DescriptionText;
                    TabSelectedIndex = ExtendedDescriptionTab;
                    _selectedExtendedDescription.PropertyChanged += DescriptionFinishedPlaying;
                }

                RaisePropertyChanged();
            }
            get
            {
                return _selectedExtendedDescription;
            }
        }

        /// <summary>
        /// Sets or gets the space selected in the tab control
        /// </summary>
        public Space SelectedSpace
        {
            set
            {
                if (_selectedSpace != null && value == null)
                {
                    _selectedSpace.IsSelected = false;
                    _selectedSpace.PropertyChanged -= SelectedSpaceTextChanged;
                }

                // Unselect previous descriptions and spaces selected
                if (SelectedRegularDescription != null)
                {
                    SelectedRegularDescription.IsSelected = false;
                    SelectedRegularDescription = null;
                }

                if (SelectedExtendedDescription != null)
                {
                    SelectedExtendedDescription.IsSelected = false;
                    SelectedExtendedDescription = null;
                }

                if (_selectedSpace != null)
                {
                    _selectedSpace.IsSelected = false;
                    _selectedSpace = null;
                }

                _selectedSpace = value;

                if (_selectedSpace != null)
                {
                    _selectedSpace.IsSelected = true;
                    TabSelectedIndex = SpaceTab;
                    DescriptionAndSpaceText = _selectedSpace.SpaceText;
                    _selectedSpace.PropertyChanged += SelectedSpaceTextChanged;
                }
                RaisePropertyChanged();
            }
            get
            {
                return _selectedSpace;
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
                SelectedRegularDescription.DescriptionText = DescriptionAndSpaceText;
            else if (TabSelectedIndex == ExtendedDescriptionTab)
                SelectedExtendedDescription.DescriptionText = DescriptionAndSpaceText;
            else if (TabSelectedIndex == SpaceTab)
                SelectedSpace.SpaceText = DescriptionAndSpaceText;
        }

        public void DescriptionFinishedPlaying(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName.Equals("IsPlaying"))
            {
                var description = (Description)sender;
                if (!description.IsPlaying)
                {
                    description.IsSelected = false;
                    UnSelectDescriptionsAndSpaceSelectedInList();
                }
            }
        }

        public void SelectedSpaceTextChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName.Equals("SpaceText"))
            {
                var space = (Space)sender;
                DescriptionAndSpaceText = space.SpaceText;
            }
        }
        #endregion

        #region State Checks
        /// <summary>
        /// Returns the state of whether the save text button can be shown or not
        /// </summary>
        /// <returns></returns>
        public bool SaveDescriptionTextStateCheck()
        {
            if (SelectedExtendedDescription != null && TabSelectedIndex == ExtendedDescriptionTab)
                return true;
            if (SelectedRegularDescription != null && TabSelectedIndex == RegularDescriptionTab)
                return true;
            if (SelectedSpace != null && TabSelectedIndex == SpaceTab)
                return true;

            return false;
        }
        #endregion

        #region Helper Functions

        public void UnSelectDescriptionsAndSpaceSelectedInList()
        {
            SelectedRegularDescription = null;
            SelectedExtendedDescription = null;
            SelectedSpace = null;
        }
        #endregion
    }
}

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Factories;
using LiveDescribe.Interfaces;
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
        private readonly DescriptionCollectionViewModel _descriptionCollectionViewModel;
        private readonly SpaceCollectionViewModel _spaceCollectionViewModel;
        private Description _selectedRegularDescription;
        private Description _selectedExtendedDescription;
        private Space _selectedSpace;
        private String _descriptionAndSpaceText;
        private int _tabSelectedIndex;
        #endregion

        public DescriptionInfoTabViewModel(DescriptionCollectionViewModel descriptionCollectionViewModel, SpaceCollectionViewModel spaceViewModel)
        {
            _descriptionCollectionViewModel = descriptionCollectionViewModel;
            _spaceCollectionViewModel = spaceViewModel;

            SaveDescriptionTextCommand = new RelayCommand(SaveDescriptionText, SaveDescriptionTextStateCheck);
            ClearDescriptionTextCommand = new RelayCommand(ClearDescriptionText, () => true);

            RecordInSpace = new RelayCommand(
                canExecute: () => SelectedSpace != null,
                execute: () =>
                {
                    var viewModel = DialogShower.SpawnSpaceRecordingView(SelectedSpace, _descriptionCollectionViewModel.Project);

                    if (viewModel.DialogResult == true)
                        _descriptionCollectionViewModel.AddDescription(viewModel.Description);
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
            set { SetSelected(value); }
            get { return _selectedRegularDescription; }
        }

        /// <summary>
        /// Sets or gets the extended description selected in the tab control
        /// </summary>
        public Description SelectedExtendedDescription
        {
            set { SetSelected(value); }
            get { return _selectedExtendedDescription; }
        }

        /// <summary>
        /// Sets or gets the space selected in the tab control
        /// </summary>
        public Space SelectedSpace
        {
            set { SetSelected(value); }
            get { return _selectedSpace; }
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
            get { return _tabSelectedIndex; }
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
            get { return _descriptionAndSpaceText; }
        }

        /// <summary>
        /// Returns a list of all the extended descriptions to be viewed in the list view inside the
        /// tab control for extended descriptions
        /// </summary>
        public ObservableCollection<Description> ExtendedDescriptions
        {
            get { return _descriptionCollectionViewModel.ExtendedDescriptions; }
        }

        /// <summary>
        /// Returns a list of all the regular descriptions to be viewed in the list view inside the
        /// tab control for regular descriptions
        /// </summary>
        public ObservableCollection<Description> RegularDescriptions
        {
            get { return _descriptionCollectionViewModel.RegularDescriptions; }
        }

        /// <summary>
        /// returns a list of all the spaces to be viewed in the tab control
        /// </summary>
        public ObservableCollection<Space> Spaces
        {
            get { return _spaceCollectionViewModel.Spaces; }
        }

        #endregion

        #region Methods

        private void SetSelected(IDescribableInterval value)
        {
            UnselectAll();

            if (value != null)
            {
                if (value is Description)
                {
                    var d = (Description)value;
                    Select(ref d);
                }
                else if (value is Space)
                {
                    var space = (Space)value;
                    Select(ref space);
                }
                else
                    throw new NotImplementedException("Value type not implemented");
            }

            UpdateProperties();
        }

        private void Select(ref Description description)
        {
            description.IsSelected = true;
            //we don't want the text to appear in the textbox if a description is playing
            DescriptionAndSpaceText = description.Text;
            description.PropertyChanged += DescriptionFinishedPlaying;

            if (description.IsExtendedDescription)
            {
                _selectedExtendedDescription = description;
                TabSelectedIndex = ExtendedDescriptionTab;
            }
            else
            {
                _selectedRegularDescription = description;
                TabSelectedIndex = RegularDescriptionTab;
            }
        }

        private void Select(ref Space space)
        {
            space.IsSelected = true;
            DescriptionAndSpaceText = space.Text;
            space.PropertyChanged += SelectedSpaceTextChanged;

            TabSelectedIndex = SpaceTab;
            _selectedSpace = space;
        }

        private void Unselect(ref Description description)
        {
            description.IsSelected = false;
            description.PropertyChanged -= DescriptionFinishedPlaying;
            description = null;
        }

        private void Unselect(ref Space space)
        {
            space.IsSelected = false;
            space.PropertyChanged -= SelectedSpaceTextChanged;
            space = null;
        }

        private void UnselectAll()
        {
            if (_selectedRegularDescription != null)
                Unselect(ref _selectedRegularDescription);
            if (SelectedExtendedDescription != null)
                Unselect(ref _selectedExtendedDescription);
            if (_selectedSpace != null)
                Unselect(ref _selectedSpace);
        }

        private void UpdateProperties()
        {
            RaisePropertyChanged("SelectedRegularDescription");
            RaisePropertyChanged("SelectedExtendedDescription");
            RaisePropertyChanged("SelectedSpace");
        }

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
                SelectedRegularDescription.Text = DescriptionAndSpaceText;
            else if (TabSelectedIndex == ExtendedDescriptionTab)
                SelectedExtendedDescription.Text = DescriptionAndSpaceText;
            else if (TabSelectedIndex == SpaceTab)
                SelectedSpace.Text = DescriptionAndSpaceText;
        }

        public void DescriptionFinishedPlaying(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName.Equals("IsPlaying"))
            {
                var description = (Description)sender;
                if (!description.IsPlaying)
                {
                    description.IsSelected = false;
                    ClearSelection();
                }
            }
        }

        public void SelectedSpaceTextChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName.Equals("Text"))
            {
                var space = (Space)sender;
                DescriptionAndSpaceText = space.Text;
            }
        }

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

        /// <summary>
        /// Clears the selected item from the VM.
        /// </summary>
        public void ClearSelection()
        {
            UnselectAll();
            UpdateProperties();
        }
        #endregion
    }
}

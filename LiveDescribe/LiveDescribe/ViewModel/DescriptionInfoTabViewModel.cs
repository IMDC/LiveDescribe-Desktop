﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Extensions;
using LiveDescribe.Factories;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Model;
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
        private readonly ProjectManager _projectManager;
        private Description _selectedRegularDescription;
        private Description _selectedExtendedDescription;
        private Space _selectedSpace;
        private int _tabSelectedIndex;
        private IDescribableInterval _selectedItem;
        #endregion

        public DescriptionInfoTabViewModel(DescriptionCollectionViewModel descriptionCollectionViewModel,
            ProjectManager projectManager)
        {
            _descriptionCollectionViewModel = descriptionCollectionViewModel;
            _projectManager = projectManager;

            InitCommands();

            SelectedRegularDescription = null;
            SelectedExtendedDescription = null;
            SelectedSpace = null;
            _selectedItem = null;
        }

        private void InitCommands()
        {
            ClearSelectedItemText = new RelayCommand(
                canExecute: () => SelectedItem != null,
                execute: () => SelectedItem.Text = "");

            RecordInSpace = new RelayCommand(
                canExecute: () => SelectedSpace != null,
                execute: () =>
                {
                    var viewModel = DialogShower.SpawnSpaceRecordingView(SelectedSpace, _projectManager.Project);

                    if (viewModel.DialogResult == true)
                        _descriptionCollectionViewModel.AddDescription(viewModel.Description);
                });

            DeleteSelectedSpaceOrDescription = new RelayCommand(
                canExecute: () => SelectedSpace != null
                    || SelectedExtendedDescription != null
                    || SelectedRegularDescription != null,
                execute: () =>
                {
                    if (SelectedSpace != null)
                        SelectedSpace.DeleteSpaceCommand.ExecuteIfCan();

                    if (SelectedRegularDescription != null)
                        SelectedRegularDescription.DescriptionDeleteCommand.ExecuteIfCan();

                    if (SelectedExtendedDescription != null)
                        SelectedExtendedDescription.DescriptionDeleteCommand.ExecuteIfCan();
                }
            );
        }

        #region Commands

        public ICommand ClearSelectedItemText { private set; get; }

        public ICommand RecordInSpace { private set; get; }

        public ICommand DeleteSelectedSpaceOrDescription { private set; get; }
        #endregion

        #region Binding Properties

        public bool CanChangeText
        {
            get { return SelectedItem != null; }
        }

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

        public IDescribableInterval SelectedItem
        {
            set
            {
                _selectedItem = value;
                RaisePropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
            get { return _selectedItem; }
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
            get { return _projectManager.Spaces; }
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

            SelectedItem = description;
        }

        private void Select(ref Space space)
        {
            space.IsSelected = true;
            TabSelectedIndex = SpaceTab;
            _selectedSpace = space;
            SelectedItem = space;
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

            SelectedItem = null;
        }

        private void UpdateProperties()
        {
            RaisePropertyChanged("SelectedRegularDescription");
            RaisePropertyChanged("SelectedExtendedDescription");
            RaisePropertyChanged("SelectedSpace");
            RaisePropertyChanged("SelectedItem");
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
                    CommandManager.InvalidateRequerySuggested();
                }
            }
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

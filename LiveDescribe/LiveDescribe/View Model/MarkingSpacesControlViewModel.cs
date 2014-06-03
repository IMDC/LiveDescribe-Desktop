using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;

namespace LiveDescribe.View_Model
{
    public class MarkingSpacesControlViewModel : ViewModelBase
    {
        #region Instance Variables

        private bool _editingEnabled;
        private Space _selectedSpace;
        private DescriptionInfoTabViewModel _descriptionInfo;
        private ILiveDescribePlayer _player;
        #endregion

        #region Constructors
        public MarkingSpacesControlViewModel(DescriptionInfoTabViewModel descriptionInfo, ILiveDescribePlayer player)
        {
            _descriptionInfo = descriptionInfo;
            _descriptionInfo.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals("SpaceSelectedInList"))
                    SelectedSpace = descriptionInfo.SpaceSelectedInList;
            };

            _player = player;

            EditingEnabled = false;
        }
        #endregion

        #region Commands
        public RelayCommand CreateSpaceCommand { get; private set; }
        #endregion

        #region Binding Properties

        /// <summary>
        /// Determines whether or not the space times can be edited or not.
        /// </summary>
        public bool EditingEnabled
        {
            set
            {
                _editingEnabled = value;
                RaisePropertyChanged("EditingEnabled");
            }
            get { return _editingEnabled; }
        }

        public Space SelectedSpace
        {
            private set
            {
                //Clean up old Selected Space
                if (_selectedSpace != null)
                {
                    //Unsubscribe from old Selected Space
                    _selectedSpace.PropertyChanged -= SelectedSpaceOnPropertyChanged;
                }

                _selectedSpace = value;

                if (value != null)
                {
                    value.PropertyChanged += SelectedSpaceOnPropertyChanged;
                    EditingEnabled = true;
                }
                else
                    EditingEnabled = false;

                //Update the binded properties by notifying them
                RaisePropertyChanged("SelectedSpace_StartInVideo");
                RaisePropertyChanged("SelectedSpace_EndInVideo");

                RaisePropertyChanged("SelectedSpace");
            }
            get { return _selectedSpace; }
        }

        private void SelectedSpaceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("StartInVideo"))
                RaisePropertyChanged("SelectedSpace_StartInVideo");
            else if (e.PropertyName.Equals("EndInVideo"))
                RaisePropertyChanged("SelectedSpace_EndInVideo");
        }

        public double SelectedSpace_StartInVideo
        {
            set
            {
                //Set value only if it is valid, otherwise update view with old value.
                if (SelectedSpace != null
                    && !double.IsNaN(value)
                    && 0 <= value
                    && value <= SelectedSpace.EndInVideo - SpacesViewModel.MinSpaceLengthInMSecs)
                    SelectedSpace.StartInVideo = value;

                RaisePropertyChanged("SelectedSpace_StartInVideo");
            }
            get { return SelectedSpace != null ? SelectedSpace.StartInVideo : 0; }
        }

        public double SelectedSpace_EndInVideo
        {
            set
            {
                if (SelectedSpace != null
                    && !double.IsNaN(value)
                    && SelectedSpace.StartInVideo + SpacesViewModel.MinSpaceLengthInMSecs <= value
                    && value <= _player.DurationMilliseconds)
                    SelectedSpace.EndInVideo = value;

                RaisePropertyChanged("SelectedSpace_EndInVideo");
            }
            get { return SelectedSpace != null ? SelectedSpace.EndInVideo : 0; }
        }

        #endregion

        #region Binding Functions
        #endregion
    }
}

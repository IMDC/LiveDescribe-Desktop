using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using System.ComponentModel;
using System.Windows.Input;

namespace LiveDescribe.View_Model
{
    public class MarkingSpacesControlViewModel : ViewModelBase
    {
        #region Instance Variables

        private bool _editingEnabled;
        private Space _selectedSpace;
        private readonly DescriptionInfoTabViewModel _descriptionInfo;
        private readonly ILiveDescribePlayer _player;
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

            //Commands
            SetBeginToMarker = new RelayCommand(
                canExecute: () => EditingEnabled,
                execute: () => SelectedSpace_StartInVideo = _player.Position.TotalMilliseconds
                );

            SetEndToMarker = new RelayCommand(
                canExecute: () => EditingEnabled,
                execute: () => SelectedSpace_EndInVideo = _player.Position.TotalMilliseconds
                );
        }
        #endregion

        #region Commands
        /// <summary>
        /// Command that Sets the StartInVideoTime to the marker time when executed.
        /// </summary>
        public ICommand SetBeginToMarker { private set; get; }
        /// <summary>
        /// Command that sets the EndInVideoTime to the marker time when executed.
        /// </summary>
        public ICommand SetEndToMarker { private set; get; }
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
                RaisePropertyChanged();
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

                RaisePropertyChanged();
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

                RaisePropertyChanged();
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

                RaisePropertyChanged();
            }
            get { return SelectedSpace != null ? SelectedSpace.EndInVideo : 0; }
        }

        #endregion

        #region Binding Functions
        #endregion
    }
}

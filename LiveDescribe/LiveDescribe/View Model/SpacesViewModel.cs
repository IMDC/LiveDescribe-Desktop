using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using LiveDescribe.Model;
using LiveDescribe.Events;
using System.Windows.Input;

namespace LiveDescribe.View_Model
{
    public class SpacesViewModel : ViewModelBase
    {

        public const double MAX_SPACE_LENGTH_IN_MILLISECONDS = 333;

        #region Instance Variables
        private ObservableCollection<Space> _spaces;
        
        private String _beginHours;
        private String _beginMins;
        private String _beginSeconds;
        private String _beginMilliseconds;

        private String _endHours;
        private String _endMins;
        private String _endSeconds;
        private String _endMilliseconds;
        #endregion

        #region Event Handlers
        public EventHandler<SpaceEventArgs> SpaceAddedEvent;
        #endregion

        #region Constructors
        public SpacesViewModel()
        {
            Spaces = new ObservableCollection<Space>();
            AddSpaceCommand = new RelayCommand(AddSpace, () => true);
        }
        #endregion

        #region Commands
        /// <summary>
        /// Command used for when a space is added
        /// </summary>
        public RelayCommand AddSpaceCommand { get; private set; }
        #endregion

        #region Binding Properties
        /// <summary>
        /// List that represents all the Spaces
        /// </summary>
        public ObservableCollection<Space> Spaces
        {
            set
            {
                _spaces = value;
                RaisePropertyChanged("Spaces");
            }
            get { return _spaces; }
        }

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
        /// <summary>
        /// Method that gets called when adding a space
        /// </summary>
        public void AddSpace()
        {
            this.AddSpace(new Space());
        }

        public void AddSpace(Space space)
        {
            EventHandler<SpaceEventArgs> handler = SpaceAddedEvent;
            if (handler != null) handler(this, new SpaceEventArgs(space));
            Spaces.Add(space);
            SetupEventsOnSpace(space);
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Setup all the events on space that don't require any info from the UI
        /// </summary>
        /// <param name="space">The space that the events are setup on</param>
        private void SetupEventsOnSpace(Space space)
        {
            space.SpaceDeleteEvent += (sender, e) =>
                {
                    Spaces.Remove(space);
                };
        }

        public void CloseSpacesViewModel()
        {
            Spaces.Clear();
        }
        #endregion
    }
}

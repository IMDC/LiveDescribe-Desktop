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
        #region Instance Variables
        private ObservableCollection<Space> _spaces;
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
        public RelayCommand AddSpaceCommand { get; private set; }
        #endregion

        #region Binding Properties
        public ObservableCollection<Space> Spaces
        {
            set
            {
                _spaces = value;
                RaisePropertyChanged("Spaces");
            }
            get
            {
                return _spaces;
            }
        }
        #endregion

        #region Binding Functions
        /// <summary>
        /// Method that gets called when adding a space
        /// </summary>
        public void AddSpace()
        {
            Space space = new Space();
            EventHandler<SpaceEventArgs> handler = SpaceAddedEvent;
            if (handler != null) handler(this, new SpaceEventArgs(space));
            Spaces.Add(space);
            //Setup events for the space here that do not concern UI
            
        }
        #endregion

        #region Helper Methods
        public void CloseSpacesViewModel()
        {
            Spaces = null;
            Spaces = new ObservableCollection<Space>();
        }
        #endregion
    }
}

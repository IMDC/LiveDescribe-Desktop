using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using LiveDescribe.Model;
using System.Windows.Input;

namespace LiveDescribe.View_Model
{
    public class SpacesViewModel : ViewModelBase
    {
        #region Instance Variables
        private ObservableCollection<Space> _spaces;
        #endregion

        #region Event Handlers
        public EventHandler SpaceAddedEvent;
        #endregion

        #region Constructors
        public SpacesViewModel()
        {
            Spaces = new ObservableCollection<Space>();
        }
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
        public void AddSpace(Space space)
        {
            Spaces.Add(space);
            EventHandler handler = SpaceAddedEvent;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        #endregion
    }
}

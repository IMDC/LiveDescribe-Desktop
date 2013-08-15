using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Microsoft.TeamFoundation.MVVM;
using System.ComponentModel;

namespace LiveDescribe.View_Model
{
    class MainControl : ViewModelBase, INotifyPropertyChanged
    {
        #region Instance Variables
        private VideoControl _videocontrol;
        #endregion

        #region Constructors
        public MainControl()
        {
            
        }
        #endregion

        #region Binding Functions

        #endregion

        #region Commands
 
        #endregion

        #region Binding Properties
        public VideoControl VideoControl
        {
            set
            {
                _videocontrol = value;
            }
            get
            {
                return _videocontrol;
            }
        }
        #endregion
    }
}

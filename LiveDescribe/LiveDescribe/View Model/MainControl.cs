using Microsoft.TeamFoundation.MVVM;
using System.ComponentModel;

namespace LiveDescribe.View_Model
{
    class MainControl : ViewModelBase
    {
        #region Instance Variables
        private VideoControl _videocontrol;
        #endregion

        #region Constructors
        public MainControl()
        {
            _videocontrol = new VideoControl();
        }
        #endregion

        #region Binding Functions

        #endregion

        #region Commands
 
        #endregion

        #region Binding Properties
        public VideoControl VideoControl
        {
            get
            {
                return _videocontrol;
            }
        }
        #endregion
    }
}

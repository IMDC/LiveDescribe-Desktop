using GalaSoft.MvvmLight;
using LiveDescribe.Interfaces;

namespace LiveDescribe.Controls.Canvases
{
    public class NumberLineViewModel : ViewModelBase
    {
        #region Fields
        private readonly ILiveDescribePlayer _player;
        #endregion

        #region Constructor
        public NumberLineViewModel(ILiveDescribePlayer player)
        {
            _player = player;
        }
        #endregion

        #region Properties
        public ILiveDescribePlayer Player
        {
            get { return _player; }
        }
        #endregion
    }
}

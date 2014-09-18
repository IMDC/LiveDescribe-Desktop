using GalaSoft.MvvmLight;
using LiveDescribe.Interfaces;

namespace LiveDescribe.ViewModel
{
    public class NumberLineCanvasViewModel : ViewModelBase
    {
        #region Fields
        private readonly ILiveDescribePlayer _player;
        #endregion

        #region Constructor
        public NumberLineCanvasViewModel(ILiveDescribePlayer player)
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

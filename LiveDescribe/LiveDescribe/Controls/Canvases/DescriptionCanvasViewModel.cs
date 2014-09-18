using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Model;

namespace LiveDescribe.Controls.Canvases
{
    public class DescriptionCanvasViewModel : ViewModelBase
    {
        private readonly ProjectManager _projectManager;
        private readonly UndoRedoManager _undoRedoManager;
        private LiveDescribeVideoStates _currentVideoState;
        private readonly ILiveDescribePlayer _player;

        public DescriptionCanvasViewModel(ILiveDescribePlayer videoMedia, ProjectManager projectManager,
            UndoRedoManager undoRedoManager)
        {
            _projectManager = projectManager;
            _undoRedoManager = undoRedoManager;
            _player = videoMedia;

            videoMedia.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals("CurrentState"))
                    CurrentVideoState = videoMedia.CurrentState;
            };
        }

        #region Binding Properties
        public ObservableCollection<Description> AllDescriptions
        {
            get { return _projectManager.AllDescriptions; }
        }

        public LiveDescribeVideoStates CurrentVideoState
        {
            set
            {
                _currentVideoState = value;
                RaisePropertyChanged();
            }
            get { return _currentVideoState; }
        }

        public ILiveDescribePlayer Player
        {
            get { return _player; }
        }

        public UndoRedoManager UndoRedoManager
        {
            get { return _undoRedoManager; }
        }

        #endregion
    }
}
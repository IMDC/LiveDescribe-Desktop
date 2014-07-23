using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    public class DescriptionCanvasViewModel : ViewModelBase
    {
        private readonly ProjectManager _projectManager;
        private LiveDescribeVideoStates _currentVideoState;
        #region Events
        public EventHandler<MouseEventArgs> DescriptionCanvasMouseUpEvent;
        public EventHandler<MouseEventArgs> DescriptionCanvasMouseMoveEvent;
        public EventHandler<MouseEventArgs> DescriptionCanvasMouseDownEvent;
        #endregion

        public DescriptionCanvasViewModel(ILiveDescribePlayer videoMedia, ProjectManager projectManager)
        {
            _projectManager = projectManager;
            DescriptionCanvasMouseUpCommand = new RelayCommand<MouseEventArgs>(DescriptionCanvasMouseUp, param => true);
            DescriptionCanvasMouseMoveCommand = new RelayCommand<MouseEventArgs>(DescriptionCanvasMouseMove, param => true);
            DescriptionCanvasMouseDownCommand = new RelayCommand<MouseEventArgs>(DescriptionCanvasMouseDown, param => true);
            videoMedia.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals("CurrentState"))
                    CurrentVideoState = videoMedia.CurrentState;
            };
        }

        #region Commands
        public RelayCommand<MouseEventArgs> DescriptionCanvasMouseUpCommand { private set; get; }
        public RelayCommand<MouseEventArgs> DescriptionCanvasMouseMoveCommand { private set; get; }
        public RelayCommand<MouseEventArgs> DescriptionCanvasMouseDownCommand { private set; get; }
        #endregion

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
        #endregion

        #region Binding Functions
        public void DescriptionCanvasMouseUp(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = DescriptionCanvasMouseUpEvent;
            if (handler != null) handler(this, e);
        }

        public void DescriptionCanvasMouseMove(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = DescriptionCanvasMouseMoveEvent;
            if (handler != null) handler(this, e);
        }

        public void DescriptionCanvasMouseDown(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = DescriptionCanvasMouseDownEvent;
            if (handler != null) handler(this, e);
        }
        #endregion
    }
}
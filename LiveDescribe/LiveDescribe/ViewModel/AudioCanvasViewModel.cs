using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Events;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    class AudioCanvasViewModel : ViewModelBase
    {
        private readonly ProjectManager _projectManager;
        private LiveDescribeVideoStates _currentState;

        #region Events
        public EventHandler<MouseEventArgs> AudioCanvasMouseDownEvent;
        public EventHandler<MouseEventArgs> AudioCanvasMouseRightButtonDownEvent;
        /// <summary>
        /// Requests to a handler what to set the StartInVideo and EndInVideo time values for the
        /// given space.
        /// </summary>
        public event EventHandler<EventArgs<Space>> RequestSpaceTime;
        #endregion

        public AudioCanvasViewModel(ILiveDescribePlayer mediaPlayer, ProjectManager projectManager, UndoRedoManager _undoManager)
        {
            _projectManager = projectManager;

            AudioCanvasMouseDownCommand = new RelayCommand<MouseEventArgs>(AudioCanvasMouseDown, param => true);
            AudioCanvasMouseRightButtonDownCommand = new RelayCommand<MouseEventArgs>(AudioCanvasMouseRightButtonDown, param => true);
            mediaPlayer.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals("CurrentState"))
                    CurrentVideoState = mediaPlayer.CurrentState;
            };

            GetNewSpaceTime = new RelayCommand(
            canExecute: () => CurrentVideoState != LiveDescribeVideoStates.VideoNotLoaded,
            execute: () =>
            {
                var s = new Space();
                OnRequestSpaceTime(s);
                Spaces.Add(s);
                _undoManager.InsertSpaceForInsert(Spaces, s);                
            });
        }

        #region Commands
        public RelayCommand<MouseEventArgs> AudioCanvasMouseDownCommand { private set; get; }
        public RelayCommand<MouseEventArgs> AudioCanvasMouseRightButtonDownCommand { private set; get; }
        public ICommand GetNewSpaceTime { get; private set; }
        #endregion

        #region Binding Properties
        public ObservableCollection<Space> Spaces
        {
            get { return _projectManager.Spaces; }
        }

        public LiveDescribeVideoStates CurrentVideoState
        {
            set
            {
                _currentState = value;
                RaisePropertyChanged();
            }
            get { return _currentState; }
        }
        #endregion

        #region Binding Functions

        private void AudioCanvasMouseDown(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = AudioCanvasMouseDownEvent;
            if (handler != null) handler(this, e);
        }

        private void AudioCanvasMouseRightButtonDown(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = AudioCanvasMouseRightButtonDownEvent;
            if (handler != null) handler(this, e);
        }
        #endregion

        #region Event Invokation
        private void OnRequestSpaceTime(Space s)
        {
            var handler = RequestSpaceTime;
            if (handler != null) handler(this, s);
        }
        #endregion
    }
}

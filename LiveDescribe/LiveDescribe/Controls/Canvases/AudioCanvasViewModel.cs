using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Events;
using LiveDescribe.Interfaces;
using LiveDescribe.Managers;
using LiveDescribe.Model;

namespace LiveDescribe.Controls.Canvases
{
    class AudioCanvasViewModel : ViewModelBase
    {
        private readonly ProjectManager _projectManager;
        private readonly UndoRedoManager _undoRedoManager;
        private readonly ILiveDescribePlayer _player;
        private LiveDescribeVideoStates _currentState;
        private Waveform _waveform;

        #region Events
        /// <summary>
        /// Requests to a handler what to set the StartInVideo and EndInVideo time values for the
        /// given space.
        /// </summary>
        public event EventHandler<EventArgs<Space>> RequestSpaceTime;
        #endregion

        public AudioCanvasViewModel(ILiveDescribePlayer mediaPlayer, ProjectManager projectManager,
            UndoRedoManager undoRedoManager)
        {
            _projectManager = projectManager;
            _undoRedoManager = undoRedoManager;
            _player = mediaPlayer;

            //TODO: Just refer to MediaPlayer?
            mediaPlayer.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals("CurrentState"))
                    CurrentVideoState = mediaPlayer.CurrentState;
            };

            _projectManager.ProjectLoaded += (sender, args) => Waveform = args.Value.Waveform;

            GetNewSpaceTime = new RelayCommand(
                canExecute: () => CurrentVideoState != LiveDescribeVideoStates.VideoNotLoaded,
                execute: () =>
                {
                    var s = new Space();
                    OnRequestSpaceTime(s);
                    projectManager.AddSpaceAndTrackForUndo(s);
                });
        }

        #region Commands
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

        public Waveform Waveform
        {
            get { return _waveform; }
            set
            {
                _waveform = value;
                RaisePropertyChanged();
            }
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

        #region Event Invokation
        private void OnRequestSpaceTime(Space s)
        {
            var handler = RequestSpaceTime;
            if (handler != null) handler(this, s);
        }
        #endregion
    }
}

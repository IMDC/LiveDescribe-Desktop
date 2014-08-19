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
        private readonly ILiveDescribePlayer _player;
        private LiveDescribeVideoStates _currentState;
        private Waveform _waveform;

        #region Events
        public EventHandler<MouseEventArgs> AudioCanvasMouseDownEvent;
        public EventHandler<MouseEventArgs> AudioCanvasMouseRightButtonDownEvent;
        /// <summary>
        /// Requests to a handler what to set the StartInVideo and EndInVideo time values for the
        /// given space.
        /// </summary>
        public event EventHandler<EventArgs<Space>> RequestSpaceTime;
        #endregion

        public AudioCanvasViewModel(ILiveDescribePlayer mediaPlayer, ProjectManager projectManager)
        {
            _projectManager = projectManager;
            _player = mediaPlayer;

            AudioCanvasMouseDownCommand = new RelayCommand<MouseEventArgs>(AudioCanvasMouseDown, param => true);
            AudioCanvasMouseRightButtonDownCommand = new RelayCommand<MouseEventArgs>(AudioCanvasMouseRightButtonDown, param => true);

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

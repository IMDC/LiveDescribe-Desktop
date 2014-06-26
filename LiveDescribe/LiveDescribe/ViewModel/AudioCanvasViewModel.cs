using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Controls;
using LiveDescribe.Interfaces;
using LiveDescribe.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    class AudioCanvasViewModel : ViewModelBase
    {
        private readonly SpaceCollectionViewModel _spaceCollectionViewModel;

        private LiveDescribeVideoStates _currentState;

        #region Events

        public EventHandler<MouseEventArgs> AudioCanvasMouseDownEvent;
        public EventHandler<MouseEventArgs> AudioCanvasMouseUpEvent;
        public EventHandler<MouseEventArgs> AudioCanvasMouseMoveEvent;
        public EventHandler<MouseEventArgs> AudioCanvasMouseRightButtonDownEvent;
        #endregion

        public AudioCanvasViewModel(SpaceCollectionViewModel spaceCollectionViewModel, ILiveDescribePlayer mediaPlayer)
        {
            _spaceCollectionViewModel = spaceCollectionViewModel;

            AudioCanvasMouseDownCommand = new RelayCommand<MouseEventArgs>(AudioCanvasMouseDown, param => true);
            AudioCanvasMouseUpCommand = new RelayCommand<MouseEventArgs>(AudioCanvasMouseUp, param => true);
            AudioCanvasMouseMoveCommand = new RelayCommand<MouseEventArgs>(AudioCanvasMouseMove, param => true);
            AudioCanvasMouseRightButtonDownCommand = new RelayCommand<MouseEventArgs>(AudioCanvasMouseRightButtonDown, param => true);
            mediaPlayer.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName.Equals("CurrentState"))
                    CurrentVideoState = mediaPlayer.CurrentState;
            };
        }

        #region Commands
        public RelayCommand<MouseEventArgs> AudioCanvasMouseDownCommand { private set; get; }
        public RelayCommand<MouseEventArgs> AudioCanvasMouseUpCommand { private set; get; }
        public RelayCommand<MouseEventArgs> AudioCanvasMouseMoveCommand { private set; get; }
        public RelayCommand<MouseEventArgs> AudioCanvasMouseRightButtonDownCommand { private set; get; }
        #endregion

        #region Binding Properties
        public ObservableCollection<Space> Spaces
        {
            get { return _spaceCollectionViewModel.Spaces; }
        }

        public SpaceCollectionViewModel SpaceCollectionViewModel
        {
            get { return _spaceCollectionViewModel; }
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

        public void AudioCanvasMouseDown(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = AudioCanvasMouseDownEvent;
            if (handler != null) handler(this, e);
        }
        public void AudioCanvasMouseUp(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = AudioCanvasMouseUpEvent;
            if (handler != null) handler(this, e);
        }
        public void AudioCanvasMouseMove(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = AudioCanvasMouseMoveEvent;
            if (handler != null) handler(this, e);
        }
        public void AudioCanvasMouseRightButtonDown(MouseEventArgs e)
        {
            EventHandler<MouseEventArgs> handler = AudioCanvasMouseRightButtonDownEvent;
            if (handler != null) handler(this, e);
        }
        #endregion
    }
}

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveDescribe.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LiveDescribe.ViewModel
{
    class AudioCanvasViewModel : ViewModelBase
    {
        private readonly SpacesViewModel _spacesViewModel;

        #region Events

        public EventHandler<MouseEventArgs> AudioCanvasMouseDownEvent;
        public EventHandler<MouseEventArgs> AudioCanvasMouseUpEvent;
        public EventHandler<MouseEventArgs> AudioCanvasMouseMoveEvent;
        public EventHandler<MouseEventArgs> AudioCanvasMouseRightButtonDownEvent;
        #endregion

        public AudioCanvasViewModel(SpacesViewModel spacesViewModel)
        {
            _spacesViewModel = spacesViewModel;

            AudioCanvasMouseDownCommand = new RelayCommand<MouseEventArgs>(AudioCanvasMouseDown, param => true);
            AudioCanvasMouseUpCommand = new RelayCommand<MouseEventArgs>(AudioCanvasMouseUp, param => true);
            AudioCanvasMouseMoveCommand = new RelayCommand<MouseEventArgs>(AudioCanvasMouseMove, param => true);
            AudioCanvasMouseRightButtonDownCommand = new RelayCommand<MouseEventArgs>(AudioCanvasMouseRightButtonDown, param => true);
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
            get { return _spacesViewModel.Spaces; }
        }

        public SpacesViewModel SpacesViewModel
        {
            get { return _spacesViewModel; }
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

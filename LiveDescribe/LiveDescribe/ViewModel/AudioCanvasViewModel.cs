using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace LiveDescribe.ViewModel
{
    class AudioCanvasViewModel : ViewModelBase
    {

        private readonly SpacesViewModel _spacesViewModel;

        #region Events

        public EventHandler AudioCanvasMouseDownEvent;
        public EventHandler AudioCanvasMouseUpEvent;
        public EventHandler AudioCanvasMouseMoveEvent;
        public EventHandler AudioCanvasMouseRightButtonDownEvent;
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

        #region Binding Functions

        public void AudioCanvasMouseDown(MouseEventArgs e)
        {
            EventHandler handler = AudioCanvasMouseDownEvent;
            if (handler != null) handler(this, e);
        }
        public void AudioCanvasMouseUp(MouseEventArgs e)
        {
            EventHandler handler = AudioCanvasMouseUpEvent;
            if (handler != null) handler(this, e);
        }
        public void AudioCanvasMouseMove(MouseEventArgs e)
        {
            EventHandler handler = AudioCanvasMouseMoveEvent;
            if (handler != null) handler(this, e);
        }
        public void AudioCanvasMouseRightButtonDown(MouseEventArgs e)
        {
            EventHandler handler = AudioCanvasMouseRightButtonDownEvent;
            if (handler != null) handler(this, e);
        }
        #endregion

        public SpacesViewModel SpacesViewModel
        {
            get { return _spacesViewModel; }
        }
    }
}
